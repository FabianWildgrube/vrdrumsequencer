using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract Base class for notes on a track.
/// Handles everything related to figuring out when a note should be played and triggering the playing.
/// Intitialisation and what "playing" a note actually looks like must be handled by subclasses.
/// 
/// Notes represent a single occurence of a sample on a track. They start playing, if their parentTrack is playing and
/// the game hits their trigger time (checked in fixedUpdate). This trigger time is dependent on the distance to their previousNote.
/// Thus, notes on a track form a linked list.
/// 
/// The component is designed to be instantiated (with a prefab) during the game.
/// Once a Track sets itself as the parent, this note initializes itself according to the Track's data.
/// The Track manages linking to other Notes upon instantiation.
/// </summary>
public abstract class Note : MonoBehaviour
{
    protected Track _parentTrack;
    public Track parentTrack
    {
        get { return _parentTrack; }
        set
        {
            _parentTrack = value;
            init();
        }
    }

    public Vector3 beatFlowForwardDir => transform.right;
    private void updatePositionInBeatDirection(float scalingFactorAlongBeatDir)
    {
        transform.localPosition = new Vector3(transform.localPosition.x * scalingFactorAlongBeatDir, transform.localPosition.y, transform.localPosition.z);
    }

    private bool initialised = false;

    #region linked Note list logic
    private Note _prevNote = null;
    public Note prevNote
    {
        get { return _prevNote; }
        set
        {
            _prevNote = value;
        }
    }

    private Note _nextNote = null;
    public Note nextNote
    {
        get { return _nextNote; }
        set { _nextNote = value; }
    }
    #endregion

    #region triggertime management
    private double _triggerTime = GlobalConstants.InvalidTime;

    /// <summary>
    /// Point in time when this note shall start playing (i.e. is triggered).
    public double triggerTime => _triggerTime;

    private void recomputeTriggerTime()
    {
        _triggerTime = _parentTrack.startTime + BPMManager.instance.distanceToTime(Vector3.Distance(transform.position, _parentTrack.minimalNotePosition));
    }

    public void updateToNewPosition()
    {
        if (_parentTrack.isPlaying)
        {
            recomputeTriggerTime();
            if (_triggerTime < AudioSettings.dspTime)
            {
                //we should have already been played => triggerTime is in the next loop
                _triggerTime += _parentTrack._durationInSeconds;
            }
            onTriggerTimeForNextLoopAvailable();
        }
    }

    /// <summary>
    /// Returns the trigger time of this note within its track (track start is assumed to be 0). Does not interfere with scheduling and is not dependant on the track playing or not.
    /// </summary>
    /// <returns></returns>
    public double zeroBasedTriggerTime()
    {
        return BPMManager.instance.distanceToTime(Vector3.Distance(transform.position, _parentTrack.minimalNotePosition));
    }

    protected abstract void onTriggerTimeForNextLoopAvailable();

    private void handleTrackTimingsChange()
    {
        if (_parentTrack.isPlaying && _triggerTime != GlobalConstants.InvalidTime)
        {
            recomputeTriggerTime();
            if (_triggerTime < AudioSettings.dspTime)
            {
                //we should have already been played => triggerTime is in the next loop
                _triggerTime += _parentTrack._durationInSeconds;
            }
            onTriggerTimeForNextLoopAvailable();
        }
    }

    private void handleTrackLengthChange(float wedontcare)
    {
        handleTrackTimingsChange();
    }
    #endregion

    #region playstate management
    public event Action OnStartedPlaying;

    private void handleTrackStart()
    {
        recomputeTriggerTime();
        onTrackStart();
    }

    private void handleTrackStop()
    {
        _triggerTime = GlobalConstants.InvalidTime; //force recompute upon next start
        onTrackStop();
    }

    protected abstract void onTrackStart();
    protected abstract void onTrackStop();

    private void FixedUpdate()
    {
        if (_parentTrack == null || !initialised) return;

        if (_parentTrack.isPlaying && triggerTime != GlobalConstants.InvalidTime && AudioSettings.dspTime >= triggerTime + 0.1f) //small epsilon to guard against unlucky audio and main thread synchronisation
        {
            //recompute triggerTime for next loop
            _triggerTime += _parentTrack._durationInSeconds;
            onTriggerTimeForNextLoopAvailable();

            if (_parentTrack.isAllowedToMakeSound && OnStartedPlaying != null) OnStartedPlaying();
        }
    }
    #endregion

    #region initialisation
    public event Action<Color> OnInitialized;

    /// make the note ready for being used on a track
    /// Assumes that member "trackParent" has been set
    private void init()
    {
        //Debug.Log("init note");
        _parentTrack.OnTrackStarted += this.handleTrackStart;
        _parentTrack.OnTrackStopped += this.handleTrackStop;
        _parentTrack.OnTimingsChanged += this.handleTrackTimingsChange;
        _parentTrack.OnTrackLengthChanged += this.handleTrackLengthChange;

        BPMManager.instance.OnDistancePerBeatChange += this.updatePositionInBeatDirection;

        initConcreteNote();

        if (OnInitialized != null) OnInitialized(_parentTrack.color);

        if (_parentTrack.isPlaying)
        {
            recomputeTriggerTime();
            if (_triggerTime < AudioSettings.dspTime)
            {
                //we should have already been played => triggerTime is in the next loop
                _triggerTime += _parentTrack._durationInSeconds;
            }
            onTriggerTimeForNextLoopAvailable();
        }

        initialised = true;
    }

    protected abstract void initConcreteNote();
    #endregion

    #region destruction
    public void delete()
    {
        //delete self from linked note list
        Note oldPrev = prevNote;
        Note oldNext = nextNote;
        if (oldPrev != null) oldPrev.nextNote = oldNext;
        if (oldNext != null) oldNext.prevNote = oldPrev;

        UsageLogger.log(UserAction.NOTE_DELETED);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _parentTrack.OnTrackStarted -= this.handleTrackStart;
        _parentTrack.OnTrackStopped -= this.handleTrackStop;
        _parentTrack.OnTimingsChanged -= this.handleTrackTimingsChange;
        _parentTrack.OnTrackLengthChanged -= this.handleTrackLengthChange;

        BPMManager.instance.OnDistancePerBeatChange -= this.updatePositionInBeatDirection;

        deInitConcreteNote();
    }

    protected abstract void deInitConcreteNote();
    #endregion

    #region utilities
    public static List<Note> getAllConnected(Note entryPoint)
    {
        List<Note> allNotes = new List<Note>();
        //find start note
        Note current = entryPoint;
        while (current.prevNote != null)
        {
            current = current.prevNote;
        }

        //collect all notes into a string
        while (true)
        {
            allNotes.Add(current);

            if (current.nextNote != null)
            {
                current = current.nextNote;
            }
            else
            {
                break;
            }
        }

        return allNotes;
    }

    /// Get a strinng representing the complete linked list of notes, that the passed in note is a part of
    public static string ListAllLinkedTo(Note entryPoint)
    {
        //find start note
        Note current = entryPoint;
        while (current.prevNote != null)
        {
            current = current.prevNote;
        }

        //collect all notes into a string
        string allNotes = "[";
        while (true)
        {
            allNotes += current.gameObject.name + (current.nextNote != null ? " - " : "");

            if (current.nextNote != null)
            {
                current = current.nextNote;
            }
            else
            {
                break;
            }
        }
        allNotes += "]";

        return allNotes;
    }
    #endregion
}
