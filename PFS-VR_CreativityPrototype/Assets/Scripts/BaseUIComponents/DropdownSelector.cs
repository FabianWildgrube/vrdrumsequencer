using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class DropdownSelector<Values_T> : MonoBehaviour where Values_T : System.Enum
{
    [Header("Non-Expanded Elements")]
    public TMPro.TextMeshPro valueLabel;
    public ButtonConfigHelper expandButton;

    [Header("Expanadble Option List")]
    public GameObject buttonPrefab;
    public GameObject container;
    public GridObjectCollection gridCollection;
    public ScrollingObjectCollection scrollingView;
    //public GameObject background;
    public float backgroundPadding = 0.005f;

    protected System.Action<Values_T> OnSelectionChanged;

    private Dictionary<Values_T, Interactable> buttonInteractables;
    private Interactable expandBtnInteractable;

    public void Awake()
    {
        buttonInteractables = new Dictionary<Values_T, Interactable>();

        var values = System.Enum.GetValues(typeof(Values_T));

        foreach (Values_T value in values)
        {
            GameObject btn = Instantiate(buttonPrefab, gridCollection.transform);
            ButtonConfigHelper btnConfig = btn.GetComponent<ButtonConfigHelper>();
            if (btnConfig != null)
            {
                btnConfig.MainLabelText = System.Enum.GetName(typeof(Values_T), value);
                btnConfig.OnClick.AddListener(() => { handleSelection(value); });

                buttonInteractables.Add(value, btnConfig.gameObject.GetComponent<Interactable>());
            }
        }

        gridCollection.UpdateCollection();
        scrollingView.UpdateContent();

        expandButton.OnClick.AddListener(this.handleExpandClick);
        expandBtnInteractable = expandButton.gameObject.GetComponent<Interactable>();
    }

    public void ShowOptions()
    {
        container.SetActive(true);
        expandBtnInteractable.IsToggled = true;
    }

    public void HideOptions()
    {
        container.SetActive(false);
        expandBtnInteractable.IsToggled = false;
    }

    private void handleExpandClick()
    {
        if (container.activeInHierarchy)
        {
            HideOptions();
        } else
        {
            ShowOptions();
        }
    }

    protected void handleSelection(Values_T value)
    {
        valueLabel.text = System.Enum.GetName(typeof(Values_T), value);
        if (OnSelectionChanged != null) OnSelectionChanged(value);
        foreach (var v in buttonInteractables.Keys)
        {
            buttonInteractables[v].IsToggled = v.CompareTo(value) == 0;
        }
        HideOptions();
    }
}
