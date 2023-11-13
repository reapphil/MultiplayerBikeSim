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
using UnityEngine.UI;

public class RCC_PhotonUIChatLine : MonoBehaviour {

    public Text text;

    private void Awake() {

        text = GetComponent<Text>();

    }

    public void Line(string chatText) {

        text.text = chatText;

    }

}
