using UnityEngine;

/// <summary>
/// Special case of a note that is only used to set the start point of a track or Pauses that need to be visualized
/// </summary>
public class SilentNote : Note
{
    protected override void initConcreteNote()
    {
        //intentionally empty, no initialisation necessary for a silent note
    }

    protected override void deInitConcreteNote()
    {
        //intentionally empty, no de-initialisation necessary for a silent note
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }

    protected override void onTrackStart()
    {
        //nothing to play for a silent note
    }

    protected override void onTrackStop()
    {
        //nothing to play for a silent note
    }

    protected override void onTriggerTimeForNextLoopAvailable()
    {
        //nothing to do for a silent note
    }
}
