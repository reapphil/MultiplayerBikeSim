using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Common
{
    public class CameraController: MonoBehaviour
    {
        public float RotationSpeed = 10f;
        public float MaxXRotation = 75f;
        public float MinXRotation = -75f;

        private float _rotationX = 0f;
        private float _rotationY = 0f;

        private void Update()
        {
            setCameraInput();
        }

        private void setCameraInput()
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            _rotationX -= mouseDelta.y * RotationSpeed*Time.deltaTime;
            _rotationX = Mathf.Clamp(_rotationX, MinXRotation, MaxXRotation);
            _rotationY = transform.localEulerAngles.y + mouseDelta.x * RotationSpeed*Time.deltaTime;

        }

        private void LateUpdate()
        {
            RotateCamera();
        }

        void RotateCamera()
        {
            transform.localRotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
        }
    }
}