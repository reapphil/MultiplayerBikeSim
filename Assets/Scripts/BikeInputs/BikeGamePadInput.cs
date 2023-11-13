using System.Collections;
using System.Collections.Generic;
using BaseClasses;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class BikeGamePadInput : InputBase
{
    [SerializeField] private InputActionReference _iarLeftRight;
    [SerializeField] private InputActionReference _iarAccelerate;
    [SerializeField] private InputActionReference _iarBreak;
    private BaseVehicleController _vehicleControllerScript;
    private bool controllerConnectedWarning = true;

    private float _steeringAngle;
    private float _speed;
    private float _pedalStrength;
    private float _brakeStrength;

    public virtual bool IsGamePad => true;
    
    public override SpeedComposition SpeedComposition => SpeedComposition.SeperateValues;

    public override void Initialize(BaseVehicleController baseVehicleController)
    {
        _vehicleControllerScript = baseVehicleController;
        _iarLeftRight.action.actionMap.Enable();
    }

    protected override float getSteeringAngle()
    {
        return _steeringAngle;
    }
    
    public override void Calibrate()
    {
        _steeringAngle = 0;
    }

    void Update()
    {
        if(IsGamePad && Gamepad.current == null && Joystick.current == null)
        {
            if (controllerConnectedWarning)
            {
                Debug.LogWarning("Gamepad Input is active, but no controller is connected");
                controllerConnectedWarning = false;
            }
            return;
        }
        controllerConnectedWarning = true;
        _steeringAngle = _iarLeftRight.action.ReadValue<float>();
        _pedalStrength = _iarAccelerate.action.ReadValue<float>();
        _brakeStrength = _iarBreak.action.ReadValue<float>();
    }

    protected override float getSpeed()
    {
        return 0;
    }

    protected override float getThrottle()
    {
        return _pedalStrength;
    }

    protected override float getBrake()
    {
        return _brakeStrength;
    }
}
