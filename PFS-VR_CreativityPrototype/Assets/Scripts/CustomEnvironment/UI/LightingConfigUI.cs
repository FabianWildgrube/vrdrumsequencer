using UnityEngine;

public class LightingConfigUI : MonoBehaviour
{
    [SerializeField]
    ValueSliderCtrl todSlider;

    [SerializeField]
    GameObject todLabel;
    
    [SerializeField]
    ValueSliderCtrl mainLightIntensitySlider;

    [SerializeField]
    ValueSliderCtrl mainLightColorTemperatureSlider;

    [SerializeField]
    ValueSliderCtrl accentLightsAmountSlider;

    [SerializeField]
    ValueSliderCtrl accentLightsIntensitySlider;

    [SerializeField]
    AccentLightColorSelector accentLightsColorSelector;

    private void Start()
    {
        EnvironmentConfigManager.instance.registerConfigSubUI(gameObject);
    }

    public void updateUIElements(EnvironmentOptions options)
    {
        todSlider.gameObject.SetActive(true); //make sure the value can be updated
        todSlider.setValue(options.lighting.tod);
        todSlider.gameObject.SetActive(ToDLightHandler.instance.DaylightEnabled); //hide again if necessary
        todLabel.SetActive(ToDLightHandler.instance.DaylightEnabled); //hide if necessary

        mainLightIntensitySlider.setValue(options.lighting.intensity);
        mainLightColorTemperatureSlider.setValue(options.lighting.colorTemp);
        accentLightsAmountSlider.setValue(options.lighting.accentLights.amount);
        accentLightsIntensitySlider.setValue(options.lighting.accentLights.intensity);
        accentLightsColorSelector.select(options.lighting.accentLights.color);
    }

    public void handleToDChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateTimeOfDay(newValue);
    }

    public void handleMainLightIntensityChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateMainLightingIntensity(newValue);
    }

    public void handleMainLightColorTempChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateMainColorTemp(newValue);
    }

    public void handleAccentLightsAmountChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateAccentLightsAmount(newValue);
    }
    public void handleAccentLightsIntensityChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateAccentLightsIntensity(newValue);
    }

    public void handleToDChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_LIGHTING_TOD_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateTimeOfDay();
    }

    public void handleMainLightIntensityChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_LIGHTING_MAINLIGHT_INTENSITY_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateMainLightingIntensity();
    }

    public void handleMainLightColorTempChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_LIGHTING_MAINLIGHT_TEMP_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateMainColorTemp();
    }

    public void handleAccentLightsAmountChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_LIGHTING_ACCENT_AMOUNT_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateAccentLightsAmount();
    }
    public void handleAccentLightsIntensityChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_LIGHTING_ACCENT_INTENSITY_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateAccentLightsIntensity();
    }
}
