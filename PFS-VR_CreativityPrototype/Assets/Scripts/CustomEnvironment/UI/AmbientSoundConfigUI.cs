using UnityEngine;

public class AmbientSoundConfigUI : MonoBehaviour
{
    [SerializeField]
    ValueSliderCtrl volumeSlider;

    private void Start()
    {
        EnvironmentConfigManager.instance.registerConfigSubUI(gameObject);
    }

    public void updateUIElements(EnvironmentOptions options)
    {
        volumeSlider.setValue(options.ambientSoundVolume);
    }

    public void handleVolumeChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateAmbientSoundVolume(newValue);
    }

    public void handleVolumeChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_AMBIENTSOUND_VOLUME_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateAmbientSoundVolume();
    }
}
