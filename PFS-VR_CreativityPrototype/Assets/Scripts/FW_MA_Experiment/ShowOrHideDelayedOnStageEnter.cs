using System.Collections;
using System.Linq;
using UnityEngine;

public enum DelayMode
{
    SHOW_AFTER_DELAY,
    HIDE_AFTER_DELAY
}

public class ShowOrHideDelayedOnStageEnter : MonoBehaviour, IStageInitializer
{
    public GameObject[] objectsToShowOrHide;
    public DelayMode delayMode = DelayMode.SHOW_AFTER_DELAY;
    public float delayInSeconds = 60f;
    [Tooltip("Restrict execution to certain stages. If none are given it will execute in all stages that this object belongs to.")]
    public ExperimentStage[] onlyTheseStages;

    private float startTime = -1f;
    private bool showScheduled = false;

    public void Start()
    {
        foreach (var obj in objectsToShowOrHide) obj.SetActive(delayMode == DelayMode.HIDE_AFTER_DELAY);
    }

    public void InitExperimentStage(ExperimentStage stage)
    {
        if (onlyTheseStages.Length == 0 || onlyTheseStages.Contains(ExperimentManager.instance.currentExperimentStage))
        {
            //schedule
            showScheduled = true;
            startTime = Time.time;
        }
    }

    private void Update()
    {
        float delay = ExperimentManager.instance.ignoreStageInitDelayComponents ? 2f : delayInSeconds;
        if (showScheduled && (Time.time > startTime + delay))
        {
            showOrHideDelayed();
            showScheduled = false;
        }
    }

    private void showOrHideDelayed()
    {
        if (onlyTheseStages.Length == 0 || onlyTheseStages.Contains(ExperimentManager.instance.currentExperimentStage))
        {
            foreach (var obj in objectsToShowOrHide) obj.SetActive(delayMode == DelayMode.SHOW_AFTER_DELAY);
        }
    }
}
