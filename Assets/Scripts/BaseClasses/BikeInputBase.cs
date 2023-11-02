using Enums;
using UnityEditor;
using UnityEngine;

namespace BaseClasses
{
    public abstract class BikeInputBase : MonoBehaviour
    {
        [Tooltip("First person camera")]
        [SerializeField] private Camera _fpCamera;
        /// <summary>
        /// First person camera
        /// </summary>
        public Camera FpCamera => _fpCamera;

        [Tooltip("Third person camera")]
        [SerializeField] private Camera _tpCamera;
        /// <summary>
        /// Third person camera
        /// </summary>
        public Camera TpCamera => _tpCamera;
        
        public abstract InputMode InputMode { get; }
        
        public abstract bool IsVR { get; }

        public abstract void Initialize(BikeController bikeController);
        public abstract float GetSpeed();
        public abstract float GetSteeringAngle();
        public abstract void Calibrate();
    }
}