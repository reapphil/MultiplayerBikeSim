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

public class BCG_EnterExitInitLoad : MonoBehaviour {

    [InitializeOnLoadMethod]
    static void InitOnLoad() {

        EditorApplication.delayCall += EditorUpdate;

    }

    public static void EditorUpdate() {

        bool hasKey = false;

#if BCG_ENTEREXIT
        hasKey = true;
#endif

        if (!hasKey) {

            Selection.activeObject = BCG_EnterExitSettings.Instance;
            BCG_SetScriptingSymbol.SetEnabled("BCG_ENTEREXIT", true);

        }

    }

}
