using Enums;
using UnityEditor;
using UnityEngine;

namespace BaseClasses
{
    public enum SpeedComposition
    {
        SingleValue,
        SeperateValues
    }
    
    public abstract class InputBase : MonoBehaviour
    {
        [SerializeField] private float maxSteeringAngle = 60f;
        [SerializeField] private float maxPedalStrength = 2f;
        [SerializeField] private float drag = 0.1f;
        [SerializeField] private float friction = 0.1f;
        [SerializeField] private float maxBrakeStrength = 5f;
        
        private float _internalSpeed;
        private float _internalSteeringAngle;
        private bool driveBackwards = false;
        
        
        /// <summary>
        /// Specify how the speed of the bike is composed an queried by the base class.
        /// If set to SingleValue, GetSpeed() is called (used in the Uduino implementation, where the speed is directly computed)
        /// If set to SeperateValues, GetThrottle() and GetBreak() is called and the speed is composed in the base class.
        /// </summary>
        public abstract SpeedComposition SpeedComposition { get; } 

        /// <summary>
        /// This method hands over the BaseVehicleController of your assigned vehicle, if in any case you would need it. The BaseVehicleController contains the common physics and interaction with the rider/vehicle which is independent of the InputController.
        /// In this method you should also implement everything you need to initialize your custom input controller. 
        /// </summary>
        /// <param name="baseVehiclecontroller"></param>
        public abstract void Initialize(BaseVehicleController baseVehiclecontroller);
        
        
        
        /// <summary>
        /// If SpeedComposition is set to SingleValue, getSpeed() is called for querying the speed of the vehicle and directly passed on to the vehicle controller. 
        /// </summary>
        /// <returns>Speed in m/s</returns>
        protected abstract float getSpeed();
        
        /// <summary>
        /// If SpeedComposition is set to SeperateValues, getThrottle() is called for querying the throttle of the vehicle and the speed is calculated in combination with getBreak(). 
        /// </summary>
        /// <returns>Value between 0 and 1</returns>
        protected abstract float getThrottle();
        
        /// <summary>
        /// If SpeedComposition is set to SeperateValues, getBreak() is called for querying the brake of the vehicle and the speed is calculated in combination with getThrottle(). 
        /// </summary>
        /// <returns>Value between 0 and 1</returns>
        protected abstract float getBrake();
        
        /// <summary>
        /// Returns the current steering Angle which will be directly passed on to the VehicleController.  
        /// </summary>
        /// <returns>Value between -1 and 1</returns>
        protected abstract float getSteeringAngle();
        
        /// <summary>
        /// Calibrates the input controller. Implement if needed.
        /// </summary>
        public abstract void Calibrate();

        public float GetSpeed()
        {
            switch (SpeedComposition)
            {
                case SpeedComposition.SingleValue:
                    return getSpeed();
                case SpeedComposition.SeperateValues:
                    return calculatedSpeed();
            }

            return 0;
        }

        public float GetThrottle()
        {
            return getThrottle();
        }

        public float GetBrake()
        {
            return getBrake();
        }

        public float GetSteeringAngle()
        {
            float steeringInput = getSteeringAngle();
            float steeringDiff =  steeringInput * maxSteeringAngle - _internalSteeringAngle;// bikeControllerScript.steeringAngle;
            _internalSteeringAngle += steeringDiff * Time.deltaTime;
            return _internalSteeringAngle;
        }

        public float GetSteeringAngleRaw()
        {
            return getSteeringAngle();
        }

        private float calculatedSpeed()
        {
            float currentSpeed = _internalSpeed;// bikeControllerScript.speedInMetersPerSecond;
            float brakeStrength = getBrake();
            float pedalStrength = getThrottle();
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
            _internalSpeed = speed;
            return _internalSpeed;
        }
    }
}