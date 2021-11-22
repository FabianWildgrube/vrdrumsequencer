using UnityEngine;

public struct NoteFinderResult
{
    public Note prev;
    public Note next;
}

/// <summary>
/// Helper class that provides utilities to find Notes in 3-D space
/// </summary>
public class NoteFinder
{
    public static NoteFinderResult getImmediateNeighborsInBeatFlow(Note note)
    {
        return getImmediateNeighborsOnLine(note, note.beatFlowForwardDir);
    }

    /// get the neighbor notes along a line defined by its forward vector
    /// NoteFinderResult contains null references if no appropriate neighbor was found
    public static NoteFinderResult getImmediateNeighborsOnLine(Note note, Vector3 lineForwardDirection)
    {
        return getImmediateNeighbors(note, lineForwardDirection, -lineForwardDirection);
    }

    public static NoteFinderResult getImmediateNeighbors(Note note, Vector3 nextSearchDir, Vector3 prevSearchDir)
    {
        NoteFinderResult res = new NoteFinderResult();

        Vector3 srcPoint = note.gameObject.transform.position;

        res.prev = getNearestNoteAlongVector(srcPoint, prevSearchDir);
        res.next = getNearestNoteAlongVector(srcPoint, nextSearchDir);

        return res;
    }

    public static Note getNearestNoteAlongVector(Vector3 srcPoint, Vector3 searchDirection)
    {
        Vector3 castDirection = searchDirection.normalized;

        RaycastHit hit;
        Debug.DrawRay(srcPoint, castDirection * 2.0f, Color.yellow, 1.0f);
        if (Physics.Raycast(srcPoint, castDirection, out hit, Mathf.Infinity, GlobalConstants.NotesLayerMask))
        {
            return hit.collider.transform.parent.gameObject.GetComponent<Note>();
        }

        return null;
    }
}
