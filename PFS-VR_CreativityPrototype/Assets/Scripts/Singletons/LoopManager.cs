using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles loading of loops. Allows only one loop at a time. (Old loop is destroyed before a new one is loaded)
/// </summary>
public class LoopManager : MonoBehaviour
{
    public static LoopManager instance;
    public LoopDataSelector loopImportSelector;
    public GameObject loopImportDialog;
    public Transform loopImportDialogSpawnPos;

    public GameObject loopPrototype;
    public Transform defaultSpawnPos;
    public Transform teleportToLoopTarget;

    public Transform allLoopStuffParent;

    private GameObject currentLoopGO;

    private Vector3 currentSpawnPos => currentLoopGO != null ? currentLoopGO.transform.position : defaultSpawnPos.position;
    private Quaternion currentSpawnRot => currentLoopGO != null ? currentLoopGO.transform.rotation : defaultSpawnPos.rotation;

    private Vector3 allLoopStuffParentDefaultPosition;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        allLoopStuffParentDefaultPosition = allLoopStuffParent.position;
    }

    public void moveLoopToDefaultPositionNearOrigin()
    {
        allLoopStuffParent.position = allLoopStuffParentDefaultPosition;
    }

    public void moveLoopToPlaySpace()
    {
        allLoopStuffParent.position = MixedRealityPlayspace.Position;
    }

    public void movePlaySpaceToLoop()
    {
        var targetPosition = teleportToLoopTarget.position;
        Vector3 targetRotation = teleportToLoopTarget.rotation.eulerAngles; //euler angles to get the y component further down

        if (currentLoopGO != null)
        {
            var currentLoop = currentLoopGO.GetComponent<Loop>();
            targetPosition = currentLoop.getPlaySpaceTargetPositionForTeleportation();
            targetRotation = currentLoop.getPlaySpaceLookAtRotationForTeleportation();
        }

        float height = targetPosition.y;
        targetPosition -= CameraCache.Main.transform.position - MixedRealityPlayspace.Position;
        targetPosition.y = height;

        MixedRealityPlayspace.Position = targetPosition;
        MixedRealityPlayspace.RotateAround(
                    CameraCache.Main.transform.position,
                    Vector3.up,
                    targetRotation.y - CameraCache.Main.transform.eulerAngles.y);

        MainMenu.instance.forceMoveMenuOrOpenScreenToUser();
    }

    public void startLoopSelectFlow()
    {
        loopImportDialog.transform.position = loopImportDialogSpawnPos.position;
        loopImportDialog.transform.rotation = loopImportDialogSpawnPos.rotation;
        loopImportDialog.SetActive(true);
        loopImportSelector.OnLoopDataSelected += this.handleLoopImportSelection;
        loopImportSelector.loadListFrom(ExperimentManager.instance.currentUserAndParticipantLoopDirs);
    }

    private void handleLoopImportSelection(LoopExportData ld)
    {
        instantiateFromLoopData(ld);
        UsageLogger.log(UserAction.LOOP_LOADED_FROM_DISK);
        ExperimentManager.instance.registerCustomLoopLoaded();
        closeLoopImportDialog();
    }

    public void closeLoopImportDialog()
    {
        loopImportSelector.OnLoopDataSelected -= this.handleLoopImportSelection;
        loopImportDialog.SetActive(false);
    }

    public void loadDefaultLoop()
    {
        clearCurrentLoop();

        currentLoopGO = Instantiate(loopPrototype, currentSpawnPos, currentSpawnRot, allLoopStuffParent);

        UsageLogger.log(UserAction.LOOP_LOADED_DEFAULT);
        ExperimentManager.instance.registerNewDefaultLoopLoaded();
    }

    public void clearCurrentLoop()
    {
        if (currentLoopGO != null)
        {
            Destroy(currentLoopGO);
            SampleEditorSpawnManager.instance.clear();
        }
    }

    public void instantiateFromFile(string fileName)
    {
        instantiateFromLoopData(parseLoopDataFromAbsFilepath(Path.Combine(GlobalConstants.LoopExportDirectoryName, fileName)));
    }

    public void instantiateFromFileAbsolute(string absFilePath)
    {
        instantiateFromLoopData(parseLoopDataFromAbsFilepath(absFilePath));
    }

    public LoopExportData parseLoopDataFromAbsFilepath(string filePath)
    {
        return FileIOUtils.createFromJsonFile<LoopExportData>(filePath, false);
    }

    public void instantiateFromLoopData(LoopExportData loopData)
    {
        if (currentLoopGO != null)
        {
            Destroy(currentLoopGO);
            SampleEditorSpawnManager.instance.clear();
        }

        currentLoopGO = instantiateToGameObjects(loopData, allLoopStuffParent);
    }

    private GameObject instantiateToGameObjects(LoopExportData loopData, Transform parent)
    {
        GameObject loopGO = Instantiate(loopPrototype, currentSpawnPos, currentSpawnRot, parent);
        Loop loop = loopGO.GetComponent<Loop>();
        if (loop == null)
        {
            Debug.LogError("Could not create Loop, LoopPrototype Prefab is missing Loop component");
            Destroy(loopGO);
            return null;
        }

        loop.loopName = loopData.loopName;
        loop.durationInBars = loopData.durationInBars;

        if (loopData.songName != null && loopData.songName != "")
        {
            LoopableSong song = SongLibrary.instance.getByName(loopData.songName);
            if (song != null)
            {
                loop.songTrack.setSong(song);
                loop.songTrack.handleVolumeChange(loopData.songVolume);
                loop.songTrack.volumeSlider.initialValue = loopData.songVolume;
            }
        }

        //set stuff on BPMManager (after loading the song, because that triggers bpm settings to change; and we want the loop settings to "win"
        //BPMManager.instance.setBPM(loopData.bpm, false); //hotfix
        BPMManager.instance.setBeatsPerBar(loopData.timeSignatureHi, false);
        BPMManager.instance.setBeatType(loopData.timeSignatureLo, false);
        //BPMManager.instance.setDistancePerBeat(loopData.metersPerBeat, false); //hotfix

        //add the tracks
        foreach (TrackExportData trackData in loopData.tracks)
        {
            GameObject trackGO = loop.addTrack(trackData.sampleDefinition, new Color(trackData.color.r, trackData.color.g, trackData.color.b), trackData.trackName);
            Track track = trackGO.GetComponent<Track>();
            if (track == null)
            {
                Debug.LogError("Could not create Loop, Track Prefab is missing Track component");
                Destroy(loopGO);
                return null;
            }

            track.handleVolumeChange(trackData.trackVolume);
            track.volumeSlider.initialValue = trackData.trackVolume;
            track.anotherIsSolo = trackData.anotherIsSolo;
            track.isSolo = trackData.isSolo;
            track.isMuted = trackData.isMuted;

            TrackLine trackLine = trackGO.GetComponentInChildren<TrackLine>();
            if (trackLine == null)
            {
                Debug.LogError("Could not create Loop, Track Prefab is missing TrackLine component in children");
                Destroy(loopGO);
                return null;
            }


            Note prevNote = null;
            foreach (NoteExportData noteData in trackData.notes)
            {
                GameObject noteGO = trackLine.instantiateNoteGameObject(trackLine.positionOnTrackInWorld(noteData.posOnTrackAxis));
                Note note = noteGO.GetComponent<Note>();
                if (note == null)
                {
                    Debug.LogError("Could not create Loop, Note Prefab is missing Note component");
                    Destroy(loopGO);
                return null;
                }

                track.addNewNote(note, prevNote, null);
                prevNote = note;
            }
        }

        return loopGO;
    }
}
