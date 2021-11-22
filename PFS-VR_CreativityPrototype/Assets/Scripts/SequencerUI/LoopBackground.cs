using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that scales to its right to make sure something is as long as is needed to cover the entire length of a loop.
/// And scales to the bottom to account for all tracks in a loop
/// Assumes that the object its attached to scales solely to the right and bottom (by placing its children with their topleft corner at the origin)
/// Has a parameter baseLength which is always added to the length for the loop to account for any extra UI at the start/finish
/// baseHeight works accordingly
/// </summary>
public class LoopBackground : MonoBehaviour
{
    public float baseLength = 0.0f;
    public float baseHeight = 0.0f;

    // Scaling factors to account for the fact that the background plate is not properly scaled to have a 1:1 ratio between scale and actual size in meters
    public float lengthScaleFactor = 1.0f;
    public float heightScaleFactor = 1.0f;

    [HideInInspector]
    public float trackHeight; //has to be set up by another script before first use!

    public void relayout(int nrOfBars, int nrOfTracks)
    {
        float newScaleX = baseLength;
        float newScaleY = baseHeight;

        newScaleY += (nrOfTracks * trackHeight) * heightScaleFactor;
        newScaleX += (nrOfBars * BPMManager.instance.barLength) * lengthScaleFactor;

        transform.localScale = new Vector3(newScaleX, newScaleY, transform.localScale.z);
    }
}
