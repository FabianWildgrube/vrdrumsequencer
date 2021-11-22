using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentConfigMenu : MonoBehaviour
{
    public GameObject finishBtn; //should only be visible, once all tabs have been shown

    private bool allTabsViewed = false;
    private bool locationSelected = false;

    private bool finishBtnWasShownByThisScript = false; //flag to prevent finish button from being shown more than exactly once

    void Start()
    {
        finishBtn.SetActive(false);
        EnvironmentConfigManager.instance.OnEnvironmentChanged += registerLocationSelected;
    }

    private void OnDestroy()
    {
        EnvironmentConfigManager.instance.OnEnvironmentChanged -= registerLocationSelected;
    }

    public void registerAllTabsViewed()
    {
        allTabsViewed = true;
        showFinishBtnIfAllowed();
    }

    public void registerLocationSelected()
    {
        if (ExperimentManager.instance.currentExperimentStage == ExperimentStage.ENVIRONMENT_CONFIGURATION)
        {
            locationSelected = true;
            showFinishBtnIfAllowed();
        }
    }

    private void showFinishBtnIfAllowed()
    {
        if (!finishBtnWasShownByThisScript
            && allTabsViewed
            && locationSelected)
        {
            finishBtn.SetActive(true);
            finishBtnWasShownByThisScript = true;
        }
    }
}
