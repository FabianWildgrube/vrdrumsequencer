using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class Keyboard : MonoBehaviour
{
    public event Action<string> OnTextCommitted;
    public event Action OnKeyboardShow;
    public event Action OnKeyboardHide;

    [SerializeField]
    TMPro.TextMeshPro titleTextfield;
    [SerializeField]
    TMPro.TextMeshPro editedTextfield;

    [SerializeField]
    GameObject tutorialContent;

    [SerializeField]
    ButtonConfigHelper deleteBtn;
    [SerializeField]
    ButtonConfigHelper enterBtn;
    [SerializeField]
    ButtonConfigHelper cancelBtn;
    [SerializeField]
    ButtonConfigHelper clearAllBtn;
    [SerializeField]
    ButtonConfigHelper toggleCaseBtn;
    private Interactable toggleCaseBtnInteractable;
    [SerializeField]
    ButtonConfigHelper toggleAltCharsetBtn;
    private Interactable toggleAltCharsetBtnInteractable;


    FollowMeToggle followToggle;

    private KeyboardCharacterKey[] characterKeys;

    private bool isFirstCharKeyPress = true;

    private bool _allUpperCase = true;
    private bool charactersAreUppercase
    {
        get { return _allUpperCase; }
        set
        {
            _allUpperCase = value;
            toggleCaseBtnInteractable.IsToggled = value;
            foreach (KeyboardCharacterKey key in characterKeys)
            {
                if (_allUpperCase)
                {
                    key.setToUpperCase();
                }
                else
                {
                    key.setToLowerCase();
                }
            }
        }
    }

    private bool _alternativeCharactersUsed = false;
    private bool charactersAreAlternative
    {
        get { return _alternativeCharactersUsed; }
        set
        {
            _alternativeCharactersUsed = value;
            toggleAltCharsetBtnInteractable.IsToggled = value;
            foreach (KeyboardCharacterKey key in characterKeys)
            {
                if (_alternativeCharactersUsed)
                {
                    key.setToAltCharacter();
                }
                else
                {
                    key.setToBaseCharacter();
                }
            }
        }
    }

    private void Awake()
    {
        characterKeys = GetComponentsInChildren<KeyboardCharacterKey>();
        foreach (KeyboardCharacterKey key in characterKeys)
        {
            key.OnKeyPressed += this.handleCharKeyPress;
        }

        deleteBtn.OnClick.AddListener(this.deleteLastChar);
        enterBtn.OnClick.AddListener(this.commitInput);
        cancelBtn.OnClick.AddListener(this.hide);
        clearAllBtn.OnClick.AddListener(this.clearAll); 
        toggleCaseBtn.OnClick.AddListener(this.toggleCharacterCase);
        toggleAltCharsetBtn.OnClick.AddListener(this.toggleAlternativeCharacters);

        followToggle = GetComponent<FollowMeToggle>();
        toggleCaseBtnInteractable = toggleCaseBtn.gameObject.GetComponent<Interactable>();
        toggleAltCharsetBtnInteractable = toggleAltCharsetBtn.gameObject.GetComponent<Interactable>();
    }

    private void OnDestroy()
    {
        foreach (KeyboardCharacterKey key in characterKeys)
        {
            if (key) key.OnKeyPressed -= this.handleCharKeyPress;
        }

        deleteBtn.OnClick.RemoveListener(this.deleteLastChar);
        enterBtn.OnClick.RemoveListener(this.commitInput);
        cancelBtn.OnClick.RemoveListener(this.hide);
        clearAllBtn.OnClick.RemoveListener(this.clearAll);
        toggleCaseBtn.OnClick.RemoveListener(this.toggleCharacterCase);
        toggleAltCharsetBtn.OnClick.RemoveListener(this.toggleAlternativeCharacters);
    }

    public void showForTutorial(string text = "", string title = "")
    {
        if (tutorialContent != null) tutorialContent.SetActive(true);
        show(text, title);
    }

    public void show(string text = "", string title = "", bool initiallyUpperCase = true, bool initiallyAltCharacters = false)
    {
        Debug.Log("Keyboard Show");
        titleTextfield.text = title;
        editedTextfield.text = text;

        gameObject.SetActive(true);
        charactersAreUppercase = initiallyUpperCase;
        charactersAreAlternative = initiallyAltCharacters;
        isFirstCharKeyPress = true;

        followToggle.SetFollowMeBehavior(true); //clear any pinning the user might have done with the keyboard earlier -> ensures that the keyboard is near the user
        if (OnKeyboardShow != null) OnKeyboardShow();
    }

    public void hide()
    {
        Debug.Log("Keyboard Hide");
        editedTextfield.text = "";
        if (tutorialContent != null) tutorialContent.SetActive(false);
        gameObject.SetActive(false);
        if (OnKeyboardHide != null) OnKeyboardHide();
    }

    private void deleteLastChar()
    {
        if (editedTextfield.text.Length > 0)
        {
            editedTextfield.text = editedTextfield.text.Substring(0, editedTextfield.text.Length - 1);
        }
    }

    private void commitInput()
    {
        Debug.Log("Keyboard Commit: " + editedTextfield.text);
        if (OnTextCommitted != null) OnTextCommitted(editedTextfield.text);
        hide();
    }

    private void handleCharKeyPress(string character)
    {
        editedTextfield.text += character;
        autoToggleCaseOnFirstChar();
    }

    private void autoToggleCaseOnFirstChar()
    {
        if (isFirstCharKeyPress)
        {
            isFirstCharKeyPress = false;
            if (charactersAreUppercase)
            {
                charactersAreUppercase = false;
            }
        }
    }

    private void clearAll()
    {
        editedTextfield.text = "";
    }

    private void toggleCharacterCase()
    {
        charactersAreUppercase = !charactersAreUppercase;
    }

    private void toggleAlternativeCharacters()
    {
        charactersAreAlternative = !charactersAreAlternative;
    }
}
