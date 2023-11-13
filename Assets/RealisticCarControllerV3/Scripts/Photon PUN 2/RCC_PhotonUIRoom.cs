//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

#if RCC_PHOTON && PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RCC_PhotonUIRoom : MonoBehaviour {

    internal string roomNameString;
    public Text roomName;
    public Text maxPlayers;

    public void Check(string _roomName, string _maxPlayers) {

        roomNameString = _roomName;
        roomName.text = _roomName;
        maxPlayers.text = _maxPlayers;

    }

    public void JoinRoom() {

        RCC_PhotonManager.Instance.JoinSelectedRoom(this);

    }

}
#endif