//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class RCC_PhotonInitLoad : MonoBehaviour {

    [InitializeOnLoadMethod]
    static void InitOnLoad() {

        EditorApplication.delayCall += EditorUpdate;

    }

    public static void EditorUpdate() {

        bool hasKey = false;

#if RCC_PHOTON
        hasKey = true;
#endif

        if (!hasKey) {

            EditorUtility.DisplayDialog("Photon PUN 2 For Realistic Car Controller", "Be sure you have imported latest Photon PUN 2 to your project. Pass in your AppID to Photon, and run the RCC City Photon 2 demo scene. You can find more detailed info in documentation.", "Close");

        }

        RCC_SetScriptingSymbol.SetEnabled("RCC_PHOTON", true);

    }

}
