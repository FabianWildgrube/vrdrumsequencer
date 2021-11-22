using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class ColorfulObject
{
    public GameObject theObject;
    public AccentObjectColorMapping[] colorMappings;
}

[System.Serializable]
public class AccentLightObject
{
    public GameObject theObject;
    public AccentLightColorMapping[] colorMappings;
}

public class RecordStudioEnvironment : MonoBehaviour, ICostumizableEnvironment
{
    public GameObject mainModel;

    public ReflectionProbe[] reflectionProbes;

    [Header("Visual Complexity")]
    public GameObject[] complexityObjects;

    [Header("Lights")]
    public Light[] mainLights;
    public AccentLight[] accentLights;
    public AccentLightObject[] accentLightObjects;

    [Header("Colorfulness")]
    public GameObject[] colorfulObjects;
    public ColorfulObject[] specialColorfulObjects;

    [Header("Sound")]
    public AudioClip ambientSound;
    public AudioSource ambientSoundSource;

    [Header("ColorMappings")]
    public EnvironmentColorMapping colorMapping;

    private EnvironmentOptions options;

    private Dictionary<Light, float> mainLightsMaxIntensity;

    private AccentLight[] randomizedAccentLights;

    private GameObject[] randomizedComplexityObjects;

    private void Awake()
    {
        mainLightsMaxIntensity = new Dictionary<Light, float>();
        foreach (Light mainLight in mainLights)
        {
            mainLightsMaxIntensity.Add(mainLight, mainLight.intensity);
        }

        System.Random rnd = new System.Random(10); //fixed seed to ensure equal "randomness" for all study participants
        randomizedAccentLights = accentLights.OrderBy(x => rnd.Next()).ToArray();
        randomizedComplexityObjects = complexityObjects.OrderBy(x => rnd.Next()).ToArray();
    }

    public void activate()
    {
        if (ambientSoundSource != null)
        {
            ambientSoundSource.clip = ambientSound;
            ambientSoundSource.Play();
        }
    }

    /// Sets the options and triggers a relayout of all environment properties
    public void set(EnvironmentOptions newOptions)
    {
        options = newOptions;
        //update all values
        updateComplexityObjects(newOptions.complexity.objects);
        updateTidyness(newOptions.complexity.tidyness);
        updateTimeOfDay(newOptions.lighting.tod);
        updateMainLightingColorTemp(newOptions.lighting.colorTemp);
        updateMainLightingIntensity(newOptions.lighting.intensity);
        updateAccentLightsAmount(newOptions.lighting.accentLights.amount);
        updateAccentLightsColor(newOptions.lighting.accentLights.color);
        updateAccentLightsIntensity(newOptions.lighting.accentLights.intensity);
        updateColorfullnessColor(newOptions.colorfullness.color);
        updateColorfullnessAmount(newOptions.colorfullness.amount);
        if (ambientSoundSource != null)
        {
            ambientSoundSource.volume = newOptions.ambientSoundVolume;
        }


        foreach (var complObj in complexityObjects)
        {
            if (complObj != null && complObj.GetComponent<AppearSoundCue>() == null)
            {
                complObj.AddComponent<AppearSoundCue>();
            }
        }
        foreach (var colorObj in colorfulObjects)
        {
            if (colorObj != null && colorObj.GetComponent<AppearSoundCue>() == null)
            {
                colorObj.AddComponent<AppearSoundCue>();
            }
        }
        foreach (var colorObj in specialColorfulObjects)
        {
            if (colorObj != null && colorObj.theObject != null && colorObj.theObject.GetComponent<AppearSoundCue>() == null)
            {
                colorObj.theObject.AddComponent<AppearSoundCue>();
            }
        }
    }

    public EnvironmentOptions get()
    {
        return options;
    }

    /// Triggers a relayout of the environment properties that have changed
    public void update(EnvironmentOptions newOptions)
    {
        //complexity
        if (newOptions.complexity.objects != options.complexity.objects)
        {
            updateComplexityObjects(newOptions.complexity.objects);
        }

        if (newOptions.complexity.tidyness != options.complexity.tidyness)
        {
            updateTidyness(newOptions.complexity.tidyness);
        }

        //general lighting
        if (newOptions.lighting.tod != options.lighting.tod)
        {
            updateTimeOfDay(newOptions.lighting.tod);
        }

        if (newOptions.lighting.colorTemp != options.lighting.colorTemp)
        {
            updateMainLightingColorTemp(newOptions.lighting.colorTemp);
        }

        if (newOptions.lighting.intensity != options.lighting.intensity)
        {
            updateMainLightingIntensity(newOptions.lighting.intensity);
        }

        //accent lights
        if (newOptions.lighting.accentLights.amount != options.lighting.accentLights.amount)
        {
            updateAccentLightsAmount(newOptions.lighting.accentLights.amount);
        }

        if (newOptions.lighting.accentLights.color != options.lighting.accentLights.color)
        {
            updateAccentLightsColor(newOptions.lighting.accentLights.color);
        }

        if (newOptions.lighting.accentLights.intensity != options.lighting.accentLights.intensity)
        {
            updateAccentLightsIntensity(newOptions.lighting.accentLights.intensity);
        }

        //Colorfullness
        if (newOptions.colorfullness.color != options.colorfullness.color)
        {
            updateColorfullnessColor(newOptions.colorfullness.color);
        }

        if (newOptions.colorfullness.amount != options.colorfullness.amount)
        {
            updateColorfullnessAmount(newOptions.colorfullness.amount);
        }

        if (newOptions.ambientSoundVolume != options.ambientSoundVolume)
        {
            ambientSoundSource.volume = newOptions.ambientSoundVolume;
        }

        options = newOptions;
    }

    private void updateComplexityObjects(float newComplexity)
    {
        activate(randomizedComplexityObjects, newComplexity);
    }

    private void updateTidyness(float newTidyness)
    {
        foreach (GameObject obj in randomizedComplexityObjects)
        {
            if (obj != null && obj.activeSelf)
            {
                var tidynessComp = obj.GetComponent<TidynessObject>();
                if (tidynessComp != null)
                {
                    tidynessComp.applyTidynessValue(newTidyness);
                }
                else
                {
                    var tidynessComps = obj.GetComponentsInChildren<TidynessObject>();
                    foreach (var tidynessChildComp in tidynessComps)
                    {
                        tidynessChildComp.applyTidynessValue(newTidyness);
                    }
                }
            }
        }
    }

    private void updateTimeOfDay(float newToD)
    {
        ToDLightHandler.instance.updateToD(newToD);
    }

    private void updateMainLightingColorTemp(float newColorTemp)
    {
        foreach(Light mainLight in mainLights)
        {
            mainLight.color = colorMapping.mainLightColorTempGradient.Evaluate(newColorTemp);
        }
    }

    private void updateMainLightingIntensity(float newIntensityNormalized)
    {
        foreach (Light mainLight in mainLights)
        {   
            mainLight.intensity = newIntensityNormalized * mainLightsMaxIntensity[mainLight];
        }
    }

    private void updateAccentLightsAmount(float newAmount)
    {
        activate(randomizedAccentLights.Select(l => l.gameObject).ToArray(), newAmount);
    }

    private void updateAccentLightsColor(AccentLightColor newColor)
    {
        {
            var mapping = colorMapping.accentLightColors.First(x => x.colorType == newColor);
            if (mapping != null)
            {
                foreach (AccentLight accentLight in accentLights)
                {
                    accentLight.changeColor(mapping);
                }
            }
        }

        foreach (var lightObj in accentLightObjects)
        {
            var mapping = lightObj.colorMappings.First(x => x.colorType == newColor);
            if (mapping != null && mapping.emissiveMaterial != null)
            {
                var renderer = lightObj.theObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = mapping.emissiveMaterial;
                }
            }
        }
    }

    private void updateAccentLightsIntensity(float newIntensity)
    {
        foreach(AccentLight accentLight in accentLights)
        {
            accentLight.changeIntensity(newIntensity);
        }
    }

    private void updateColorfullnessColor(AccentObjectColor newColor)
    {
        {
            var mapping = colorMapping.colorfolObjectsColors.First(x => x.colorType == newColor);
            if (mapping != null)
            {
                Gradient colorRange = mapping.colors;
                System.Random rnd = new System.Random();

                foreach (GameObject colorfulObj in colorfulObjects)
                {
                    var renderer = colorfulObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = colorRange.Evaluate(rnd.Next(0, 100) * 0.1f);
                    }
                }
            }
        }

        foreach(var colorfulObj in specialColorfulObjects)
        {
            var mapping = colorfulObj.colorMappings.First(x => x.colorType == newColor);
            if (mapping != null && mapping.material != null)
            {
                var renderer = colorfulObj.theObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = mapping.material;
                }
            }
        }
    }

    private void updateColorfullnessAmount(float newAmount)
    {
        activate(colorfulObjects, newAmount);
        activate(specialColorfulObjects.Select(s => s.theObject).ToArray(), newAmount);
    }

    private void activate(GameObject[] objects, float percentageActive)
    {
        if (objects.Length > 0)
        {
            //scale to number of objects
            int nrVisible = Mathf.RoundToInt(percentageActive * objects.Length);

            //set as many active as needed
            int idx = 0;
            while (idx < nrVisible)
            {
                if (objects[idx] != null)
                    objects[idx].SetActive(true);
                idx++;
            }
            while (idx < objects.Length)
            {
                if (objects[idx] != null)
                    objects[idx].SetActive(false);
                idx++;
            }
        }
    }


    public bool daylightShouldBeActivated()
    {
        return false; //studio is indoor location
    }

    public void refreshReflections()
    {
        foreach (var probe in reflectionProbes)
        {
            if (probe != null)
                probe.RenderProbe();
        }
    }
}
