using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles user interaction with notes that are placed on a track (ignores free-floating notes)
/// Also, notes that are in the process of being added to a track are not affected
/// 
/// On "Hover" (i.e. pointer focus) show the context menu, hide after a delay after focus was lost
/// On Drag, move the note along the track and hide the context menu
/// </summary>
public class NoteInteractionHandler : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusHandler
{
    public Note note;
    bool dragInProgress = false;

    public GameObject contextMenu;
    public float contextMenuHideDelay = 0.5f;
    private Coroutine runningContextMenuDelayedHide;

    private bool noteInteractionEnabled { get { return note != null ? note.parentTrack != null : false; } set { } }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        //ignore
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (noteInteractionEnabled && !dragInProgress)
        {
            dragInProgress = true;
            forceHideContextMenu();
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (dragInProgress)
        {
            note.parentTrack.handleNoteMovementOnTrack(note, eventData.Pointer.Position);
        }
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (noteInteractionEnabled && dragInProgress)
        {
            //make sure linked note list stays in sync with physical positioning
            NoteFinderResult neighbors = NoteFinder.getImmediateNeighborsInBeatFlow(note);
            if ((neighbors.next != null && note.nextNote != neighbors.next)
                || (neighbors.prev != null && note.prevNote != neighbors.prev))
            {
                Note oldPrev = note.prevNote;
                Note oldNext = note.nextNote;

                if (oldPrev != null) oldPrev.nextNote = oldNext;
                if (oldNext != null) oldNext.prevNote = oldPrev;

                note.nextNote = neighbors.next;
                note.prevNote = neighbors.prev;
            }

            note.updateToNewPosition();

            dragInProgress = false;

            UsageLogger.log(UserAction.NOTE_MOVED);

            forceHideContextMenu();
        }

    }

    public void OnFocusEnter(FocusEventData eventData)
    {
        if (noteInteractionEnabled && contextMenu != null)
        {
            if (runningContextMenuDelayedHide != null)
            {
                StopCoroutine(runningContextMenuDelayedHide);
            }
            contextMenu.SetActive(true);
        }
    }

    public void OnFocusExit(FocusEventData eventData)
    {
        if (noteInteractionEnabled && contextMenu != null)
        {
            if (runningContextMenuDelayedHide != null) StopCoroutine(runningContextMenuDelayedHide);
            runningContextMenuDelayedHide = StartCoroutine(hideContextMenuDelayed());
        }
    }

    private IEnumerator hideContextMenuDelayed()
    {
        yield return new WaitForSeconds(contextMenuHideDelay);
        if (contextMenu != null) contextMenu.SetActive(false);
    }

    private void forceHideContextMenu()
    {
        if (runningContextMenuDelayedHide != null)
        {
            StopCoroutine(runningContextMenuDelayedHide);
        }
        if (contextMenu != null) contextMenu.SetActive(false);
    }
}
