using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "PFS-VR/EnvironmentConfig", order = 2)]
public class EnvironmentOptionsScriptableObject : ScriptableObject
{
    public ComplexityOptions complexity;
    public LightingOptions lighting;
    public ColorfullnessOptions colorfullness;
    [Range(0f, 1f)]
    public float ambientSoundVolume;

    public EnvironmentOptions convert()
    {
        EnvironmentOptions options = new EnvironmentOptions();
        options.complexity = complexity;
        options.lighting = lighting;
        options.colorfullness = colorfullness;
        options.ambientSoundVolume = ambientSoundVolume;

        return options;
    }
}
