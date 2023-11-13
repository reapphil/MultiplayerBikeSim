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
/// Enter Exit for FPS Player.
/// </summary>
[AddComponentMenu("BoneCracker Games/Shared Assets/Enter-Exit/BCG Enter Exit Script For Player Characters")]
public class BCG_EnterExitPlayer : MonoBehaviour {

    public bool isTPSController = false;
    public float rayHeight = 1f;

    public bool canControl = true;
    public bool showGui = false;
    public BCG_EnterExitVehicle targetVehicle;

    public bool playerStartsAsInVehicle = false;
    public BCG_EnterExitVehicle inVehicle;

    public Camera characterCamera;

    public delegate void onBCGPlayerSpawned(BCG_EnterExitPlayer player);
    public static event onBCGPlayerSpawned OnBCGPlayerSpawned;

    public delegate void onBCGPlayerDestroyed(BCG_EnterExitPlayer player);
    public static event onBCGPlayerDestroyed OnBCGPlayerDestroyed;

    public delegate void onBCGPlayerEnteredAVehicle(BCG_EnterExitPlayer player, BCG_EnterExitVehicle vehicle);
    public static event onBCGPlayerEnteredAVehicle OnBCGPlayerEnteredAVehicle;

    public delegate void onBCGPlayerExitedFromAVehicle(BCG_EnterExitPlayer player, BCG_EnterExitVehicle vehicle);
    public static event onBCGPlayerExitedFromAVehicle OnBCGPlayerExitedFromAVehicle;

    void Awake() {

        if (!playerStartsAsInVehicle)
            inVehicle = null;

        if (!isTPSController)
            characterCamera = GetComponentInChildren<Camera>();

    }

    void OnEnable() {

        if (OnBCGPlayerSpawned != null)
            OnBCGPlayerSpawned(this);

        if (playerStartsAsInVehicle)
            StartCoroutine(StartInVehicle());

    }

    IEnumerator StartInVehicle() {

        yield return new WaitForFixedUpdate();

        GetIn(inVehicle);

    }

    public void GetIn(BCG_EnterExitVehicle vehicle) {

        if (OnBCGPlayerEnteredAVehicle != null)
            OnBCGPlayerEnteredAVehicle(this, vehicle);

    }

    public void GetOut() {

        if (inVehicle == null)
            return;

        if (inVehicle.speed > BCG_EnterExitSettings.Instance.enterExitSpeedLimit)
            return;

        if (OnBCGPlayerExitedFromAVehicle != null)
            OnBCGPlayerExitedFromAVehicle(this, inVehicle);

    }

    private void Update() {

        if (!canControl)
            return;

        Vector3 rayPosition;
        Quaternion rayRotation = new Quaternion();

        if (characterCamera && !isTPSController) {

            rayPosition = characterCamera.transform.position;
            rayRotation = characterCamera.transform.rotation;

        } else {

            rayPosition = transform.position + (Vector3.up * rayHeight);
            rayRotation = transform.rotation;

        }

        Vector3 rayDirection = rayRotation * Vector3.forward;
        RaycastHit hit;

        Debug.DrawRay(rayPosition, rayDirection * 1.5f, Color.blue);

        if (Physics.Raycast(rayPosition, rayDirection, out hit, 1.5f)) {

            if (!targetVehicle) {

                targetVehicle = hit.collider.transform.GetComponentInParent<BCG_EnterExitVehicle>();

            } else {

                showGui = true;

                //if (Input.GetKeyDown(BCG_EnterExitSettings.Instance.enterExitVehicleKB))
                //    GetIn(targetVehicle);

            }

        } else {

            showGui = false;
            targetVehicle = null;

        }

    }

    private void OnGUI() {

        if (showGui) {

            GUI.skin.label.fontSize = 36;
            GUI.Label(new Rect((Screen.width / 2f) - 300f, (Screen.height / 2f) - 25f, 600f, 50f), "Press Interaction Button To Get In");

        }

    }

    void OnDestroy() {

        if (OnBCGPlayerDestroyed != null)
            OnBCGPlayerDestroyed(this);

    }


}
