using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(ButtonConfigHelper))]
public class KeyboardCharacterKey : MonoBehaviour
{
    public string baseCharacter = "";
    public string alternativeCharacter = "";
    private bool isUpperCase = true;
    private bool isAlt = false;
    private string currentCharacter
    {
        get
        {
            string baseCharCased = isUpperCase ? baseCharacter.ToUpper() : baseCharacter.ToLower();
            return isAlt ? alternativeCharacter : baseCharCased;
        }
    }

    private ButtonConfigHelper btnConfig;

    public event System.Action<string> OnKeyPressed;

    void Awake()
    {
        btnConfig = GetComponent<ButtonConfigHelper>();
        btnConfig.MainLabelText = baseCharacter;
        btnConfig.OnClick.AddListener(this.sendKeyPressUpwards);
    }

    private void OnDestroy()
    {
        btnConfig.OnClick.RemoveListener(this.sendKeyPressUpwards);
    }

    public void setToUpperCase()
    {
        isUpperCase = true;
        updateLabel();
    }

    public void setToLowerCase()
    {
        isUpperCase = false;
        updateLabel();
    }

    public void setToAltCharacter()
    {
        isAlt = true;
        updateLabel();
    }

    public void setToBaseCharacter()
    {
        isAlt = false;
        updateLabel();
    }

    private void updateLabel()
    {
        btnConfig.MainLabelText = currentCharacter;
    }

    private void sendKeyPressUpwards()
    {
        if (OnKeyPressed != null) OnKeyPressed(currentCharacter);
    }
}
