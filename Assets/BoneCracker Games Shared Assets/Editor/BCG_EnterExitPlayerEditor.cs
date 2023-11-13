//----------------------------------------------
//            BCG Shared Assets
//
// Copyright © 2014 - 2021 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------


using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(BCG_EnterExitPlayer))]
public class BCG_EnterExitPlayerEditor : Editor {

    BCG_EnterExitPlayer prop;

    public override void OnInspectorGUI() {

        prop = (BCG_EnterExitPlayer)target;
        serializedObject.Update();

        EditorGUILayout.HelpBox("Script must be attached to root of your character player.", MessageType.Info);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("isTPSController"), new GUIContent("Is TPS Controller?"), false);

        if (prop.isTPSController) {

            EditorGUILayout.PropertyField(serializedObject.FindProperty("rayHeight"), new GUIContent("Ray Height"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterCamera"), new GUIContent("TPS Camera"), false);

        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerStartsAsInVehicle"), new GUIContent("Player Starts As In Vehicle"), false);

        if (prop.playerStartsAsInVehicle) {

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inVehicle"), new GUIContent("Vehicle"), false);
            EditorGUI.indentLevel--;

        }

        if (!FindObjectOfType<BCG_EnterExitManager>()) {

            EditorGUILayout.HelpBox("Your scene doesn't have BCG_EnterExitManager. In order to use enter-exit system, your scene must have _BCGEnterExitManager.", MessageType.Error);

            if (GUILayout.Button("Create BCG_EnterExitManager")) {

                GameObject newBCG_EnterExitManager = new GameObject();
                newBCG_EnterExitManager.transform.name = "_BCGEnterExitManager";
                newBCG_EnterExitManager.transform.position = Vector3.zero;
                newBCG_EnterExitManager.transform.rotation = Quaternion.identity;
                newBCG_EnterExitManager.AddComponent<BCG_EnterExitManager>();

            }

        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

    }

}
