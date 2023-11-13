using System;
using System.Collections.Generic;
using BaseClasses;
using Fusion;
using Fusion.Sockets;
using Helper;
using Network;
using UnityEngine;

namespace Common
{
    public class CarController : BaseVehicleController
    {
        private RCC_CarControllerV3 _car;

        private RCC_Inputs _newInputs = new RCC_Inputs();

        [Networked]
        private CarInputData _vehicleInputData { get; set; }

        private Rigidbody rigid;
        private float updateTime = 0;
        
        private void Awake()
        {
            _car = GetComponent<RCC_CarControllerV3>();
            _car.OverrideInputs(_newInputs);
            rigid = GetComponent<Rigidbody>();
        }
        
        public override void Spawned()
        {
            base.Spawned();

        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (!vehicle)
                return;
             _car.externalController = !HasInputAuthority;
             _car.canControl = HasInputAuthority;
             if (GetInput(out CarInputData _carInput))
             {
                 _vehicleInputData = _carInput;
             }
            

             updateTime = Time.time;
            if (!HasInputAuthority)
            {
                setClientData();
            }
            else
            {
                _newInputs.brakeInput = _inputController.GetBrake();
                _newInputs.throttleInput = _inputController.GetThrottle();
                _newInputs.steerInput = _inputController.GetSteeringAngleRaw();
                _car.OverrideInputs(_newInputs);
            }
        }

        private void setClientData()
        {
            Vector3 projectedPosition = _vehicleInputData.correctPlayerPos + _vehicleInputData.currentVelocity * (Time.time - updateTime);

            if (Vector3.Distance(transform.position, _vehicleInputData.correctPlayerPos) < 15f) {

                transform.position = Vector3.Lerp(transform.position, projectedPosition, Time.deltaTime * 5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, _vehicleInputData.correctPlayerRot, Time.deltaTime * 5f);

            } else {

                transform.position = _vehicleInputData.correctPlayerPos;
                transform.rotation = _vehicleInputData.correctPlayerRot;

            }

           _car.throttleInput = _vehicleInputData.gasInput;
           _car.brakeInput = _vehicleInputData.brakeInput;
           _car.steerInput = _vehicleInputData.steerInput;
           _car.handbrakeInput = _vehicleInputData.handbrakeInput;
           _car.boostInput = _vehicleInputData.boostInput;
           _car.clutchInput = _vehicleInputData.clutchInput;
           _car.currentGear = _vehicleInputData.gear;
           _car.direction = _vehicleInputData.direction;
           _car.changingGear = _vehicleInputData.changingGear;
           _car.semiAutomaticGear = _vehicleInputData.semiAutomaticGear;

           _car.fuelInput = _vehicleInputData.fuelInput;
           _car.engineRunning = _vehicleInputData.engineRunning;
            
        }


        protected override void QueryInput(NetworkRunner runner, NetworkInput input)
        {
            CarInputData carInputData = default;
            carInputData.gasInput = _car.throttleInput; //_vehicleInputData.GetAcceleration();
            carInputData.brakeInput = _car.brakeInput; //_c.GetBrake();
            carInputData.steerInput = _car.steerInput;// _inputController.GetSteeringAngleRaw();
            carInputData.handbrakeInput = _car.handbrakeInput;
            carInputData.boostInput = _car.boostInput;
            carInputData.clutchInput = _car.clutchInput;
            carInputData.gear = _car.currentGear;
            carInputData.direction = _car.direction;
            carInputData.changingGear = _car.changingGear;
            carInputData.semiAutomaticGear = _car.semiAutomaticGear;

            carInputData.fuelInput = _car.fuelInput;
            carInputData.engineRunning = _car.engineRunning;

            carInputData.correctPlayerPos = transform.position;
            carInputData.correctPlayerRot = transform.rotation;
            carInputData.currentVelocity = rigid.velocity;
            input.Set(carInputData);
        }


        public override void Render()
        {
            base.Render();
            VehicleInputData.SteeringAngle = _car.steerAngle;
            // _newInputs.brakeInput = _inputController.GetBrake();
            // _newInputs.throttleInput = _inputController.GetAcceleration();
            // _newInputs.steerInput = _inputController.GetSteeringAngleRaw();
            // _car.OverrideInputs(_newInputs);
        }
    }
}