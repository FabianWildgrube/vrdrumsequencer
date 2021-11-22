using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardExample : MonoBehaviour
{
    public TMPro.TextMeshPro textToEdit;
    

    public void startEditFlow()
    {
        KeyboardManager.instance.keyboard.showForTutorial(textToEdit.text, "Teste die Tastatur");
        KeyboardManager.instance.keyboard.OnTextCommitted += this.handleEditCommit;
        KeyboardManager.instance.keyboard.OnKeyboardHide += this.handleEditCancel;
    }

    private void handleEditCancel()
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleEditCommit;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleEditCancel;
    }

    private void handleEditCommit(string newText)
    {
        KeyboardManager.instance.keyboard.OnTextCommitted -= this.handleEditCommit;
        KeyboardManager.instance.keyboard.OnKeyboardHide -= this.handleEditCancel;
        textToEdit.text = newText;
    }
}
