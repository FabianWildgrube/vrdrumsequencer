using System;
using UnityEngine;

public class SongSelector : MonoBehaviour
{
    public event Action<LoopableSong> OnSongSelected;

    public void signalSelection(LoopableSong song)
    {
        if (OnSongSelected != null) OnSongSelected(song);
    }
}
