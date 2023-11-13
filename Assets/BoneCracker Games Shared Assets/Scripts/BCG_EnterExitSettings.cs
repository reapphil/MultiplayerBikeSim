//----------------------------------------------
//            BCG Shared Assets
//
// Copyright © 2014 - 2021 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Stored all general shared Enter-Exit settings here.
/// </summary>
[System.Serializable]
public class BCG_EnterExitSettings : ScriptableObject {

    #region singleton
    public static BCG_EnterExitSettings instance;
    public static BCG_EnterExitSettings Instance { get { if (instance == null) instance = Resources.Load("BCG_EnterExitSettings") as BCG_EnterExitSettings; return instance; } }
    #endregion

    public bool enableEnterExit_Photon = false;
    public bool keepEnginesAlive = true;
    public float enterExitSpeedLimit = 20f;
    public bool mobileController = false;

}
