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

public class BCG_MobileCharacterController : MonoBehaviour {

    public static Vector2 mouse;
    public static Vector2 move;

    public BCG_Joystick mouseJoystick;
    public BCG_Joystick moveJoystick;

    void Update() {

        mouse = mouseJoystick.inputVector;
        move = moveJoystick.inputVector;

    }

}
