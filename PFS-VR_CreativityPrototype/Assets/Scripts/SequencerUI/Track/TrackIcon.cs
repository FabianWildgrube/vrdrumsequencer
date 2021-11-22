using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackIcon : MonoBehaviour
{
    public Track track;
    public GameObject visualisation;

    void Start()
    {
        ITrackIconVisualisation vis = visualisation.GetComponent<ITrackIconVisualisation>();
        if (vis != null)
        {
            vis.setColor(track.color);
        }
    }
}
