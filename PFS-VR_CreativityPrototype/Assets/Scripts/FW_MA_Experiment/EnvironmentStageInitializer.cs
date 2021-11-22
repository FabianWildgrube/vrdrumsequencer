using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class EnvironmentStageInitializer : MonoBehaviour, IStageInitializer
{
    public void InitExperimentStage(ExperimentStage stage)
    {
        if (stage == ExperimentStage.ENVIRONMENT_CONFIGURATION)
        {
            MixedRealityPlayspace.Position = Vector3.zero; //make sure user is at world origin, because all environments are built around world origin
            LoopManager.instance.moveLoopToDefaultPositionNearOrigin(); //also have the loop near the origin because it could be anywhere, depending on what the user did with it previously
            MainMenu.instance.forceMoveMenuOrOpenScreenToUser();
        }
    }
}
