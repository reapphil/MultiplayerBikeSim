using Fusion;
using UnityEngine;

namespace Network
{
    public struct CarInputData : INetworkInput
    {
        public  Vector3 currentVelocity;
        public float updateTime;
        public float gasInput;
        public float brakeInput;
        public float steerInput;
        public float handbrakeInput;
        public float boostInput;
        public float clutchInput;
        public int gear;
        public int direction;
        public bool changingGear;
        public bool semiAutomaticGear;
        public float fuelInput;
        public bool engineRunning;
        public Vector3 correctPlayerPos;
        public Quaternion correctPlayerRot;
    }
}