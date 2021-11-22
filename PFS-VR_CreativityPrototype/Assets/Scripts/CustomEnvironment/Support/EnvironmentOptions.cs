using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ComplexityOptions
{
    [Range(0f, 1f)]
    public float objects;
    [Range(0f, 1f)]
    public float tidyness;
}

[System.Serializable]
public enum AccentLightColor
{
    BLAU,
    ROT,
    GRÜN
}

[System.Serializable]
public struct AccentLightsOptions
{
    [Range(0f, 1f)]
    public float amount;
    public AccentLightColor color;
    [Range(0f, 1f)]
    public float intensity;
}

[System.Serializable]
public struct LightingOptions
{
    [Range(0f, 1f)]
    public float tod;
    [Range(0f, 1f)]
    public float colorTemp;
    [Range(0f, 1f)]
    public float intensity;
    public AccentLightsOptions accentLights;
}

[System.Serializable]
public enum AccentObjectColor
{
    BLAUGRÜN,
    ROTORANGE
}

[System.Serializable]
public struct ColorfullnessOptions
{
    public AccentObjectColor color;
    [Range(0f, 1f)]
    public float amount;
}

public struct EnvironmentOptions
{
    public ComplexityOptions complexity;
    public LightingOptions lighting;
    public ColorfullnessOptions colorfullness;
    [Range(0f, 1f)]
    public float ambientSoundVolume;

    public static EnvironmentOptions getDefaults()
    {
        EnvironmentOptions options = new EnvironmentOptions();
        options.complexity.objects = 0.5f;
        options.complexity.tidyness = 0.5f;
        options.lighting = new LightingOptions();
        options.lighting.tod = 0.5f;
        options.lighting.colorTemp = 0.5f;
        options.lighting.intensity = 0.5f;
        options.lighting.accentLights = new AccentLightsOptions();
        options.lighting.accentLights.amount = 0.5f;
        options.lighting.accentLights.color = AccentLightColor.BLAU;
        options.lighting.accentLights.intensity = 0.5f;
        options.colorfullness.amount = 0.5f;
        options.colorfullness.color = AccentObjectColor.BLAUGRÜN;
        options.ambientSoundVolume = 0.5f;

        return options;
    }
}
