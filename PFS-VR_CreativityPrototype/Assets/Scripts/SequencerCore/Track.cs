using Microsoft.MixedReality.Toolkit.UI;
using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Track : MonoBehaviour
{
    public Color color;
    public Sample sample;
    public string trackName;
    public Interactable muteInteractable;
    public Interactable soloInteractable;
    public float defaultVolume = 0.5f;
    public ValueSliderCtrl volumeSlider;

    public AudioSource audioSource;

    [SerializeField()]
    Transform firstNotePosition;
    public Vector3 minimalNotePosition => firstNotePosition.position;
    public Vector3 trackForwardDirection => firstNotePosition.right;

    [HideInInspector]
    public bool isPlaying = false;

    private bool _isMuted = false;
    [HideInInspector]
    public bool isMuted { get { return _isMuted; } set { _isMuted = value; if (OnAllowedToMakeSoundChanged != null) OnAllowedToMakeSoundChanged(isAllowedToMakeSound); } }

    private bool _isSolo = false;
    [HideInInspector]
    public bool isSolo { get { return _isSolo; } set { _isSolo = value; if (OnAllowedToMakeSoundChanged != null) OnAllowedToMakeSoundChanged(isAllowedToMakeSound); } }

    private bool _anotherIsSolo = false;
    [HideInInspector]
    public bool anotherIsSolo { get { return _anotherIsSolo;  }
    set
        {
            _anotherIsSolo = value;
            if (value)
            {
                _isSolo = false;
                soloInteractable.IsToggled = false;
            }
            if (OnAllowedToMakeSoundChanged != null) OnAllowedToMakeSoundChanged(isAllowedToMakeSound);
        }
    }
    public bool isAllowedToMakeSound => _isSolo || (!_anotherIsSolo && !_isMuted);

    [HideInInspector()]
    public double startTime = GlobalConstants.InvalidTime;

    #region Events for Notes of the Loop
    public event Action OnTrackStarted;
    public event Action OnTrackStopped;
    public event Action OnTimingsChanged;
    public event Action<bool> OnAllowedToMakeSoundChanged;
    #endregion

    #region Events for Track Length
    public event Action<float> OnTrackLengthChanged;
    #endregion

    #region Events for Parent Loop
    public event Action<Track> OnTrackDeleted;
    public event Action<Track, bool> OnSoloed;
    #endregion

    [HideInInspector]
    public Note firstNote = null;

    private bool snappingEnabled = true;

    public float volume;
    public Action<float> OnVolumeChanged;

    private float _length = 0f;
    public float _durationInSeconds = 0f; //kept in sync with length, but updated "by hand" in handleTimingParamsChanged because some computations require the "old" duration to be known when timings change
    public float length => _length;
    private void setLength(float newLength)
    {
        _length = newLength;
        if (OnTrackLengthChanged != null) OnTrackLengthChanged.Invoke(_length);
    }

    private float _durationInBars = 0f;
    public float durationInBars
    {
        get { return _durationInBars; }
        set
        {
            _durationInBars = value;
            _durationInSeconds = BPMManager.instance.secondsPerBar * _durationInBars;
            setLength(BPMManager.instance.timeToDistance(_durationInSeconds));
        }
    }

    private void Start()
    {
        BPMManager.instance.OnBPMChange += this.handleBPMChanged;
        BPMManager.instance.OnBeatsPerBarChange += this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatTypeChange += this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnDistancePerBeatChange += this.handleDistancePerBeatChange;

        muteInteractable.IsToggled = _isMuted;
        soloInteractable.IsToggled = _isSolo;
        handleVolumeChange(defaultVolume);
    }

    private void OnDestroy()
    {
        BPMManager.instance.OnBPMChange -= this.handleBPMChanged;
        BPMManager.instance.OnBeatsPerBarChange -= this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnBeatTypeChange -= this.handleLengthInfluencingTimingChanges;
        BPMManager.instance.OnDistancePerBeatChange -= this.handleDistancePerBeatChange;
    }

    private void handleDistancePerBeatChange(float ignore)
    {
        setLength(BPMManager.instance.timeToDistance(_durationInSeconds));
    }

    private void handleLengthInfluencingTimingChanges(int ignore)
    {
        _durationInSeconds = BPMManager.instance.secondsPerBar * _durationInBars;
        setLength(BPMManager.instance.timeToDistance(_durationInSeconds));
    }


    public void init(Sample sample)
    {
        this.sample = sample;
    }

    public void PlaySampleOneShot()
    {
        if (audioSource != null && sample != null)
        {
            audioSource.PlayOneShot(sample.clip);
            UsageLogger.log(UserAction.TRACK_SAMPLE_PREVIEW_PLAYED);
        }
    }

    public void Play()
    {
        if (isPlaying)
        {
            Stop();
        }

        startTime = AudioSettings.dspTime;
        _durationInSeconds = BPMManager.instance.distanceToTime(_length);
        Debug.Log(trackName + ": Play triggered (" + startTime + ")");
        isPlaying = true;
        if (OnTrackStarted != null) OnTrackStarted();
    }

    private void FixedUpdate()
    {
        //adjust start time to start of next loop around to enable notes recomputing their trigger times when timings change
        if (isPlaying && AudioSettings.dspTime > startTime + _durationInSeconds)
        {
            startTime += _durationInSeconds;
        }
    }

    public void Stop()
    {
        Debug.Log(trackName + ": Stop triggered");
        if (isPlaying)
        {
            isPlaying = false;
            startTime = GlobalConstants.InvalidTime;
            if (OnTrackStopped != null) OnTrackStopped();
        }
    }

    public void ToggleSolo()
    {
        isSolo = !isSolo;
        UsageLogger.log(isSolo ? UserAction.TRACK_SOLOED : UserAction.TRACK_UNSOLOED);
        if (isSolo) anotherIsSolo = false;
        if (OnSoloed != null) OnSoloed(this, isSolo);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        UsageLogger.log(isMuted ? UserAction.TRACK_MUTED : UserAction.TRACK_UNMUTED);
    }

    public void enableSnapping()
    {
        snappingEnabled = true;
    }

    public void disableSnapping()
    {
        snappingEnabled = false;
    }
    
    public void handleVolumeChange(float newVolume)
    {
        volume = newVolume;
        audioSource.volume = newVolume;
        if (OnVolumeChanged != null) OnVolumeChanged(newVolume);
    }

    public void handleVolumeChangeInteractionEnd()
    {
        UsageLogger.log(UserAction.TRACK_VOLUME_CHANGED);
    }

    public void moveToSampleEditor()
    {
        SampleEditorSpawnManager.instance.movePlaySpaceToSampleEditor(sample.name);
    }

    public void addNewNote(Note newNote, Note prevNote, Note nextNote)
    {
        //parent it
        newNote.parentTrack = this;
        if (newNote is NormalNote)
        {
            (newNote as NormalNote).volume = volume;
        }

        //properly insert new note into the linked note list
        if (prevNote != null) {
            prevNote.nextNote = newNote;
            newNote.prevNote = prevNote;
        } else
        {
            firstNote = newNote;
        }

        if (nextNote != null) {
            newNote.nextNote = nextNote;
            nextNote.prevNote = newNote;
        }

        Debug.Log(trackName + ": Note added");
        Debug.Log(trackName + " notes: " + Note.ListAllLinkedTo(newNote));
    }

    private Tuple<bool, Vector3> preventNoteMovementPastTrackEdges(Vector3 theoreticalPosOnTrack)
    {
        Vector3 vecBetweenNoteAndMinPositionNormalized = (theoreticalPosOnTrack - minimalNotePosition).normalized;
        Debug.Log("vecBetweenNoteAndMinPositionNormalized" + vecBetweenNoteAndMinPositionNormalized + ", ForwardDir: " + trackForwardDirection);
        if (!Utils.roughEquals(vecBetweenNoteAndMinPositionNormalized, trackForwardDirection))
        {
            //the vector between note and minimal position doesn't point in the correct direction -> we have crossed the minimal position
            Debug.Log("Returning minimal note position");
            return new Tuple<bool, Vector3>(true, minimalNotePosition);
        }
        else
        {
            Vector3 maximumNotePosition = minimalNotePosition + trackForwardDirection * length;
            Vector3 vecBetweenNoteAndMaxPositionNormalized = (maximumNotePosition - theoreticalPosOnTrack).normalized;
            if (!Utils.roughEquals(vecBetweenNoteAndMaxPositionNormalized, trackForwardDirection))
            {
                //the vector between note and maximal position doesn't point in the correct direction -> we have crossed the maximal position
                return new Tuple<bool, Vector3>(true, maximumNotePosition);
            }
        }

        return new Tuple<bool, Vector3>(false, theoreticalPosOnTrack);
    }

    private Vector3 snapToBeatLines(Vector3 theoreticalPosOnTrack)
    {
        //get position along the beat axis && convert to time within the loop
        float posAsTimeInLoop = BPMManager.instance.distanceToTime(Vector3.Distance(theoreticalPosOnTrack, minimalNotePosition));
        //find the closest (sub) beat
        float closestSubBeatTime = BPMManager.instance.closestSubBeatTime(posAsTimeInLoop);
        Debug.Log("TheoreticalPositiona as Time: " + posAsTimeInLoop + ", Distance: " + Vector3.Distance(theoreticalPosOnTrack, minimalNotePosition) + "Closest Subbeat: " + closestSubBeatTime / BPMManager.instance.secondsPerSubBeat);
        //snap to that (sub)beat's position if we're within the snapping distance
        if (Mathf.Abs(posAsTimeInLoop - closestSubBeatTime) <= BPMManager.instance.subbeatSnappingDistance)
        {
            return minimalNotePosition + trackForwardDirection * BPMManager.instance.timeToDistance(closestSubBeatTime);
        } else
        {
            return theoreticalPosOnTrack;
        }
    }

    public Vector3 getNoteSpawnPosition(Vector3 controllerWorldPosition)
    {
        Vector3 controllerPositionOnTrack = projectOntoTrack(controllerWorldPosition);
        return minimalNotePosition + controllerPositionOnTrack;
    }

    public void handleNoteMovementOnTrack(Note note, Vector3 dragHandleWorldPos)
    {
        Vector3 newControllerPosOnTrack = projectOntoTrack(dragHandleWorldPos);
        Vector3 theoreticalEndPosition = minimalNotePosition + newControllerPosOnTrack;

        var safePosition = preventNoteMovementPastTrackEdges(theoreticalEndPosition);
        Vector3 endPosition = safePosition.Item2;
        bool alreadyMovedPastTrackEdge = safePosition.Item1;

        if (snappingEnabled && !alreadyMovedPastTrackEdge)
        {
            endPosition = snapToBeatLines(theoreticalEndPosition);
        }

        note.gameObject.transform.position = endPosition;
    }

    private Vector3 projectOntoTrack(Vector3 positionInWorldSpace)
    {
        //get closest point on trackForward; similar to the example here https://docs.unity3d.com/ScriptReference/Vector3.Project.html
        Vector3 heading = positionInWorldSpace - minimalNotePosition;
        return Vector3.Project(heading, trackForwardDirection);
    }

    public void delete()
    {
        Stop();
        if (isSolo && OnSoloed != null) OnSoloed(this, false); //force un-solo so other tracks are unmuted
        if (OnTrackDeleted != null) OnTrackDeleted(this);
        UsageLogger.log(UserAction.TRACK_DELETED);
        Destroy(gameObject);
    }

    private void handleBPMChanged(float ingore)
    {
        if (isPlaying) //re-adjust time scale for new timing parameters to hear track in new tempo instantly
        {
            //where are we along the (old) time axis
            double progressInLoop = (AudioSettings.dspTime - startTime) / (_durationInSeconds);

            Debug.Log("Progress in Loop: " + progressInLoop);
            Debug.Log("Current start time: " + startTime);

            //new duration
            _durationInSeconds = BPMManager.instance.distanceToTime(_length);

            //adjust startTime to ensure that notes which are in the future use the correct new timing
            double timeTheoreticallyPassedWithNewLength = progressInLoop * _durationInSeconds;
            startTime = AudioSettings.dspTime - timeTheoreticallyPassedWithNewLength;

            Debug.Log("Theoretical start time with new parameters: " + startTime);
        }
        if (OnTimingsChanged != null) OnTimingsChanged();
    }
}
