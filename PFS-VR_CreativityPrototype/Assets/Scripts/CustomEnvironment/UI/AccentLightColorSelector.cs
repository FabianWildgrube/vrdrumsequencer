using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class AccentLightColorSelector : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GridObjectCollection gridCollection;

    private Dictionary<AccentLightColor, Interactable> buttonInteractables;

    private void Awake()
    {
        buttonInteractables = new Dictionary<AccentLightColor, Interactable>();

        foreach (AccentLightColor color in System.Enum.GetValues(typeof(AccentLightColor)))
        {
            GameObject btn = Instantiate(buttonPrefab, gridCollection.transform);
            ButtonConfigHelper btnConfig = btn.GetComponent<ButtonConfigHelper>();
            if (btnConfig != null)
            {
                btnConfig.MainLabelText = System.Enum.GetName(typeof(AccentLightColor), color);
                btnConfig.OnClick.AddListener(() => { handleSelection(color); });

                buttonInteractables.Add(color, btnConfig.gameObject.GetComponent<Interactable>());
            }
        }

        gridCollection.UpdateCollection();
    }

    private void handleSelection(AccentLightColor color, bool fromCode = false)
    {
        EnvironmentConfigManager.instance.updateAccentLightsColor(color);
        if (!fromCode) UsageLogger.log(UserAction.ENVIRONMENT_LIGHTING_ACCENT_COLOR_CHANGED);
        foreach (var c in buttonInteractables.Keys)
        {
            buttonInteractables[c].IsToggled = c == color;
        }
    }

    public void select(AccentLightColor color)
    {
        handleSelection(color, true);
    }
}
