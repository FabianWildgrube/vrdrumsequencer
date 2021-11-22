using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class AvailableEnvironment
{
    public string name;
    public GameObject environmentPrefab;
    public Material environmentSky;
}

public struct EnvironmentCustomizationExport
{
    public string locationName;
    public System.DateTime exportDate;
    public EnvironmentOptions options;
}

public struct EnvironmentCustomizationImport
{
    public string participantId;
    public string locationName;
    public System.DateTime exportDate;
    public EnvironmentOptions options;
}

public class EnvironmentConfigManager : MonoBehaviour
{
    public static EnvironmentConfigManager instance;

    public GameObject configurationUI;

    public ReflectionProbe reflectionProbe;

    public AudioClip appearSound;
    [Range(0f, 1f)]
    public float appearSoundVolume;
    public bool appearSoundsEnabled = true;

    public AvailableEnvironment defaultEnvironment;

    [Header("Perfect Environment")]
    public string perfectEnvironmentName;
    public EnvironmentOptionsScriptableObject perfectEnvironmentConfig;

    [SerializeField]
    AvailableEnvironment[] availableEnvironments;

    [HideInInspector]
    public List<string> environmentNames; //meant for other components

    public System.Action OnEnvironmentChanged;

    private EnvironmentOptions currentOptions;
    private string currentSelectedEnvironmentName;
    private ICostumizableEnvironment currentSelectedEnvironment;

    private Dictionary<string, GameObject> instantiatedEnvironments;

    private List<GameObject> configSubUIs = new List<GameObject>();

    private bool initialExportHappened = false;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            environmentNames = new List<string>();
            foreach (AvailableEnvironment env in availableEnvironments)
            {
                environmentNames.Add(env.name);
            }

            currentSelectedEnvironment = null;
            instantiatedEnvironments = new Dictionary<string, GameObject>();

            currentOptions = EnvironmentOptions.getDefaults();
        } 
    }

    private void Start()
    {
        if (defaultEnvironment != null)
        {
            loadDefaultEnvironment();
        }

        ExperimentManager.instance.OnStageChanged += handleStageChange;
    }

    private void OnDestroy()
    {
        ExperimentManager.instance.OnStageChanged -= handleStageChange;
    }

    public void registerConfigSubUI(GameObject obj)
    {
        configSubUIs.Add(obj);
        obj.SendMessage("updateUIElements", currentOptions);
    }

    /// Activates the environment associated with that name.
    /// If the environment was selected (and modified) before, it will be reloaded with its options.
    /// If the environment was not selected before it will be reloaded and the current options are applied to it.
    public void selectEnvironment(string newEnvironmentName, EnvironmentOptionsScriptableObject overrideOptionsObject = null)
    {
        AvailableEnvironment newEnv = newEnvironmentName == defaultEnvironment.name ? defaultEnvironment : System.Array.Find(availableEnvironments, x => x.name == newEnvironmentName);
        if (newEnv != null)
        {
            appearSoundsEnabled = false; //don't have appear sounds when switching the environment

            if (currentSelectedEnvironment != null) ((MonoBehaviour)currentSelectedEnvironment).gameObject.SetActive(false);

            //reuse environment from cache or instantiate anew
            if (instantiatedEnvironments.ContainsKey(newEnv.name))
            {
                GameObject newEnvGO = instantiatedEnvironments[newEnv.name];
                newEnvGO.SetActive(true);
                currentSelectedEnvironment = newEnvGO.GetComponent<ICostumizableEnvironment>();

                //(re-)activate sunlight if necessary, must happen before initialising the new environment otherwise this would override the tod set upon initialisation
                ToDLightHandler.instance.DaylightEnabled = currentSelectedEnvironment.daylightShouldBeActivated();

                if (overrideOptionsObject != null)
                {
                    currentOptions = overrideOptionsObject.convert();
                    currentSelectedEnvironment.set(currentOptions);
                } else
                {
                    //reload config of that environment
                    currentOptions = currentSelectedEnvironment.get();
                }
            }
            else
            {
                GameObject newEnvGO = Instantiate(newEnv.environmentPrefab);
                instantiatedEnvironments.Add(newEnv.name, newEnvGO);
                currentSelectedEnvironment = newEnvGO.GetComponent<ICostumizableEnvironment>();

                //(re-)activate sunlight if necessary, must happen before initialising the new environment otherwise this would override the tod set upon initialisation
                ToDLightHandler.instance.DaylightEnabled = currentSelectedEnvironment.daylightShouldBeActivated();

                if (overrideOptionsObject != null) currentOptions = overrideOptionsObject.convert();
                currentSelectedEnvironment.set(currentOptions);
            }

            currentSelectedEnvironment.activate();

            //update the ui accordingly
            foreach (GameObject subUI in configSubUIs)
            {
                if (ExperimentManager.instance.currentExperimentStage == ExperimentStage.ENVIRONMENT_CONFIGURATION)
                {
                    //activate shortly so it can update objects and then let the tab view handle correct activation itself again
                    subUI.SetActive(true);
                    subUI.SendMessage("updateUIElements", currentOptions);
                    subUI.SetActive(false);
                }
            }

            if (newEnv.environmentSky != null)
            {
                Debug.Log("New Skybox: " + newEnv.name + ", " + newEnv.environmentSky);
                RenderSettings.skybox = newEnv.environmentSky;
                recomputeReflections();
            }

            currentSelectedEnvironmentName = newEnvironmentName;
            if (OnEnvironmentChanged != null) OnEnvironmentChanged();

            if (currentSelectedEnvironmentName != defaultEnvironment.name && ExperimentManager.instance.currentCondition == ExperimentCondition.CUSTOM_ENVIRONMENT)
            {
                //this takes care of saving the config, when the user changes location after their initial configuration
                //but not saving it when the location is changed due to study flow (going to questionnaire or to perfect environment during fixed condition)
                updateConfigurationExportIfNecessary();
            }
            StartCoroutine(allowAppearSoundsDeferred(0.1f));
        } else
        {
            Debug.LogWarning($"Trying to select unknown environment '{newEnvironmentName}'!");
        }
    }

    private IEnumerator allowAppearSoundsDeferred(float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);
        appearSoundsEnabled = true;
    }

    public void executeDeferred(float delay, System.Action callback)
    {
        StartCoroutine(deferredExecutionCoroutine(delay, callback));
    }

    private IEnumerator deferredExecutionCoroutine(float delay, System.Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback();
    }

    private void handleStageChange(ExperimentStage stage)
    {
        //switch to perfect environment if we should but we're not there yet
        if (ExperimentManager.instance.perfectEnvironmentShouldBeUsed)
        {
            if (currentSelectedEnvironmentName != perfectEnvironmentName)
            {
                loadPerfectEnvironment();
            }
        }

        if (ExperimentManager.instance.defaultEnvironmentShouldBeUsed)
        {
            if (currentSelectedEnvironmentName != defaultEnvironment.name)
            {
                loadDefaultEnvironment();
            }
        }
    }

    private void loadPerfectEnvironment()
    {
        selectEnvironment(perfectEnvironmentName, perfectEnvironmentConfig);
    }

    private void loadDefaultEnvironment()
    {
        selectEnvironment(defaultEnvironment.name);
    }

    public void recomputeReflections()
    {
        reflectionProbe.RenderProbe();
        currentSelectedEnvironment.refreshReflections();
    }

    public void finishConfiguring()
    {
        exportCurrentConfiguration();
        configurationUI.SetActive(false);
    }

    private void exportCurrentConfiguration()
    {
        if (currentSelectedEnvironment != null)
        {
            EnvironmentCustomizationExport exportData = new EnvironmentCustomizationExport();
            exportData.locationName = currentSelectedEnvironmentName;
            exportData.exportDate = System.DateTime.Now;
            exportData.options = currentOptions;

            string configExportString = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            if (!FileIOUtils.DuplicateSafeWriteToFile(ExperimentManager.instance.currentConditionUserDataDirPath, exportData.locationName, "json", configExportString))
            {
                throw new System.Exception($"Could not export environment to {exportData.locationName}.json!");
            }
            Debug.Log($"Exported Environment {exportData.locationName}");
            initialExportHappened = true;
        }
    }

    private void notifySelectedEnvironment()
    {
        if (currentSelectedEnvironment != null) currentSelectedEnvironment.update(currentOptions);
    }

    // Make sure to save all changes after the initial configuring
    // i.e. the user changes the configuration while working in the custom environment -> those changes need to be analysed
    private void updateConfigurationExportIfNecessary()
    {
        if (initialExportHappened)
        {
            exportCurrentConfiguration();
        }
    }

    public void updateComplexityObjects(float newComplexityObjects)
    {
        currentOptions.complexity.objects = newComplexityObjects;
        notifySelectedEnvironment();
    }

    public void lastUpdateComplexityObjects()
    {
        updateConfigurationExportIfNecessary();
    }

    public void updateComplexityTidyness(float newTidyness)
    {
        currentOptions.complexity.tidyness = newTidyness;
        notifySelectedEnvironment();
    }

    public void lastUpdateComplexityTidynesss()
    {
        updateConfigurationExportIfNecessary();
    }

    public void updateTimeOfDay(float newToD)
    {
        currentOptions.lighting.tod = newToD;
        notifySelectedEnvironment();
    }

    public void lastUpdateTimeOfDay()
    {
        recomputeReflections();
        updateConfigurationExportIfNecessary();
    }

    public void updateMainColorTemp(float newColorTemp)
    {
        currentOptions.lighting.colorTemp = newColorTemp;
        notifySelectedEnvironment();
    }

    public void lastUpdateMainColorTemp()
    {
        recomputeReflections();
        updateConfigurationExportIfNecessary();
    }

    public void updateMainLightingIntensity(float newIntensity)
    {
        currentOptions.lighting.intensity = newIntensity;
        notifySelectedEnvironment();
    }

    public void lastUpdateMainLightingIntensity()
    {
        recomputeReflections();
        updateConfigurationExportIfNecessary();
    }

    public void updateAccentLightsAmount(float newAmount)
    {
        currentOptions.lighting.accentLights.amount = newAmount;
        notifySelectedEnvironment();
    }

    public void lastUpdateAccentLightsAmount()
    {
        recomputeReflections();
        updateConfigurationExportIfNecessary();
    }

    public void updateAccentLightsColor(AccentLightColor newColor)
    {
        currentOptions.lighting.accentLights.color = newColor;
        notifySelectedEnvironment();
        recomputeReflections();
        updateConfigurationExportIfNecessary();
    }

    public void updateAccentLightsIntensity(float newIntensity)
    {
        currentOptions.lighting.accentLights.intensity = newIntensity;
        notifySelectedEnvironment();
    }

    public void lastUpdateAccentLightsIntensity()
    {
        recomputeReflections();
        updateConfigurationExportIfNecessary();
    }

    public void updateColorfullnessColor(AccentObjectColor newColor)
    {
        currentOptions.colorfullness.color = newColor;
        notifySelectedEnvironment();
        updateConfigurationExportIfNecessary();
    }

    public void updateColorfullnessAmount(float newAmount)
    {
        currentOptions.colorfullness.amount = newAmount;
        notifySelectedEnvironment();
    }

    public void lastUpdateColorfullnessAmount()
    {
        updateConfigurationExportIfNecessary();
    }

    public void updateAmbientSoundVolume(float newVolume)
    {
        currentOptions.ambientSoundVolume = newVolume;
        notifySelectedEnvironment();
    }

    public void lastUpdateAmbientSoundVolume()
    {
        updateConfigurationExportIfNecessary();
    }
}
