using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EnvScreenshotCameraPosition
{
    public string name;
    public Transform cameraTrans;
}

public class EnvironmentScreenshotAutomator : MonoBehaviour
{
    public string environmentConfigJsonFilesDirectory;
    public Screenshotter screenShotCamera;
    public EnvScreenshotCameraPosition[] cameraPositions;
    public int width = 5000;
    public int height = 3000;
    public string outputDir;

    public void recordAllScreenshots()
    {
        StartCoroutine(takeAllScreenshots());
    }

    public void cancelScreenshots()
    {
        StopAllCoroutines();
    }

    private IEnumerator takeAllScreenshots()
    {
        screenShotCamera.width = width;
        screenShotCamera.height = height;
        screenShotCamera.outputDirectory = outputDir;

        string[] files = Directory.GetFiles(environmentConfigJsonFilesDirectory, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string jsonFilePath in files)
        {
            Debug.Log("Exporting screenshot for: " + jsonFilePath);
            var envConfigWrapper = FileIOUtils.createFromJsonFile<EnvironmentCustomizationImport>(jsonFilePath);
            var locationName = envConfigWrapper.locationName;
            var envOptions = envConfigWrapper.options;

            var envOptionsScriptable = ScriptableObject.CreateInstance<EnvironmentOptionsScriptableObject>();
            envOptionsScriptable.complexity = envOptions.complexity;
            envOptionsScriptable.lighting = envOptions.lighting;
            envOptionsScriptable.colorfullness = envOptions.colorfullness;
            envOptionsScriptable.ambientSoundVolume = envOptions.ambientSoundVolume;

            EnvironmentConfigManager.instance.selectEnvironment(locationName, envOptionsScriptable);

            yield return new WaitForSeconds(0.5f);

            foreach (var postionSet in cameraPositions)
            {
                if (postionSet.name == envConfigWrapper.locationName)
                {
                    screenShotCamera.setPosition(postionSet.cameraTrans.position, postionSet.cameraTrans.rotation);
                    break;
                }
            }

            screenShotCamera.Capture(envConfigWrapper.participantId + "_" + envConfigWrapper.locationName + "_configured_screenshot");

            yield return new WaitForSeconds(0.5f);
        }

    }
}
