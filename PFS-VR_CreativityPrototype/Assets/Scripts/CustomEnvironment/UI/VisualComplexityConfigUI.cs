using UnityEngine;

public class VisualComplexityConfigUI : MonoBehaviour
{
    [SerializeField]
    ValueSliderCtrl objectsSlider;

    [SerializeField]
    ValueSliderCtrl tidynessSlider;

    private void Start()
    {
        EnvironmentConfigManager.instance.registerConfigSubUI(gameObject);
    }

    public void updateUIElements(EnvironmentOptions options)
    {
        objectsSlider.setValue(options.complexity.objects);
        tidynessSlider.setValue(options.complexity.tidyness);
    }

    public void handleComplexityObjectsChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateComplexityObjects(newValue);
    }
    public void handleTidynessChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateComplexityTidyness(newValue);
    }

    public void handleTidynessChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_COMPLEXITY_TIDYNESS_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateComplexityTidynesss();
    }

    public void handleComplexityObjectsChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_COMPLEXITY_AMOUNT_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateComplexityObjects();
    }
}
