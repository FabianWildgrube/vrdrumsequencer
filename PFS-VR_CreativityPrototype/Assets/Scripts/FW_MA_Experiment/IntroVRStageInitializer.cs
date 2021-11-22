using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroVRStageInitializer : MonoBehaviour, IStageInitializer
{
    public GameObject maxTimeWarningDialog;
    public float maxTimeWarningShowTime = 10f; //in minutes

    private Coroutine runningTimer;
    private GameObject openTimerWindow;

    public void InitExperimentStage(ExperimentStage stage)
    {
        if (runningTimer != null) StopCoroutine(runningTimer);
        closeTimerWarning();

        scheduleMaxTimeWarning();
    }

    private IEnumerator scheduleMaxTimeWarning()
    {
        yield return new WaitForSeconds(maxTimeWarningShowTime);
        maxTimeWarningDialog.SetActive(true);
        openTimerWindow = maxTimeWarningDialog;
        runningTimer = null;
    }

    public void closeTimerWarning()
    {
        if (openTimerWindow != null)
        {
            openTimerWindow.SetActive(false);
            openTimerWindow = null;
        }
    }
}
