using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopElementsStageInitializer : MonoBehaviour, IStageInitializer
{
    public void InitExperimentStage(ExperimentStage stage)
    {
        //TODO: during the environment config the loop should not be interactable

        if (stage == ExperimentStage.EXPLORATION_01 || stage == ExperimentStage.EXPLORATION_02 ||
            stage == ExperimentStage.INTRO_PFS_USER || stage == ExperimentStage.INTRO_PFS_CONDUCTOR)
        {
            //start with a fresh loop in these stages
            LoopManager.instance.loadDefaultLoop();
        }

        if (stage == ExperimentStage.PERFECTION_01 || stage == ExperimentStage.PERFECTION_02)
        {
            //force user to choose loop from disk in these stages
            LoopManager.instance.clearCurrentLoop();
        }
    }
}
