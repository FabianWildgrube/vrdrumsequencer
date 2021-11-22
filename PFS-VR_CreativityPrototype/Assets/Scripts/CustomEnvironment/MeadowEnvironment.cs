using System.Collections.Generic;
using UnityEngine;
using System.Linq;

struct ParticleSystemInfo
{
    public int maxParticles;
    public float rateOverTime;
    public float startSpeed;
}

public class MeadowEnvironment : MonoBehaviour, ICostumizableEnvironment
{
    public GameObject mainModel;

    [Header("Visual Complexity")]
    public GameObject[] complexityObjects;
    public ParticleSystem[] particleSystems;

    [Header("Lights")]
    public Light[] mainLights;
    public AccentLight[] accentLights;

    [Header("Colorfulness")]
    public GameObject[] redOrangeObjectParents;
    public GameObject[] greenBlueObjectParents;
    private List<GameObject> currentColorObjects = new List<GameObject>();

    [Header("Sound")]
    public AudioClip ambientSoundDay;
    public AudioSource ambientSoundSourceDay;
    public AnimationCurve daySoundVolumeCurve;
    public AudioClip ambientSoundNight;
    public AudioSource ambientSoundSourceNight;
    public AnimationCurve nightSoundVolumeCurve;

    [Header("ColorMappings")]
    public EnvironmentColorMapping colorMapping;

    private EnvironmentOptions options;

    private Dictionary<Light, float> mainLightsMaxIntensity;

    private Dictionary<ParticleSystem, ParticleSystemInfo> particleSystemsMaxInfos;

    private AccentLight[] randomizedAccentLights;


    private void Awake()
    {
        mainLightsMaxIntensity = new Dictionary<Light, float>();
        foreach (Light mainLight in mainLights)
        {
            mainLightsMaxIntensity.Add(mainLight, mainLight.intensity);
        }

        System.Random rnd = new System.Random(15);
        randomizedAccentLights = accentLights.OrderBy(x => rnd.Next()).ToArray();

        particleSystemsMaxInfos = new Dictionary<ParticleSystem, ParticleSystemInfo>();
        foreach (var particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            var emmission = particleSystem.emission;
            ParticleSystemInfo maxValues = new ParticleSystemInfo();
            maxValues.rateOverTime = emmission.rateOverTime.constant;
            maxValues.maxParticles = main.maxParticles;
            maxValues.startSpeed = main.startSpeed.constant;

            particleSystemsMaxInfos.Add(particleSystem, maxValues);
        }
    }

    public void activate()
    {
        ambientSoundSourceDay.clip = ambientSoundDay;
        ambientSoundSourceDay.Play();
        ambientSoundSourceNight.clip = ambientSoundNight;
        ambientSoundSourceNight.Play();
        updateTimeOfDay(options.lighting.tod); //reapply tod to ensure outdoor environment behaves correctly after indoor environment disabled the day/night cycle
    }

    public EnvironmentOptions get()
    {
       return options;
    }

    public void set(EnvironmentOptions newOptions)
    {
        options = newOptions;

        updateComplexityObjects(newOptions.complexity.objects);
        updateTidyness(newOptions.complexity.tidyness);
        updateAmbientSound(newOptions.ambientSoundVolume, newOptions.lighting.tod); //update ambient sound before tod because tod changes ambient sound volume
        updateTimeOfDay(newOptions.lighting.tod);
        updateMainLightingColorTemp(newOptions.lighting.colorTemp);
        updateMainLightingIntensity(newOptions.lighting.intensity);
        updateAccentLightsAmount(newOptions.lighting.accentLights.amount);
        updateAccentLightsColor(newOptions.lighting.accentLights.color);
        updateAccentLightsIntensity(newOptions.lighting.accentLights.intensity);
        updateColorfullnessColor(newOptions.colorfullness.color);
        updateColorfullnessAmount(newOptions.colorfullness.amount);        

        foreach (var complObj in complexityObjects)
        {
            if (complObj.GetComponent<AppearSoundCue>() == null)
            {
                complObj.AddComponent<AppearSoundCue>();
            }
        }
        foreach (var blueParent in greenBlueObjectParents)
        {
            foreach (Transform child in blueParent.transform)
            {
                if (child.gameObject.GetComponent<AppearSoundCue>() == null)
                {
                    child.gameObject.AddComponent<AppearSoundCue>();
                }
            }
        }
        foreach (var redParent in redOrangeObjectParents)
        {
            foreach (Transform child in redParent.transform)
            {
                if (child.gameObject.GetComponent<AppearSoundCue>() == null)
                {
                    child.gameObject.AddComponent<AppearSoundCue>();
                }
            }
        }
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
            updateAmbientSoundVolume(newOptions.ambientSoundVolume);
        }

        options = newOptions;
    }

    private void updateComplexityObjects(float newComplexity)
    {
        activateObjects(complexityObjects, newComplexity);
        activateObjects(particleSystems.Select(ps => ps.gameObject).ToArray(), newComplexity);
        foreach (var particleSystem in particleSystems)
        {
            if (particleSystem.gameObject.activeSelf)
            {

                if (!particleSystem.isPlaying) particleSystem.Play();
            }
        }
    }

    private void updateTidyness(float newTidyness)
    {
        foreach (GameObject obj in complexityObjects)
        {
            if (obj.activeSelf)
            {
                var tidynessComp = obj.GetComponent<TidynessObject>();
                if (tidynessComp != null)
                {
                    tidynessComp.applyTidynessValue(newTidyness);
                } else
                {
                    var tidynessComps = obj.GetComponentsInChildren<TidynessObject>();
                    foreach (var tidynessChildComp in tidynessComps)
                    {
                        tidynessChildComp.applyTidynessValue(newTidyness);
                    }
                }
            }
        }
        

        foreach (var particleSystem in particleSystems)
        {
            if (particleSystem.gameObject.activeSelf && particleSystem.isPlaying)
            {
                Debug.Log("Stop ps " + particleSystem.gameObject.name);
                particleSystem.Stop();
            }
           
                var main = particleSystem.main;
                var emmission = particleSystem.emission;
                var maxInfos = particleSystemsMaxInfos[particleSystem];
                float particleTidyness = 1f - newTidyness; // 0 tidyness => maximum chaos
                main.maxParticles = Mathf.RoundToInt(Mathf.Max(maxInfos.maxParticles * particleTidyness, 0.1f));
                main.startSpeed = Mathf.Max(maxInfos.startSpeed * particleTidyness, 0.1f);
                emmission.rateOverTime = Mathf.Max(maxInfos.rateOverTime * particleTidyness, 0.1f);

            if (particleSystem.gameObject.activeSelf)
            {
                Debug.Log("Restart ps " + particleSystem.gameObject.name);
                particleSystem.Play();
            }
        }
    }

    private void updateTimeOfDay(float newToD)
    {
        ToDLightHandler.instance.updateToD(newToD);
        updateAmbientSoundTimeOfDay(newToD);
    }

    private void updateMainLightingColorTemp(float newColorTemp)
    {
        foreach (Light mainLight in mainLights)
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
        activateObjects(randomizedAccentLights.Select(l => l.gameObject).ToArray(), newAmount);
    }

    private void updateAccentLightsColor(AccentLightColor newColor)
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

    private void updateAccentLightsIntensity(float newIntensity)
    {
        foreach (AccentLight accentLight in accentLights)
        {
            accentLight.changeIntensity(newIntensity);
        }
    }

    private void updateColorfullnessColor(AccentObjectColor newColor)
    {
        currentColorObjects.Clear();

        EnvironmentConfigManager.instance.appearSoundsEnabled = false;

        if (newColor == AccentObjectColor.BLAUGRÜN)
        {
            //show bluegreen objects
            foreach (var blueParent in greenBlueObjectParents)
            {
                blueParent.SetActive(true);
                foreach(Transform child in blueParent.transform)
                {
                    currentColorObjects.Add(child.gameObject);
                }
            }

            foreach (var redParent in redOrangeObjectParents)
            {
                redParent.SetActive(false);
            }
        } else if (newColor == AccentObjectColor.ROTORANGE)
        {
            //show redorange
            foreach (var redParent in redOrangeObjectParents)
            {
                redParent.SetActive(true);
                foreach (Transform child in redParent.transform)
                {
                    currentColorObjects.Add(child.gameObject);
                }
            }

            foreach (var blueParent in greenBlueObjectParents)
            {
                blueParent.SetActive(false);
            }
        }

        activateObjects(currentColorObjects.ToArray(), options.colorfullness.amount);

        EnvironmentConfigManager.instance.appearSoundsEnabled = true;
    }

    private void updateColorfullnessAmount(float newAmount)
    {
        activateObjects(currentColorObjects.ToArray(), newAmount);
    }

    private void updateAmbientSoundVolume(float newVolume)
    {
        updateAmbientSound(newVolume, options.lighting.tod);
    }

    private void updateAmbientSoundTimeOfDay(float tod)
    {
        updateAmbientSound(options.ambientSoundVolume, tod);
    }

    private void updateAmbientSound(float newVolume, float tod)
    {
        float dayTimeVolumeMultiplier = daySoundVolumeCurve.Evaluate(tod);
        float nightTimeVolumeMultiplier = nightSoundVolumeCurve.Evaluate(tod);

        ambientSoundSourceDay.volume = newVolume * dayTimeVolumeMultiplier;
        ambientSoundSourceNight.volume = newVolume * nightTimeVolumeMultiplier;
    }

    private void activateObjects(GameObject[] objects, float percentageActive)
    {
        if (objects.Length > 0)
        {
            //scale to number of objects
            int nrVisible = Mathf.RoundToInt(percentageActive * objects.Length);

            //set as many active as needed
            int idx = 0;
            while (idx < nrVisible)
            {
                objects[idx].SetActive(true);
                idx++;
            }
            while (idx < objects.Length)
            {
                objects[idx].SetActive(false);
                idx++;
            }
        }
    }

    public bool daylightShouldBeActivated()
    {
        return true;
    }

    public void refreshReflections()
    {
        
    }
}
