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

/// <summary>
/// Simple FPS Character controller used in demo scenes. Not professional.
/// You can use your 3rd party or any character controller instead of this script. 
/// </summary>
public class BCG_FPSController : MonoBehaviour {

    private BCG_EnterExitPlayer enterExitPlayer;
    private bool canControl = true;

    #region Controller

    public float speed = 4f;     //	Maixmum directional speed.

    private float inputMovementY;       //	Input Y clamped (-1, 1f)
    private float inputMovementX;       //	Input X clamped (-1, 1f)

    #endregion

    #region Camera

    public Camera characterCamera;      //	Camera of the character.
    public float sensitivity = 1f;        //	Sensitivity of the camera.
    public float smoothing = 10f;      //	Smoothing factor of the camera.

    private Vector2 mouseInputVector = new Vector2(0f, 0f);       //	Mouse input for the camera.

    // smooth the mouse moving
    private Vector2 smoothV = new Vector2(0f, 0f);

    #endregion

    public BCG_Inputs inputs = new BCG_Inputs();

    void Start() {

        enterExitPlayer = GetComponent<BCG_EnterExitPlayer>();

        //	Find character camera at the start.
        if (!characterCamera)
            characterCamera = GetComponentInChildren<Camera>();

    }

    void FixedUpdate() {

        // Override canControl bool from BCG_EnterExitPlayer. 
        if(enterExitPlayer)
            canControl = enterExitPlayer.canControl;

        //	If canControl is enabled, enable the camera, receive inputs from the player, and feed it.
        if (canControl)
            Controller();       //	Process the controller.

    }

    private void Update() {

        // Override canControl bool from BCG_EnterExitPlayer. 
        if (enterExitPlayer)
            canControl = enterExitPlayer.canControl;

        //	If canControl is enabled, enable the camera, receive inputs from the player, and feed it.
        if (canControl) {

            if(characterCamera)
                characterCamera.gameObject.SetActive(true);

            Inputs();       //	Receive inputs form the player.
            Camera();       //	Process the camera.

        } else {

            if (characterCamera)
                characterCamera.gameObject.SetActive(false);

        }

    }

    /// <summary>
    /// Receive inputs from the player.
    /// </summary>
    private void Inputs() {

        inputs = BCG_InputManager.Instance.inputs;

        //Receive keyboard inputs if controller type is not mobile.If controller type is mobile, inputs will be received by BCG_MobileCharacterController component attached to FPS/ TPS Controller UI Canvas.
        if (!BCG_EnterExitSettings.Instance.mobileController) {

            //	X and Y inputs based "Vertical" and "Horizontal" axes.
            inputMovementY = inputs.verticalInput * speed;
            inputMovementX = inputs.horizonalInput * speed;

            // Mouse delta
            var mouseDelta = new Vector2(inputs.aim.x * sensitivity, inputs.aim.y * sensitivity);

            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, smoothing * Time.deltaTime);
            smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, smoothing * Time.deltaTime);

            // incrementally add to the camera look
            mouseInputVector += smoothV;
            mouseInputVector = new Vector3(mouseInputVector.x, Mathf.Clamp(mouseInputVector.y, -75f, 75f));

        } else {

            //	Receiving X and Y inputs from mobile inputs.
            inputMovementY = BCG_MobileCharacterController.move.y * speed;
            inputMovementX = BCG_MobileCharacterController.move.x * speed;

            // Mouse delta
            var mouseDelta = new Vector2(BCG_MobileCharacterController.mouse.x * sensitivity, BCG_MobileCharacterController.mouse.y * sensitivity);

            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, smoothing * Time.deltaTime);
            smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, smoothing * Time.deltaTime);

            // incrementally add to the camera look
            mouseInputVector += smoothV * Time.deltaTime * 100f;
            mouseInputVector = new Vector3(mouseInputVector.x, Mathf.Clamp(mouseInputVector.y, -75f, 75f));

        }

    }

    private void Controller() {

        // Translating the character with X and Y directions.
        transform.Translate(inputMovementX * Time.deltaTime, 0, inputMovementY * Time.deltaTime);

    }

    private void Camera() {

        //	Setting rotations of the camera.
        characterCamera.transform.localRotation = Quaternion.AngleAxis(-mouseInputVector.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(mouseInputVector.x, transform.up);

    }

}