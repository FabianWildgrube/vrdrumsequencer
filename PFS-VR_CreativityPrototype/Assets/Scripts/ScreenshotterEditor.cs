using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Screenshotter))]
public class ScreenshotterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Capture Tools", EditorStyles.boldLabel);

        Screenshotter screenshotter = (Screenshotter)target;
        if (GUILayout.Button("Single Screenshot"))
        {
            screenshotter.Capture();
        }
        if (GUILayout.Button("360 Cube Image"))
        {
            screenshotter.Capture360CubeImage();
        }
        if (GUILayout.Button("Panorama Series (multiple images)"))
        {
            screenshotter.CapturePanorama();
        }
    }
}
