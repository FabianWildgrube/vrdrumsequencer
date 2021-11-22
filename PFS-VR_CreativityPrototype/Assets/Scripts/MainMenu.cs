using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component that activates when the controller Menu Button is pressed and must be deactivated through e.g. a VR Button Press
/// (this activation is triggered by an InputActionHandler component on the "Managers" GameObject, that listens for the "Menu" MRTK Input action)
/// 
/// Shows an Explanation screen the first time it is summoned and shows explanations for the menu's buttons when they are clicked as long as the explanation screen is visible.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    public FollowMeToggle followToggle;
    public RadialView tagAlong;

    public GameObject menuTray;

    [Header("Explanation Screen Components")]
    public GameObject explanationScreen;
    public MainMenuExplanation mainMenuExplanation;

    [Header("Main Experiment Screen Components")]
    public GameObject mainExperimentScreen;
    public Transform mainExperimentScreenDefaultTransform;
    public Interactable[] mainExperimentScreenButtons;

    [Header("Environment Configuration Screen Components")]
    public GameObject environmentConfigScreen;
    public Transform environmentConfigScreenDefaultTransform;
    public Interactable[] environmentConfigScreenButtons;

    public GameObject buttonsCollectionWithEnvironment;
    public GameObject buttonsCollectionWithoutEnvironment;

    public UnityEvent OnGoBackHomeRequest;
    public UnityEvent OnBringInstrumentHereRequest;

    private bool menuVisible => menuTray.activeSelf;
    private bool mainExperimentScreenVisible => mainExperimentScreen.activeSelf;
    private bool environmentConfigScreenVisible => environmentConfigScreen.activeSelf;
    private bool explanationScreenVisible => explanationScreen.activeSelf;

    private bool environmentConfigSelectable => buttonsCollectionWithEnvironment.activeSelf;

    private bool firstTime = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            followToggle.SetFollowMeBehavior(menuVisible);
        }
    }

    private void Start()
    {
        ExperimentManager.instance.OnConditionChanged += handleConditionChange;

        if (ExperimentManager.instance.currentCondition == ExperimentCondition.CUSTOM_ENVIRONMENT) enableEnvironmentConfig();
    }

    private void OnDestroy()
    {
        ExperimentManager.instance.OnConditionChanged -= handleConditionChange;
    }

    private void handleConditionChange(ExperimentCondition newCondition)
    {
        if (newCondition == ExperimentCondition.CUSTOM_ENVIRONMENT)
        {
            enableEnvironmentConfig();
        } else { 
            disableEnvironmentConfig();
        }
    }

    public void Show()
    {
        menuTray.SetActive(true);

        if (firstTime)
        {
            firstTime = false;
            followToggle.SetFollowMeBehavior(true);
            if (ExperimentManager.instance.currentExperimentStage == ExperimentStage.INTRO_VR) showExplanationScreen();
        }
    }

    public void Hide()
    {
        //clear any pinning to make sure the follow toggle keeps moving even when the tray is not shown
        //This enables the screens to pop up near the user when the experiment stage changes
        followToggle.SetFollowMeBehavior(true);
        menuTray.SetActive(false);
    }

    public void toggleActive()
    {
        if (firstTime)
        {
            Show();
            UsageLogger.log(UserAction.MAIN_MENU_OPENED);
        }
        else
        {
            if (menuVisible) Hide();
            else Show();
            UsageLogger.log(menuVisible ? UserAction.MAIN_MENU_CLOSED : UserAction.MAIN_MENU_OPENED);
        }
    }

    public void forceMoveMenuOrOpenScreenToUser()
    {
        bool reopenExperimentGuide = false;
        bool reopenEnvironmentConfig = false;
        bool reopenMenu = menuVisible;

        if (mainExperimentScreenVisible) {
            hideMainExperimentScreen();
            reopenExperimentGuide = true;
        } else if (environmentConfigScreenVisible) {
            hideEnvironmentScreen();
            reopenEnvironmentConfig = true;
        }

        float oldMoveLerpTime = tagAlong.MoveLerpTime;
        float oldRotateLerpTime = tagAlong.RotateLerpTime;

        //ensure an immediate jump to the user
        tagAlong.MoveLerpTime = 0f;
        tagAlong.RotateLerpTime = 0f;
        followToggle.SetFollowMeBehavior(true);

        //wait a little to make sure the tag along is near the user
        StartCoroutine(executeDelayed(0.1f, () =>
        {
            if (reopenExperimentGuide) showMainExperimentScreen();
            else if (reopenEnvironmentConfig) showEnvironmentScreen();

            if (reopenMenu) Show();
            tagAlong.MoveLerpTime = oldMoveLerpTime;
            tagAlong.RotateLerpTime = oldRotateLerpTime;
        }));
    }

    public void showExperimentGuideTriggeredByUser()
    {
        showMainExperimentScreen();
        if(!explanationScreenVisible) UsageLogger.log(UserAction.EXPERIMENT_GUIDE_OPENED);
    }

    public void hideExperimentGuideTriggeredByUser()
    {
        hideMainExperimentScreen();
        if (!explanationScreenVisible) UsageLogger.log(UserAction.EXPERIMENT_GUIDE_CLOSED);
    }

    public void showEnvironmentConfigTriggeredByUser()
    {
        showEnvironmentScreen();
        if (!explanationScreenVisible) UsageLogger.log(UserAction.ENVIRONMENT_CONFIG_MENU_OPENED);
    }

    public void hideEnvironmentConfigTriggeredByUser()
    {
        hideEnvironmentScreen();
        if (!explanationScreenVisible) UsageLogger.log(UserAction.ENVIRONMENT_CONFIG_MENU_CLOSED);
    }

    public void finishExplanations()
    {
        explanationScreen.SetActive(false);
        showMainExperimentScreen();
    }

    private void showExplanationScreen()
    {
        hideMainExperimentScreen();
        hideEnvironmentScreen();

        explanationScreen.SetActive(true);
    }

    private IEnumerator executeDelayed(float delay, System.Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback();
    }

    public void showMainExperimentScreen()
    {
        if (explanationScreenVisible)
        {
            mainMenuExplanation.showExperimentGuideExplanation();
            foreach (var btn in mainExperimentScreenButtons) btn.IsToggled = false; //don't toggle on press when it's only for explanation
        } else
        {
            if (menuVisible) Hide();

            hideEnvironmentScreen();

            mainExperimentScreen.SetActive(true);
            foreach (var btn in mainExperimentScreenButtons) btn.IsToggled = true;
            mainExperimentScreen.transform.position = mainExperimentScreenDefaultTransform.position;
            mainExperimentScreen.transform.rotation = mainExperimentScreenDefaultTransform.rotation;
        }
    }

    public void hideMainExperimentScreen()
    {
        mainExperimentScreen.gameObject.SetActive(false);
        foreach (var btn in mainExperimentScreenButtons) btn.IsToggled = false;
    }

    public void enableEnvironmentConfig()
    {
        buttonsCollectionWithoutEnvironment.SetActive(false);
        buttonsCollectionWithEnvironment.SetActive(true);
    }

    public void disableEnvironmentConfig()
    {
        buttonsCollectionWithoutEnvironment.SetActive(true);
        buttonsCollectionWithEnvironment.SetActive(false);
        hideEnvironmentScreen();
    }

    public void showEnvironmentScreen()
    {
        if (!explanationScreenVisible) //environment config is not explained during intro -> this handler shouldn't be called then anyway
        {
            if (menuVisible) Hide();

            hideMainExperimentScreen();

            environmentConfigScreen.SetActive(true);
            foreach (var btn in environmentConfigScreenButtons) btn.IsToggled = true;
            environmentConfigScreen.transform.position = environmentConfigScreenDefaultTransform.position;
            environmentConfigScreen.transform.rotation = environmentConfigScreenDefaultTransform.rotation;
        }
    }

    public void hideEnvironmentScreen()
    {
        environmentConfigScreen.gameObject.SetActive(false);
        foreach (var btn in environmentConfigScreenButtons) btn.IsToggled = false;
    }

    public void handleGoBackHomeBtn()
    {
        if (explanationScreenVisible)
        {
            mainMenuExplanation.showGoBackHomeExplanation();
        } else
        {
            Hide();
            OnGoBackHomeRequest.Invoke();
            UsageLogger.log(UserAction.JUMPED_BACK_TO_INSTRUMENT);
        }
    }

    public void handleMoveInstrumentBtn()
    {
        if (explanationScreenVisible)
        {
            mainMenuExplanation.showBringInstrumentExplanation();
        }
        else
        {
            Hide();
            OnBringInstrumentHereRequest.Invoke();
            UsageLogger.log(UserAction.MOVED_INSTRUMENT_TO_PLAYSPACE);
        }
    }
}
