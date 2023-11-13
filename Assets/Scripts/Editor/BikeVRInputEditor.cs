using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BikeVRInput))]
public class BikeVRInputEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        //button
        if (GUILayout.Button("Recenter Steering"))
        {
            Debug.Log("Steering recentered");
            ((BikeVRInput)target).Calibrate();
        }
        DrawDefaultInspector();
    }
}