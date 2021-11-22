using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StepUpdatedEvent : UnityEvent<int>
{
}

public class ValueStepCtrl : MonoBehaviour
{
    private int value;
    public int initialValue = 0;

    public TMPro.TextMeshPro valueTextField;

    public StepUpdatedEvent OnValueChanged;

    private void Start()
    {
        value = initialValue;
        valueTextField.text = "" + value;
    }

    public void increase()
    {
        value++;
        valueTextField.text = "" + value;
        notifyListeners();
    }

    public void decrease()
    {
        value--;
        valueTextField.text = "" + value;
        notifyListeners();
    }

    public void setValue(int value)
    {
        setValueSilently(value);
        notifyListeners();
    }

    public void setValueSilently(int value)
    {
        this.value = value;
        valueTextField.text = "" + this.value;
    }

    private void notifyListeners()
    {
        OnValueChanged.Invoke(value);
    }
}
