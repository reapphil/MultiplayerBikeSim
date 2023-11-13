//----------------------------------------------
//            BCG Shared Assets
//
// Copyright © 2014 - 2021 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(BCG_EnterExitSettings))]
public class BCG_EnterExitSettingsEditor : Editor {

    BCG_EnterExitSettings prop;

    public bool EnablePhotonEnterExit {

        get {

            bool _bool = BCG_EnterExitSettings.Instance.enableEnterExit_Photon;
            return _bool;

        }

        set {

            bool _bool = BCG_EnterExitSettings.Instance.enableEnterExit_Photon;

            if (_bool == value)
                return;

            BCG_EnterExitSettings.Instance.enableEnterExit_Photon = value;
            BCG_SetScriptingSymbol.SetEnabled("BCG_ENTEREXITPHOTON", value);

        }

    }

    public override void OnInspectorGUI() {

        prop = (BCG_EnterExitSettings)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("keepEnginesAlive"), new GUIContent("Keep Engines Alive"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterExitSpeedLimit"), new GUIContent("Enter Exit Speed Limit"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mobileController"), new GUIContent("Mobile Controller"));

        EnablePhotonEnterExit = EditorGUILayout.ToggleLeft(new GUIContent("Enable Photon Enter Exit", "It will enable Photon Enter Exit support for all BCG vehicles."), EnablePhotonEnterExit);

        EditorGUILayout.LabelField("BCG Enter Exit  " + BCG_Version.version + " \nBoneCracker Games", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(50f));

        EditorGUILayout.LabelField("Created by Buğra Özdoğanlar", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(50f));

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        serializedObject.ApplyModifiedProperties();

    }

}
