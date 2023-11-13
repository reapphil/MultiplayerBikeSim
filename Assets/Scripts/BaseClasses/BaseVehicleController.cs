using System;
using System.Collections.Generic;
using Common;
using Enums;
using Fusion;
using Fusion.Sockets;
using Network;
using UnityEngine;

namespace BaseClasses
{
    public class BaseVehicleController : NetworkBehaviour, INetworkRunnerCallbacks
    {
        [Networked]
        public VehicleRider VehicleRider { get; set; }
    
        [Networked]
        protected float _speedInMetersPerSecond { get; set; }
    
        [Networked]
        protected float _steeringAngle { get; set; }

        public float SpawnOffsetY = 0;
        [SerializeField] protected GameObject _thirdPersonCamera;
        [SerializeField] protected GameObject _firstPersonCamera;
        
        public PlayerName PlayerNameControl;
        
        protected InputBase _inputController;
        public InputMode InputMode => VehicleRider.InputMode;
        public CameraMode CameraMode => VehicleRider.CameraMode;
        
        protected VehicleInputData VehicleInputData;
        protected NetworkTransform _networkTransform;
        protected GameObject vehicle;

        public GameObject ActiveCameraGO => CameraMode == CameraMode.FirstPerson ? _firstPersonCamera : _thirdPersonCamera;

        public override void Spawned()
        {
            base.Spawned();
            _networkTransform = GetComponent<NetworkTransform>();
            if (HasInputAuthority)
            {
                Runner.AddCallbacks(this);
                if(_networkTransform)
                    _networkTransform.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            }
        }
        
        public override void Render()
        {
            base.Render();
            if (HasInputAuthority && _inputController != null)
            {
                VehicleInputData.Speed = _inputController.GetSpeed();
                VehicleInputData.Throttle = _inputController.GetThrottle();
                VehicleInputData.Brake = _inputController.GetBrake();
                VehicleInputData.SteeringAngle = _inputController.GetSteeringAngle();
                VehicleInputData.SteeringAngleRaw = _inputController.GetSteeringAngleRaw();
            }
        }
        
        

        public virtual void Initialize(VehicleRider vehicleRider, InputBase inputController)
        {
            VehicleRider = vehicleRider;
            _inputController = inputController;
            vehicle = gameObject;
            PlayerNameControl.SetPlayerName(vehicleRider.PlayerData.Name.Value);
            if (vehicleRider.HasInputAuthority)
            {
                PlayerManager.LocalVehicle = this;
                PlayerNameControl.gameObject.SetActive(false);
                setCameraMode();
                if (!vehicleRider.IsVR && vehicleRider.AddCameraController)
                {
                    if(_thirdPersonCamera)
                        _thirdPersonCamera.gameObject.AddComponent<CameraController>();
                    if(_firstPersonCamera)
                        _firstPersonCamera.gameObject.AddComponent<CameraController>();
                }

                LevelSettings.Instance.DeactivateOverviewCamera();
            }
            else
            {
                DeactivateCamera();
            }
        }
        
        protected void setCameraMode()
        {
            if (InputMode == InputMode.VR)
                return;
            switch (CameraMode)
            {
                case CameraMode.ThirdPerson:
                    if(_thirdPersonCamera)
                        _thirdPersonCamera.gameObject.SetActive(true);
                    if(_firstPersonCamera)
                        _firstPersonCamera.gameObject.SetActive(false);
                    break;
                case CameraMode.FirstPerson:
                    if(_thirdPersonCamera)
                        _thirdPersonCamera.gameObject.SetActive(false);
                    if(_firstPersonCamera)
                        _firstPersonCamera.gameObject.SetActive(true);
                    break;
            }
        }
        
        public void DeactivateCamera()
        {
            if(_thirdPersonCamera)
                _thirdPersonCamera.gameObject.SetActive(false);
            if(_firstPersonCamera)
                _firstPersonCamera.gameObject.SetActive(false);
        }
        
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            QueryInput(runner,input);
        }

        protected virtual void QueryInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(VehicleInputData);
        }
        

        #region Unused Fusion-Callbacks
    
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    
        #endregion
    }
}