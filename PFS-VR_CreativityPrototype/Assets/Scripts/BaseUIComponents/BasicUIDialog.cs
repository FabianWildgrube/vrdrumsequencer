using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasicUIDialog : MonoBehaviour
{
    public string heading
    {
        get => headingTxtField.text;
        set => headingTxtField.text = value;
    }

    public string description
    {
        get => decriptionTxtField.text;
        set => decriptionTxtField.text = value;
    }

    [SerializeField]
    TMPro.TextMeshPro headingTxtField;
    [SerializeField]
    TMPro.TextMeshPro decriptionTxtField;

    public UnityEvent OnConfirmed;
    public UnityEvent OnCanceled;

    // Start is called before the first frame update
    void Start()
    {
        headingTxtField.text = heading;
        decriptionTxtField.text = description;
    }

    public void Confirm()
    {
        if (OnConfirmed != null) OnConfirmed.Invoke();
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        if (OnCanceled != null) OnCanceled.Invoke();
        gameObject.SetActive(false);
    }
}
