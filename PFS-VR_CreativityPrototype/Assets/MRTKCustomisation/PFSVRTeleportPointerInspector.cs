// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.﻿

using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Teleport.Editor
{
    [CustomEditor(typeof(PFSVRTeleportPointer))]
    public class PFSVRTeleportPointerInspector : LinePointerInspector
    {
        private SerializedProperty hotSpotCursorVisibility;
        private SerializedProperty teleportAction;
        private SerializedProperty inputThreshold;
        private SerializedProperty angleOffset;
        private SerializedProperty teleportActivationAngle;
        private SerializedProperty upDirectionThreshold;
        private SerializedProperty lineColorHotSpot;
        private SerializedProperty validLayers;
        private SerializedProperty invalidLayers;
        private SerializedProperty pointerAudioSource;
        private SerializedProperty teleportRequestedClip;
        private SerializedProperty teleportCompletedClip;

        private bool teleportPointerFoldout = true;

        protected override void OnEnable()
        {
            DrawBasePointerActions = false;
            base.OnEnable();

            hotSpotCursorVisibility = serializedObject.FindProperty("hotSpotCursorVisibility");
            teleportAction = serializedObject.FindProperty("teleportAction");
            inputThreshold = serializedObject.FindProperty("inputThreshold");
            angleOffset = serializedObject.FindProperty("angleOffset");
            teleportActivationAngle = serializedObject.FindProperty("teleportActivationAngle");
            upDirectionThreshold = serializedObject.FindProperty("upDirectionThreshold");
            lineColorHotSpot = serializedObject.FindProperty("LineColorHotSpot");
            validLayers = serializedObject.FindProperty("ValidLayers");
            invalidLayers = serializedObject.FindProperty("InvalidLayers");
            pointerAudioSource = serializedObject.FindProperty("pointerAudioSource");
            teleportRequestedClip = serializedObject.FindProperty("teleportRequestedClip");
            teleportCompletedClip = serializedObject.FindProperty("teleportCompletedClip");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            teleportPointerFoldout = EditorGUILayout.Foldout(teleportPointerFoldout, "Teleport Pointer Settings", true);

            if (teleportPointerFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(hotSpotCursorVisibility);
                EditorGUILayout.PropertyField(teleportAction);
                EditorGUILayout.PropertyField(inputThreshold);
                EditorGUILayout.PropertyField(angleOffset);
                EditorGUILayout.PropertyField(teleportActivationAngle);
                EditorGUILayout.PropertyField(upDirectionThreshold);
                EditorGUILayout.PropertyField(lineColorHotSpot);
                EditorGUILayout.PropertyField(validLayers);
                EditorGUILayout.PropertyField(invalidLayers);
                EditorGUILayout.PropertyField(pointerAudioSource);
                EditorGUILayout.PropertyField(teleportRequestedClip);
                EditorGUILayout.PropertyField(teleportCompletedClip);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}