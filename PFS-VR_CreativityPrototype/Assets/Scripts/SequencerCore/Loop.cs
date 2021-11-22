using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Loop : MonoBehaviour
{
    public GameObject loadNewMenu;
    public Playhead playhead;
    public Metronome metronome;
    public LoopGrid grid;
    public LoopBackground background;
    public SongTrack songTrack;
    public GameObject trackPrototype;
    public int nrOfInitialTracks = 2;
    public Transform firstTrackSpawnPosition;
    public Transform teleportToMeTarget;
    public float trackSpacing = 0.05f;

    public GameObject addTrackDialog;
    public SampleDefinitionSelector sampleSelector;

    public string loopName;
    private int exportVersion;
    public event System.Action<string> OnNameChanged;

    public Color[] trackColors;

    public List<Track> tracks = new List<Track>();
    private Vector3 NextTrackSpawnPos {
        get
        {
            Transform initialSpawnPoint = firstTrackSpawnPosition;
            Vector3 spawnPos = firstTrackSpawnPosition.position;
            spawnPos -= firstTrackSpawnPosition.up * tracks.Count * trackSpacing;
            return spawnPos;
        }
    }

    private int trackIdCtr = 0;

    private int _durationInBars = 2;
    public int durationInBars { get { return _durationInBars; }
        set {
            _durationInBars = value;
            foreach (Track track in tracks)
            {
                track.durationInBars = durationInBars;
            }
            relayoutLoopUIComponents();
        }
    }
    public float length { get { return durationInBars * BPMManager.instance.barLength; } }
    public float duration { get { return BPMManager.instance.distanceToTime(length); } }

    public BasicUIDialog warningDialog;
    public GameObject exportSuccessToast;

    bool isPlaying = false;
    double _playStartDspTime = GlobalConstants.InvalidTime;

    private static int loopCtr = 1;
    private bool hasDefaultName = false;

    private bool snappingEnabled = true;

    private bool wasSaved = false;
    public bool WasSaved => wasSaved;

    public Vector3 getPlaySpaceTargetPositionForTeleportation()
    {
        //Perform a raycast from the target down, to find the actual ground
        RaycastHit hit;
        if (Physics.Raycast(teleportToMeTarget.position, Vector3.down, out hit, Mathf.Infinity, GlobalConstants.groundLayerMask))
        {
            return hit.point;
        } else
        {
            Debug.LogWarning("Couldn't find ground beneath loop. Teleporting into empty space...");

            //project target pos onto 0 height as a fallback
            Vector3 targetPosOnFloor = Vector3.zero;
            targetPosOnFloor.x = teleportToMeTarget.position.x;
            targetPosOnFloor.z = teleportToMeTarget.position.z;

            return targetPosOnFloor;
        }
    }

    public Vector3 getPlaySpaceLookAtRotationForTeleportation()
    {
        return teleportToMeTarget.rotation.eulerAngles;
    }

    public void toggleMetronome()
    {
        metronome.muted = !metronome.muted;
        UsageLogger.log(metronome.muted ? UserAction.LOOP_METRONOME_TURNED_OFF : UserAction.LOOP_METRONOME_TURNED_ON);
    }

    public void toggleSnapping()
    {
        snappingEnabled = !snappingEnabled;

        if (snappingEnabled)
        {
            UsageLogger.log(UserAction.LOOP_SNAP_TO_SUBBEATS_TURNED_ON);
            tracks.ForEach(t => t.enableSnapping());
        } else
        {
            UsageLogger.log(UserAction.LOOP_SNAP_TO_SUBBEATS_TURNED_OFF);
            tracks.ForEach(t => t.disableSnapping());
        }
    }

    public void loadDefaultLoop()
    {
        if (!WasSaved)
        {
            showNotSavedWarning();
        } else
        {
            LoopManager.instance.loadDefaultLoop();
        }
    }

    public void Start()
    {
        if (loopName == null || loopName == "")
        {
            loopName = "Loop" + loopCtr;
            if (OnNameChanged != null) OnNameChanged(loopName);
            loopCtr++;
            hasDefaultName = true;
        }

        background.trackHeight = trackSpacing;
        grid.trackHeight = trackSpacing;
        playhead.trackHeight = trackSpacing;
        Vector3 initialNextTrackSpawnPos = firstTrackSpawnPosition.position;

        //add default tracks if no tracks were added before Start was called
        if (tracks.Count == 0)
        {
            for (int i = 0; i < nrOfInitialTracks; i++)
            {
                Color c = trackColors[i % trackColors.Length];
                addTrack(SampleDefinition.makeRandomSample("Random " + i), c);
            }
        } else
        {
            //lay out tracks properly because they didn't get transforms applied during instantiation
            updateTrackPositions();
        }

        loadNewMenu.SetActive(ExperimentManager.instance.currentExperimentStage == ExperimentStage.EXPLORATION_01
                                || ExperimentManager.instance.currentExperimentStage == ExperimentStage.EXPLORATION_02
                                || ExperimentManager.instance.currentExperimentStage == ExperimentStage.INTRO_PFS_CONDUCTOR
                                || ExperimentManager.instance.currentExperimentStage == ExperimentStage.INTRO_PFS_USER
                                );

        relayoutLoopUIComponents(); //initialize layout

        BPMManager.instance.OnDistancePerBeatChange += this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatsPerBarChange += this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatTypeChange += this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatSubdivisionsChange += this.handleBeatSubdivisionChange;
    }

    public void OnDestroy()
    {
        BPMManager.instance.OnDistancePerBeatChange -= this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatsPerBarChange -= this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatTypeChange -= this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatSubdivisionsChange -= this.handleBeatSubdivisionChange;
    }

    public void Play()
    {
        isPlaying = true;
        _playStartDspTime = AudioSettings.dspTime;
        foreach (Track track in tracks)
        {
            track.Play();
        }
        songTrack.Play();
        playhead.startMoving();
        metronome.StartTicking();
        UsageLogger.log(UserAction.LOOP_STARTED);
    }

    public void Stop()
    {
        if (isPlaying)
        {
            isPlaying = false;
            _playStartDspTime = GlobalConstants.InvalidTime;
            foreach (Track track in tracks)
            {
                track.Stop();
            }
        }
        songTrack.Stop();
        playhead.stopMoving();
        metronome.StopTicking();
        UsageLogger.log(UserAction.LOOP_STOPPED);
    }

    public void startNameChangeFlow()
    {
        KeyboardManager.instance.keyboard.show(loopName, "Edit Loop name");
        KeyboardManager.instance.keyboard.OnTextCommitted += this.handleNameChangeDialogCommit;
        KeyboardManager.instance.keyboard.OnKeyboardHide += this.handleNameChangeCancelled;
    }

    private void handleNameChangeCancelled()
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleNameChangeDialogCommit;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleNameChangeCancelled;
    }

    private void handleNameChangeDialogCommit(string newName)
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleNameChangeDialogCommit;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleNameChangeCancelled;
        if (loopName != newName)
        {
            loopName = newName;
            hasDefaultName = false;
            exportVersion = 1;
            UsageLogger.log(UserAction.LOOP_CHANGED_NAME);
            if (OnNameChanged != null) OnNameChanged(newName);
        }
    }

    private void handleTrackSoloed(Track track, bool wasSoloed)
    {
        foreach (Track t in tracks)
        {
            if (t != track)
            {
                t.anotherIsSolo = wasSoloed;
            }
        }
    }

    public void Export()
    {
        if (hasDefaultName)
        {
            showWarningDialog("Saving with default name",
                              "You are about to save this loop with the default name. Please cancel and rename the loop, if you don't want to use the default name: " + loopName,
                              this.exportToJson);
        } else
        {
            exportToJson();
        }
       
    }

    private void exportToJson()
    {
        LoopExporter.exportToJson(loopName + "_v" + exportVersion, this);
        exportVersion++;
        ExperimentManager.instance.registerLoopWasSaved();
        UsageLogger.log(UserAction.LOOP_EXPORTED);
        wasSaved = true;
        if (songTrack.song != null)
        {
            SongLibrary.instance.markSongAsUsed(songTrack.song);
        }
        StartCoroutine(showExportSuccess());
    }

    private IEnumerator showExportSuccess()
    {
        exportSuccessToast.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        exportSuccessToast.SetActive(false);
    }

    public void showAddTrackDialog()
    {
        addTrackDialog.SetActive(true);
        sampleSelector.OnSampleDefinitionSelected += this.handleAddTrackDialogSelection;
    }

    private void handleAddTrackDialogSelection(SampleDefinition sd)
    {
        addTrack(sd);
        closeAddTrackDialog();
    }

    public void closeAddTrackDialog()
    {
        sampleSelector.OnSampleDefinitionSelected -= this.handleAddTrackDialogSelection;
        addTrackDialog.SetActive(false);
    }

    

    public void addBar()
    {
        durationInBars += 1;
        UsageLogger.log(UserAction.LOOP_BAR_ADDED);
    }

    public void addTrack(SampleDefinition sd)
    {
        int cIdx = Random.Range(nrOfInitialTracks, trackColors.Length);

        //instantiate track with its own copy of the sample definition
        addTrack(new SampleDefinition(sd), trackColors[cIdx]);
        UsageLogger.log(UserAction.TRACK_ADDED);
    }

    public GameObject addTrack(SampleDefinition sd, Color c, string trackName = "")
    {
        if (trackName == "") {
            trackName = "Track " + trackIdCtr;
        }
        trackIdCtr++;

        //A track with the same sample already exists -> link this new track to the same sample and its editor
        SampleEditor exisitingEditor = SampleEditorSpawnManager.instance.getEditor(sd.name);
        SampleEditor editor = exisitingEditor != null ? exisitingEditor : SampleEditorSpawnManager.instance.createNewSampleEditor(new Sample(sd), c);

        GameObject track = Instantiate(trackPrototype, NextTrackSpawnPos, Quaternion.identity, transform);
        track.transform.localRotation = Quaternion.identity;
        Track t = track.GetComponent<Track>();
        if (t != null)
        {
            tracks.Add(t);
            t.init(editor.sample);
            t.color = c;
            t.durationInBars = durationInBars;
            t.trackName = trackName;
            if (isPlaying && _playStartDspTime != GlobalConstants.InvalidTime)
            {
                t.isPlaying = true; //we can set that directly here because a new track is garuanteed to have no notes so

                //find startTime of last loop iteration
                double startTime = _playStartDspTime;
                while (startTime < AudioSettings.dspTime) startTime += duration;
                startTime -= duration;

                t.startTime = startTime;
            }

            t.OnTrackDeleted += this.handleTrackDeleted;
            t.OnSoloed += this.handleTrackSoloed;
            relayoutLoopUIComponents();
        } else
        {
            Debug.LogError("Gameobject added as Track did not contain a Track component!");
        }
        return track;
    }

    private void handleTrackDeleted(Track t)
    {
        tracks.Remove(t);
        t.OnTrackDeleted -= this.handleTrackDeleted;
        t.OnSoloed -= this.handleTrackSoloed;

        SampleEditorSpawnManager.instance.deleteSampleEditor(t.sample.name);

        updateTrackPositions();
        relayoutLoopUIComponents();
    }

    private void updateTrackPositions()
    {
        Vector3 spawnPos = firstTrackSpawnPosition.position;

        foreach (Track t in tracks)
        {
            t.gameObject.transform.position = spawnPos;
            spawnPos -= firstTrackSpawnPosition.up * trackSpacing;
        }
    }

    private void handleLengthInfluencingTimingChanges(float ignoreChangeFactor)
    {
        songTrack.length = BPMManager.instance.barLength * _durationInBars;
        grid.relayoutLines(_durationInBars, tracks.Count);
        background.relayout(_durationInBars, tracks.Count);
    }

    private void handleLengthInfluencingTimingChanges(int ignoreChangeFactor)
    {
        songTrack.length = BPMManager.instance.barLength * _durationInBars;
        grid.relayoutLines(_durationInBars, tracks.Count);
        background.relayout(_durationInBars, tracks.Count);
    }

    private void handleBeatSubdivisionChange(BeatSubdivision newSubDivs)
    {
        grid.relayoutLines(_durationInBars, tracks.Count);
    }

    private void relayoutLoopUIComponents()
    {
        songTrack.length = BPMManager.instance.barLength * _durationInBars;
        playhead.relayout(_durationInBars, tracks.Count);
        grid.relayoutLines(_durationInBars, tracks.Count);
        background.relayout(_durationInBars, tracks.Count);
    }

    private void showNotSavedWarning()
    {
        showWarningDialog("Loop nicht gespeichert!", "Du hast diesen Loop noch nicht gespeichert. Bitte speichere ihn, bevor Du einen neuen Loop anfängst.", this.Export);
    }

    private void showWarningDialog(string heading, string description, UnityAction handleConfirm = null, UnityAction handleCancel = null)
    {
        warningDialog.gameObject.SetActive(true);
        warningDialog.heading = heading;
        warningDialog.description = description;
        if (handleConfirm != null)
        {
            UnityAction confirmCB = null;
            confirmCB = () =>
            {
                handleConfirm();
                warningDialog.OnConfirmed.RemoveListener(confirmCB);
            };
            warningDialog.OnConfirmed.AddListener(confirmCB);
        }
        if (handleCancel != null)
        {
            UnityAction cancelCB = null;
            cancelCB = () =>
            {
                handleCancel();
                warningDialog.OnConfirmed.RemoveListener(cancelCB);
            };
            warningDialog.OnCanceled.AddListener(cancelCB);
        }
    }
}
