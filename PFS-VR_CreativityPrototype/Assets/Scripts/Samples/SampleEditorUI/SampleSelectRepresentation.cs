using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SampleSelectRepresentation : MonoBehaviour
{
    private AudioSource audioSrc;
    private Sample sample;

    public bool isRandomSample = false;

    public TMPro.TextMeshPro nameTxtField;

    public PlayStopBtn playStopBtn;

    public event Action<SampleDefinition> OnSelected;

    public event Action OnPlay;

    private void Start()
    {
        if (isRandomSample)
        {
            audioSrc = GetComponent<AudioSource>();
            audioSrc.playOnAwake = false;

            sample = new Sample("Random Sample");
            sample.OnClipChanged += this.handleClipChanged;

            nameTxtField.text = sample.name;
        }
    }

    public void updateData(SampleDefinition sd)
    {
        if (!isRandomSample)
        {
            Debug.Log("updateData called with sd: " + sd.name);
            if (audioSrc == null)
            {
                audioSrc = GetComponent<AudioSource>();
                audioSrc.playOnAwake = false;
            }

            if (sample == null)
            {
                sample = new Sample(sd);
                sample.OnClipChanged += this.handleClipChanged;
            }
            else
            {
                sample.setNewDefinition(sd);
            }

            nameTxtField.text = sd.name;
        }
    }

    private void OnDestroy()
    {
        if (sample != null)
        sample.OnClipChanged -= this.handleClipChanged;
    }

    void handleClipChanged(AudioClip newClip)
    {
        audioSrc.clip = newClip;
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
        if (OnSelected != null) OnSelected(sample.definition);
    }
}
