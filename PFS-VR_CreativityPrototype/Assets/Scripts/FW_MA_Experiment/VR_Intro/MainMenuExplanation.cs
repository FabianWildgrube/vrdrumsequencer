using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuExplanation : MonoBehaviour
{
    public GameObject defaultExplanationText;
    public GameObject experimentGuideExplanationText;
    public GameObject bringInstrumentExplanationText;
    public GameObject goBackHomeExplanationText;
    public GameObject okBtn;

    private bool experimentGuideExplanationViewed = false;
    private bool bringInstrumentExplanationViewed = false;
    private bool goBackHomeExplanationViewed = false;

    private void Start()
    {
        okBtn.SetActive(false);
    }

    private void hideAll()
    {
        defaultExplanationText.SetActive(false);
        experimentGuideExplanationText.SetActive(false);
        bringInstrumentExplanationText.SetActive(false);
        goBackHomeExplanationText.SetActive(false);
}

    private void checkAllSeen()
    {
        if (experimentGuideExplanationViewed && bringInstrumentExplanationViewed && goBackHomeExplanationViewed)
        {
            okBtn.SetActive(true);
        }
    }

    public void showExperimentGuideExplanation()
    {
        hideAll();
        experimentGuideExplanationText.SetActive(true);
        experimentGuideExplanationViewed = true;
        checkAllSeen();
    }

    public void showGoBackHomeExplanation()
    {
        hideAll();
        goBackHomeExplanationText.SetActive(true);
        goBackHomeExplanationViewed = true;
        checkAllSeen();
    }

    public void showBringInstrumentExplanation()
    {
        hideAll();
        bringInstrumentExplanationText.SetActive(true);
        bringInstrumentExplanationViewed = true;
        checkAllSeen();
    }
}
