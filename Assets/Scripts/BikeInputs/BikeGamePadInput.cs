using System.Collections;
using System.Collections.Generic;
using BaseClasses;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class BikeGamePadInput : BikeInputBase
{
    [SerializeField] private float maxSteeringAngle = 60f;
    [SerializeField] private float maxPedalStrength = 2f;
    [SerializeField] private float drag = 0.1f;
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private float maxBrakeStrength = 5f;


    [SerializeField] private InputActionReference _iarLeftRight;
    [SerializeField] private InputActionReference _iarAccelerate;
    [SerializeField] private InputActionReference _iarBreak;
    private BikeController bikeControllerScript;
    private bool controllerConnectedWarning = true;
    private bool driveBackwards = false;
    private float _steeringAngle;
    private float _speed;

    public override InputMode InputMode => bikeControllerScript ? bikeControllerScript.InputMode : InputMode.KeyboardWASD;
    public override bool IsVR => false;

    public override void Initialize(BikeController bikeController)
    {
        bikeControllerScript = bikeController;
        _iarLeftRight.action.actionMap.Enable();
    }
    public override float GetSpeed()
    {
        return _speed;
    }
    public override float GetSteeringAngle()
    {
        return _steeringAngle;
    }
    public override void Calibrate()
    {
        _steeringAngle = 0;
    }

    void Update()
    {
        if(InputMode == InputMode.Gamepad && Gamepad.current == null && Joystick.current == null)
        {
            if (controllerConnectedWarning)
            {
                Debug.LogWarning("Gamepad Input is active, but no controller is connected");
                controllerConnectedWarning = false;
            }
            return;
        }
        controllerConnectedWarning = true;
        float steeringDiff = _iarLeftRight.action.ReadValue<float>() * maxSteeringAngle - _steeringAngle;// bikeControllerScript.steeringAngle;
        float pedalStrength = _iarAccelerate.action.ReadValue<float>();
        float brakeStrength = _iarBreak.action.ReadValue<float>();
        float currentSpeed = _speed;// bikeControllerScript.speedInMetersPerSecond;
        if(currentSpeed < 0.01f && brakeStrength == 0)
            driveBackwards = true;
        if (currentSpeed > 0.02f)
            driveBackwards = false;
        if (driveBackwards && pedalStrength == 0)
            pedalStrength = -brakeStrength / 5f;
        float speedAbs = Mathf.Abs(currentSpeed);
        float speed = Mathf.Sign(currentSpeed) * (speedAbs - (speedAbs * speedAbs * drag + friction) * Time.deltaTime); //reducing speed depending on friction and drag
        speed += pedalStrength * maxPedalStrength * Time.deltaTime; //add pedal power
        if(!driveBackwards) speed = Mathf.Max(0, speed - brakeStrength * maxBrakeStrength * Time.deltaTime); //brake
        _speed = speed;
        _steeringAngle += steeringDiff * Time.deltaTime;
        //bikeControllerScript.speedInMetersPerSecond = speed;
        //bikeControllerScript.steeringAngle += steeringDiff * Time.deltaTime; //steering is interpolated
    }
    

}
