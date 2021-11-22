using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

[System.Serializable]
public class SliderUpdatedEvent : UnityEvent<float>
{
}

[System.Serializable]
public class SliderManipulationEndStateEvent : UnityEvent<float>
{
}


public class ValueSliderCtrl : MonoBehaviour
{
    public TMPro.TextMeshPro valueTextfield;
    public float minValue = 0;
    public float maxValue = 1;
    public float displayOffset = 0;
    public float initialValue = 0.5f;
    public int displayPrecision = 0;

    public PinchSlider slider;

    public SliderUpdatedEvent OnValueChanged;

    public UnityEvent OnInteractionEnded;
    public SliderManipulationEndStateEvent OnInteractionEndedWithValue;

    private bool valueWasSet = false;

    private void Start()
    {
        if (!valueWasSet) slider.SliderValue = normalized(initialValue); //hotfix to prevent slider from overriding value set before this start() is called
        valueTextfield.text = "" + initialValue.ToString("F" + displayPrecision);
    }

    public void onSliderValueChanged(SliderEventData data)
    {
        float newData = denormalized(data.NewValue);
        valueTextfield.text = "" + newData.ToString("F" + displayPrecision);
        OnValueChanged.Invoke(newData);
    }

    public void handleInteractionEnded(SliderEventData data)
    {
        OnInteractionEnded?.Invoke();
        OnInteractionEndedWithValue?.Invoke(denormalized(data.NewValue));
    }

    public void setValue(float newValue)
    {
        valueWasSet = true; //hotfix to prevent slider from overriding value set before this start() is called
        slider.SliderValue = normalized(newValue);
    }

    private float normalized(float realWorldValue)
    {
        return (realWorldValue - minValue) / (maxValue - minValue);
    }

    private float denormalized(float normalizedValue)
    {
        return minValue + normalizedValue * (maxValue - minValue) + displayOffset;
    }
}
