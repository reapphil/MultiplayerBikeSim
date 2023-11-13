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

public class BCG_TPSController : MonoBehaviour {

    #region Controller

    public float speed = 4f;

    private float inputMovementX = 0f;
    private float inputMovementY = 0f;

    #endregion

    #region Camera
    public Camera characterCamera;

    // Calculate the current rotation angles for TPS mode.
    private Quaternion wantedRotation = Quaternion.identity;

    // Target position.
    private Vector3 targetPosition = Vector3.zero;

    // Orbit X and Y inputs.
    private float orbitX = 0f;
    private float orbitY = 0f;

    // Minimum and maximum Orbit X, Y degrees.
    public float minOrbitY = -20f;
    public float maxOrbitY = 80f;

    public float distance = 5f;
    public float height = 1.5f;

    public float sensitivity = 1f;

    #endregion

    public BCG_Inputs inputs = new BCG_Inputs();

    void Update() {

        Inputs();
        Camera();

    }

    void FixedUpdate() {

        Controller();

    }

    private void Inputs() {

        inputs = BCG_InputManager.Instance.inputs;

        if (!BCG_EnterExitSettings.Instance.mobileController) {

            //	X and Y inputs based "Vertical" and "Horizontal" axes.
            inputMovementY = inputs.verticalInput * speed;
            inputMovementX = inputs.horizonalInput * speed;

            orbitX += inputs.aim.x * sensitivity;
            orbitY -= inputs.aim.y * sensitivity;

            // Clamping Y.
            orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

            if (orbitX < -360f)
                orbitX += 360f;
            if (orbitX > 360f)
                orbitX -= 360f;

        } else {

            inputMovementY = BCG_MobileCharacterController.move.y * speed;
            inputMovementX = BCG_MobileCharacterController.move.x * speed;

            orbitX += BCG_MobileCharacterController.mouse.x * sensitivity * Time.deltaTime * 100f;
            orbitY -= BCG_MobileCharacterController.mouse.y * sensitivity * Time.deltaTime * 100f;

            // Clamping Y.
            orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

            if (orbitX < -360f)
                orbitX += 360f;
            if (orbitX > 360f)
                orbitX -= 360f;

        }

    }

    private void Controller() {

        transform.Translate(inputMovementX * Time.deltaTime, 0, inputMovementY * Time.deltaTime, characterCamera.transform);
        transform.rotation = Quaternion.AngleAxis(orbitX, transform.up);

    }

    private void Camera() {

        wantedRotation = transform.rotation;

        targetPosition = transform.position;
        targetPosition -= (wantedRotation * Quaternion.Euler(orbitY, 0f, 0f)) * Vector3.forward * distance;
        targetPosition += Vector3.up * height;

        characterCamera.transform.position = targetPosition;
        characterCamera.transform.LookAt(transform);

    }

}