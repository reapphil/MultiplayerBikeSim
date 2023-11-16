using System.Collections.Generic;
using System.Globalization;
using BaseClasses;
using Enums;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Uduino;
using UnityEngine.Serialization;

/*
This script enables steering the bike with a VR controller. As well as controlling the speed with serial data from an arduino.
One of the VR controllers is simply attached to the handlebar of the bike.

Resetting the steering angle can be done by pressing the button on the script in the inspector or pressing the trigger on the controller used for steering.
The code simply sets the calculated speed and steering angle in bike controller script.
*/


public class BikeVRInput : InputBase
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ActionBasedController roationController;

    [SerializeField] private Transform _xrOrigin;
    //[SerializeField] private Transform handleBar; //nessesairy to get the correct rotation axis

    [SerializeField] private bool printSerialData = false;
    private Quaternion controllerZeroRotation;
    private Quaternion initialHandlebarRotation;
    private Quaternion controllerRotationSmoothed;
    private BikeController _bikeControllerScript;

    private float _speed;
    private float _steeringAngle;
    public override SpeedComposition SpeedComposition => SpeedComposition.SingleValue;

    public override void Initialize(BaseVehicleController baseVehicleController)
    {
        _bikeControllerScript = baseVehicleController as BikeController;
        
        UduinoManager.Instance.OnDataReceived += ProcessSerialData;
        
        controllerZeroRotation = roationController.transform.localRotation;
        initialHandlebarRotation = _bikeControllerScript.HandleBar.localRotation;
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Left, inputDevices);
        controllerRotationSmoothed = roationController.transform.localRotation;
    }

    protected override float getSpeed()
    {
        return _speed;
    }

    protected override float getThrottle()
    {
        return 0;
    }

    protected override float getBrake()
    {
        return 0;
    }
    
    protected override float getSteeringAngle()
    {
        return _steeringAngle;
    }

    public override void Calibrate()
    {
        controllerZeroRotation = roationController.transform.localRotation;
        Quaternion yRot = Quaternion.Euler(0,_camera.transform.localEulerAngles.y,0);
        Vector3 xrPos = _camera.transform.localPosition;
        xrPos.y = 0;
        _xrOrigin.localPosition = -xrPos;
        transform.localRotation = Quaternion.Inverse(yRot);

    }

    void Update()
    {
        if(roationController.activateActionValue.action.ReadValue<float>() > 0.5f)
            Calibrate();

        if (controllerZeroRotation == Quaternion.identity && roationController.transform.localRotation != Quaternion.identity)
        {
            controllerZeroRotation = roationController.transform.localRotation;
            Calibrate();
        }

        if (controllerZeroRotation != Quaternion.identity)
        {
            float steeringAngle =
                (Quaternion.Inverse(controllerZeroRotation) * roationController.transform.localRotation).eulerAngles.y;
            _steeringAngle = steeringAngle;
        }
    }

    void ProcessSerialData(string data, UduinoDevice device)
    {
        if (!enabled)
            return;

        if (printSerialData)
            Debug.Log(data);
        
        //example serial data from current arduino code: "speedOut 0.00,frontbrake 44,rearbrake 44,combined 88,resistance 4"
        string[] values = data.Split(',');
        if(values.Length == 0)
            return;

        for (int i = 0; i < values.Length; i++)
        {
            string[] keyValue = values[i].Split(' ');
            if(keyValue.Length != 2)
                break;
            string key = keyValue[0];
            string value = keyValue[1];

            if(key == "speedOut")
            {
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float speed))
                {
                    speed /= 3.6f; //conversion to m/s
                    _speed = speed;
                    Debug.Log($"Speed: {_speed:N2}");
                    //bikeControllerScript.speedInMetersPerSecond = speed;
                }
            }
        }
    }
}
