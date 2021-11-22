using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class AccentObjectColorSelector : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GridObjectCollection gridCollection;

    private Dictionary<AccentObjectColor, Interactable> buttonInteractables;

    private void Awake()
    {
        buttonInteractables = new Dictionary<AccentObjectColor, Interactable>();

        foreach (AccentObjectColor color in System.Enum.GetValues(typeof(AccentObjectColor)))
        {
            GameObject btn = Instantiate(buttonPrefab, gridCollection.transform);
            ButtonConfigHelper btnConfig = btn.GetComponent<ButtonConfigHelper>();
            if (btnConfig != null)
            {
                btnConfig.MainLabelText = System.Enum.GetName(typeof(AccentObjectColor), color);
                btnConfig.OnClick.AddListener(() => { handleSelection(color); });

                buttonInteractables.Add(color, btnConfig.gameObject.GetComponent<Interactable>());
            }
        }

        gridCollection.UpdateCollection();
    }

    private void handleSelection(AccentObjectColor color, bool fromCode = false)
    {
        EnvironmentConfigManager.instance.updateColorfullnessColor(color);
        if (!fromCode) UsageLogger.log(UserAction.ENVIRONMENT_COLORFULLNESS_COLOR_CHANGED);
        foreach(var c in buttonInteractables.Keys)
        {
            buttonInteractables[c].IsToggled = c == color;
        }
    }

    public void select(AccentObjectColor color)
    {
        handleSelection(color, true);
    }
}
