//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCC_PhotonDemoVehicles : ScriptableObject {

    public RCC_CarControllerV3[] vehicles;

    #region singleton
    private static RCC_PhotonDemoVehicles instance;
    public static RCC_PhotonDemoVehicles Instance { get { if (instance == null) instance = Resources.Load("Photon Vehicles/RCC_PhotonDemoVehicles") as RCC_PhotonDemoVehicles; return instance; } }
    #endregion

}
