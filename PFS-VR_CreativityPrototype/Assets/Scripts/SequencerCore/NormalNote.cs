using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A note with a sound attached to it.
/// When it receives the onTrackStart signal from its base class it schedules its clip to be played at the trigger time
/// Reschedules for each new loop repetition until onTrackStop is called.
/// This uses "PlayScheduled" which provides high accuracy needed to have proper rythmic stability in the sequencer.
/// 
/// Soloing and Muting of the Track is transferred to the AudioSource's "mute" property as to not interfere with the
/// play scheduling.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class NormalNote : Note
{
    private AudioSource _audioSource;   

    public float volume
    {
        get { return _audioSource != null ? _audioSource.volume : 0.0f; }
        set{ if (_audioSource != null) _audioSource.volume = value; }
    }

    private void handleTrackSoundAllowedChange(bool soundAllowed)
    {
        _audioSource.mute = !soundAllowed;
    }

    /// Assumes that member "_parentTrack" has been set
    protected override void initConcreteNote()
    {
        //Debug.Log("init note");
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (_parentTrack.sample.clip != null)
        {
            _audioSource.clip = _parentTrack.sample.clip;
        }

        _audioSource.mute = !_parentTrack.isAllowedToMakeSound;

        _parentTrack.sample.OnClipChanged += this.handleTrackSampleDefinitionChanged;
        _parentTrack.OnVolumeChanged += this.handleTrackVolumChange;
        _parentTrack.OnAllowedToMakeSoundChanged += this.handleTrackSoundAllowedChange;
    }

    protected override void deInitConcreteNote()
    {
        _parentTrack.sample.OnClipChanged -= this.handleTrackSampleDefinitionChanged;
        _parentTrack.OnVolumeChanged -= this.handleTrackVolumChange;
        _parentTrack.OnAllowedToMakeSoundChanged -= this.handleTrackSoundAllowedChange;
    }

    private void handleTrackSampleDefinitionChanged(AudioClip newClip)
    {
        if (_audioSource != null)
        {
            _audioSource.clip = newClip;
        }
    }

    private void handleTrackVolumChange(float newVolume)
    {
        volume = newVolume;
    }    

    protected override void onTrackStart()
    {
        float epsilon = 0.01f;
        if (triggerTime - AudioSettings.dspTime <= epsilon && triggerTime - AudioSettings.dspTime >= -epsilon)
        {
            //notes at the very beginning of the loop must be played instantly 
            _audioSource.Play();
        }
        else
        {
            scheduleNextPlay();
        }
    }

    protected override void onTriggerTimeForNextLoopAvailable()
    {
        StartCoroutine(scheduleNextPlayAfterCurrentIsOverCoroutine());
        StartCoroutine(prioDown());
    }

    protected override void onTrackStop()
    {
        StopAllCoroutines();
        _audioSource.Stop();
    }

    private IEnumerator scheduleNextPlayAfterCurrentIsOverCoroutine()
    {
        if (_audioSource.time > 0)
        {
            //Debug.Log("Reschedule of note waiting because it is playing: " + (_audioSource.clip.length - _audioSource.time));
            yield return new WaitForSeconds(_audioSource.clip.length - _audioSource.time + 0.01f); //let the clip play through because rescheduling stops the current playing
        } else
        {
            //Debug.Log("Reschedule stopped note.");
            _audioSource.Stop(); //cancel current scheduling
        }
        scheduleNextPlay();
        _audioSource.priority = 0;
    }

    private void scheduleNextPlay()
    {
        //Debug.Log("Current DSP: " + AudioSettings.dspTime + ", scheduling note for: " + triggerTime);
        if (triggerTime > AudioSettings.dspTime)
        {
            _audioSource.PlayScheduled(triggerTime);
        } else
        {
            //Debug.LogError("Tried to schedule note in the past!");
        }
    }

    private IEnumerator prioDown()
    {
        yield return new WaitForSeconds(0.3f);
        _audioSource.priority = 128;
    }
}
