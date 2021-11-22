using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class LocationSelector : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GridObjectCollection gridCollection;

    private Dictionary<string, Interactable> buttonInteractables;

    private void Start()
    {
        buttonInteractables = new Dictionary<string, Interactable>();

        //get the locations from the Environment Manager
        //instantiate buttons with proper handlers for each one
        //lay them out nicely
        foreach (string name in EnvironmentConfigManager.instance.environmentNames)
        {
            GameObject btn = Instantiate(buttonPrefab, gridCollection.transform);
            ButtonConfigHelper btnConfig = btn.GetComponent<ButtonConfigHelper>();
            if (btnConfig != null)
            {
                btnConfig.MainLabelText = name;
                btnConfig.IconStyle = ButtonIconStyle.None;
                btnConfig.OnClick.AddListener(() => { handleSelection(name); });

                buttonInteractables.Add(name, btnConfig.gameObject.GetComponent<Interactable>());
            }
        }
        gridCollection.UpdateCollection();
    }

    private void handleSelection(string name)
    {
        EnvironmentConfigManager.instance.selectEnvironment(name);
        UsageLogger.log(UserAction.ENVIRONMENT_LOCATION_SELECTED);
        foreach (var n in buttonInteractables.Keys)
        {
            buttonInteractables[n].IsToggled = n == name;
        }
    }
}
