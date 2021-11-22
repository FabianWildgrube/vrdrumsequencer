using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvironmentScreenshotAutomator))]

public class EnvironmentScreenshotAutomatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnvironmentScreenshotAutomator automator = (EnvironmentScreenshotAutomator)target;
        if (GUILayout.Button("Export Screenshots"))
        {
            automator.recordAllScreenshots();
        }

        if (GUILayout.Button("Cancel"))
        {
            automator.cancelScreenshots();
        }
    }
}
