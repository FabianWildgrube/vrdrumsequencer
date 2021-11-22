using UnityEngine;

public class ColorfulnessConfigUI : MonoBehaviour
{
    [SerializeField]
    ValueSliderCtrl amountSlider;

    [SerializeField]
    AccentObjectColorSelector colorSelector;

    private void Start()
    {
        EnvironmentConfigManager.instance.registerConfigSubUI(gameObject);
    }

    public void updateUIElements(EnvironmentOptions options)
    {
        amountSlider.setValue(options.colorfullness.amount);
        colorSelector.select(options.colorfullness.color);
    }

    public void handleColorfulnessAmountChange(float newValue)
    {
        EnvironmentConfigManager.instance.updateColorfullnessAmount(newValue);
    }

    public void handleColorfulnessAmountChangeEnd()
    {
        UsageLogger.log(UserAction.ENVIRONMENT_COLORFULLNESS_AMOUNT_CHANGED);
        EnvironmentConfigManager.instance.lastUpdateColorfullnessAmount();
    }
}
