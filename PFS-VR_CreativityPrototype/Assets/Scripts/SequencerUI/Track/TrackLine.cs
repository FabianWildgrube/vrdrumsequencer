using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

/// <summary>
/// Component responsible for creating notes on a track, when the user clicks on its representation in space
/// Handles MRTK pointer events on all its child objects by implementing the IMixedRealityPointerHandler
/// </summary>
public class TrackLine : MonoBehaviour, IMixedRealityPointerHandler
{
    #region Inspector Variables
    public Track track;
    public GameObject notePrototype;
    public GameObject trackLineVisualisation;
    public Transform noteObjectsParent;
    #endregion

    /// <summary>
    /// Helper class to keep state of a note that is in the process of being added in one place
    /// </summary>
    class NoteInMovement
    {
        public Note noteComponent;
        public GameObject gameObject;
        public Vector3 previousControllerPosition;
        public Vector3 relativeMovementSinceAddStart;

        public NoteInMovement(GameObject noteGO)
        {
            gameObject = noteGO;
            noteComponent = gameObject.GetComponent<Note>();
            if (noteComponent == null) Debug.LogError("notePrototype GO did not contain a Note component!");
        }
    }
    private NoteInMovement currentNewNote;

    private void Start()
    {
        track.OnTrackLengthChanged += this.handleTrackLengthChange;
        BPMManager.instance.OnDistancePerBeatChange += this.handleTrackLengthChange;
        handleTrackLengthChange(track.length);
    }

    private void OnDestroy()
    {
        track.OnTrackLengthChanged -= this.handleTrackLengthChange;
        BPMManager.instance.OnDistancePerBeatChange -= this.handleTrackLengthChange;
    }

    /// scale track visualisation to the new length
    private void handleTrackLengthChange(float newLength)
    {
        newLength = BPMManager.instance.timeToDistance(track._durationInSeconds);
        if (newLength > 0.001f)
        {
            trackLineVisualisation.transform.localPosition = new Vector3(newLength * 0.5f, 0, 0);
            // scale along y-Axis because the track is a rotated cylinder
            trackLineVisualisation.transform.localScale = new Vector3(trackLineVisualisation.transform.localScale.x, newLength * 0.5f, trackLineVisualisation.transform.localScale.z);
        }
    }

    #region MRTK pointer input handling
    void IMixedRealityPointerHandler.OnPointerDown(
         MixedRealityPointerEventData eventData)
    {
        if (currentNewNote == null)
        {
            Debug.Log("Creating new Note");
            currentNewNote = new NoteInMovement(instantiateNoteGameObject(track.getNoteSpawnPosition(eventData.Pointer.Result.Details.Point))); //use the actual hit point for spawning
            currentNewNote.previousControllerPosition = eventData.Pointer.Position; //remember the controller's position to use its movement for dragging the new note
            currentNewNote.relativeMovementSinceAddStart = Vector3.zero;
        }
    }

    public GameObject instantiateNoteGameObject(Vector3 spawnPosition)
    {
        Quaternion noteRightIsTrackForward = Quaternion.FromToRotation(Vector3.right, track.trackForwardDirection); //make sure note's right face along the track's forward
        return Instantiate(notePrototype, spawnPosition, noteRightIsTrackForward, noteObjectsParent);
    }

    /// <summary>
    /// Returns a point along the track line in world coordinates
    /// </summary>
    /// <param name="alongForward">value between 0.0f - 1.0f representing the point along the track line</param>
    public Vector3 positionOnTrackInWorld(float alongForward)
    {
        return track.minimalNotePosition + track.trackForwardDirection * alongForward * track.length;
    }

    void IMixedRealityPointerHandler.OnPointerDragged(
         MixedRealityPointerEventData eventData)
    {
        if (currentNewNote != null)
        {
            var relativeMovementSinceLastCall = eventData.Pointer.Position - currentNewNote.previousControllerPosition;
            currentNewNote.relativeMovementSinceAddStart += relativeMovementSinceLastCall; //relative since start is needed because the pointer sticks to the track line, even when the note moves => constant start position
            track.handleNoteMovementOnTrack(currentNewNote.noteComponent, eventData.Pointer.Result.Details.Point + currentNewNote.relativeMovementSinceAddStart);
            currentNewNote.previousControllerPosition = eventData.Pointer.Position;
        }
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (currentNewNote != null)
        {
            Debug.Log("Releasing new Note");
            NoteFinderResult neighbors = NoteFinder.getImmediateNeighborsOnLine(currentNewNote.noteComponent, track.trackForwardDirection);
            track.addNewNote(currentNewNote.noteComponent, neighbors.prev, neighbors.next);
            UsageLogger.log(UserAction.NOTE_ADDED);

            resetForNextNote();
        }
    }
    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData) { /*ignore event*/ }
    #endregion

    private void resetForNextNote()
    {
        currentNewNote = null;
    }
}
