using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AppearSoundCue : MonoBehaviour
{
    private AudioSource audioSource;

    private static int nrPlaying = 0;
    const int maxPlayers = 10;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.maxDistance = 45f;
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
    }

    private void OnEnable()
    {
        if (EnvironmentConfigManager.instance.appearSoundsEnabled && nrPlaying < maxPlayers)
        {
            audioSource.volume = EnvironmentConfigManager.instance.appearSoundVolume;
            nrPlaying++;
            EnvironmentConfigManager.instance.executeDeferred(EnvironmentConfigManager.instance.appearSound.length, () => nrPlaying--); //execute via longliving game object because this scripts gameobject might be set inactive very quickly
            audioSource.PlayOneShot(EnvironmentConfigManager.instance.appearSound);
        }
    }
}
