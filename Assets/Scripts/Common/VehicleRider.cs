using System;
using System.Collections;
using BaseClasses;
using Enums;
using Fusion;
using Network;
using UnityEngine;

namespace Common
{
    public class VehicleRider : NetworkBehaviour
    {
        [Networked]
        public PlayerData PlayerData { get; set; }
        [Networked]
        public BaseVehicleController Vehicle { get; set; }
        
        public bool IsVR;
        public InputBase InputController;
        public bool IsLocal;
        public bool AddCameraController => PlayerData.AddCameraController;
        public InputMode InputMode => (InputMode)PlayerData.InputMode;
        public CameraMode CameraMode => (CameraMode)PlayerData.CameraMode;

        public override void Spawned()
        {
            base.Spawned();
            if (HasInputAuthority)
            {
                PlayerData playerData = default;
                playerData.InputMode = (int)(GameLauncher.IsClone
                    ? GameLauncher.Instance.InputModeClone
                    : GameLauncher.Instance.InputMode);
                playerData.AddCameraController = GameLauncher.IsClone
                    ? GameLauncher.Instance.AddCameraControllerClone
                    : GameLauncher.Instance.AddCameraController;
                playerData.CameraMode = (int)(GameLauncher.IsClone
                    ? GameLauncher.Instance.CameraModeClone
                    : GameLauncher.Instance.CameraMode);
                playerData.VehiclePrefab = (int)(GameLauncher.IsClone
                    ? GameLauncher.Instance.VehiclePrefabClone
                    : GameLauncher.Instance.VehiclePrefab);
                playerData.Name = (GameLauncher.IsClone
                    ? GameLauncher.Instance.PlayerNameClone
                    : GameLauncher.Instance.PlayerName);
                RPC_setPlayerData(playerData);
            }
            StartCoroutine(waitForBikeToBeSpawned());
        }

        [Rpc(RpcSources.InputAuthority,RpcTargets.StateAuthority)]
        private void RPC_setPlayerData(PlayerData playerData)
        {
            playerData.Initialized = true;
            PlayerData = playerData;
            spawnVehicle(GameController.Instance.GetSpawnPoint());
        }

        IEnumerator waitForBikeToBeSpawned()
        {
            while (!Vehicle || !PlayerData.Initialized)
            {
                yield return null;
            }

            if (HasInputAuthority)
            {
                loadInputController();
                Vehicle.Initialize(this, InputController);
                InputController.Initialize(Vehicle);
            }
            else
            {
                Vehicle.Initialize(this, InputController);
                Vehicle.DeactivateCamera();
            }
        }
        

        private void spawnVehicle(Transform spawnPoint)
        {
            if (HasStateAuthority)
            {
                BaseVehicleController vehiclePrefab = GlobalSettings.Instance.GetVehiclePrefab((VehiclePrefab)PlayerData.VehiclePrefab);
                Vehicle = Runner.Spawn(vehiclePrefab, spawnPoint.position + new Vector3(0,vehiclePrefab.SpawnOffsetY,0), spawnPoint.rotation,
                    Object.InputAuthority);
                Vehicle.transform.SetParent(transform);
            }
        }

        private void loadInputController()
        {
            InputBase inputControllerPrefab = GameController.GlobalSettings.GetInputControllerPrefab(InputMode);
            if (inputControllerPrefab)
            {
                InputController = Instantiate(inputControllerPrefab, Vehicle.transform)
                    .GetComponent<InputBase>();
            }
            else
            {
                Debug.LogError($"InputPrefab for input mode '{InputMode}' not set.");
            }
        }
    }
}