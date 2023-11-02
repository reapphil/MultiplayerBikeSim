using System.Collections.Generic;
using BaseClasses;
using Enums;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Uduino;

/*
This script enables steering the bike with a VR controller. As well as controlling the speed with serial data from an arduino.
One of the VR controllers is simply attached to the handlebar of the bike.

Resetting the steering angle can be done by pressing the button on the script in the inspector or pressing the trigger on the controller used for steering.
The code simply sets the calculated speed and steering angle in bike controller script.
*/


public class BikeVRInput : BikeInputBase
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ActionBasedController roationController;
    //[SerializeField] private Transform handleBar; //nessesairy to get the correct rotation axis

    [SerializeField] private bool printSerialData = false;
    private Quaternion controllerZeroRotation;
    private Quaternion initialHandlebarRotation;
    private Quaternion controllerRotationSmoothed;
    private BikeController bikeControllerScript;

    private float _speed;
    private float _steeringAngle;

    public override InputMode InputMode => InputMode.VR;
    public override bool IsVR => true;

    public override void Initialize(BikeController bikeController)
    {
        bikeControllerScript = bikeController;
        UduinoManager.Instance.OnDataReceived += ProcessSerialData;

        controllerZeroRotation = roationController.transform.localRotation;
        initialHandlebarRotation = bikeControllerScript.HandleBar.localRotation;
        bikeControllerScript = GetComponent<BikeController>();
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Left, inputDevices);
        controllerRotationSmoothed = roationController.transform.localRotation;
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
        
    }

    void Update()
    {
        if(roationController.activateActionValue.action.ReadValue<float>() > 0.5f)
            recenterSteering();

        //calculate difference between current and initial rotation of the controller
        //rotate this rotation with the inverse (initial) rotaion of the handle bar (to consider head angle in rotation)
        //take euler y component of the rotation
        float steeringAngle = (Quaternion.Inverse(initialHandlebarRotation)*(Quaternion.Inverse(controllerZeroRotation) * roationController.transform.localRotation)).eulerAngles.y;
        _steeringAngle = steeringAngle;
        //bikeControllerScript.steeringAngle = steeringAngle;
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
                if (float.TryParse(value, out float speed))
                {
                    speed /= 3.6f; //conversion to m/s
                    _speed = speed;
                    //bikeControllerScript.speedInMetersPerSecond = speed;
                }
            }
        }
    }

    public void recenterSteering()
    {
        controllerZeroRotation = roationController.transform.localRotation;
    }


}
