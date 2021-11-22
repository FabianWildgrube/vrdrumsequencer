using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongSelectRepresentation : MonoBehaviour
{
    private AudioSource audioSrc;
    private LoopableSong song;
    public LoopableSong Song => song;

    public TMPro.TextMeshPro nameTxtField;
    public TMPro.TextMeshPro genreTxtField;
    public TMPro.TextMeshPro durationTxtField;
    public TMPro.TextMeshPro tempoTxtField;

    public PlayStopBtn playStopBtn;

    public event Action<LoopableSong> OnSelected;

    public event Action OnPlay;

    public void init(LoopableSong song)
    {
        if (audioSrc == null)
        {
            audioSrc = GetComponent<AudioSource>();
            audioSrc.playOnAwake = false;
        }

        this.song = song;
        audioSrc.clip = song.clip;
        nameTxtField.text = song.name;
        genreTxtField.text = song.genre;
        tempoTxtField.text = $"{song.bpm:0} BPM";
        durationTxtField.text = song.durationInBars + " Takte";
    }

    public void Play()
    {
        if (audioSrc != null)
        {
            audioSrc.Play();
            if (OnPlay != null) OnPlay();
        }
    }

    public void Stop()
    {
        if (audioSrc != null) audioSrc.Stop();
    }

    public void forceStop()
    {
        Stop();
        playStopBtn.forceShowPlayState();
    }

    public void Select()
    {
        if (OnSelected != null) OnSelected(song);
    }
}
