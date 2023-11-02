using System;
using System.Collections;
using BaseClasses;
using Enums;
using Fusion;
using UnityEngine;

namespace Common
{
    public class BikeRider : NetworkBehaviour
    {
        public InputMode InputMode;
        public CameraMode CameraMode;
        public bool IsVR;
        [Networked]
        public BikeController Bike { get; set; }
        public BikeInputBase InputController;

        public override void Spawned()
        {
            base.Spawned();
            spawnBike(GameController.Instance.GetSpawnPoint());
            StartCoroutine(waitForBikeToBeSpawned());
        }

        IEnumerator waitForBikeToBeSpawned()
        {
            while (!Bike)
            {
                yield return null;
            }

            if (HasInputAuthority)
            {
                loadInputController();
                Bike.Initialize(this, InputController);
                InputController.Initialize(Bike);
                Bike.SetCameras(InputController.FpCamera, InputController.TpCamera);
            }
            else
            {
                Bike.Initialize(this, InputController);
                Bike.DeactivateCamera();
            }
        }
        

        private void spawnBike(Transform spawnPoint)
        {
            if (HasStateAuthority)
            {
                Bike = Runner.Spawn(GlobalSettings.Instance.BikePrefab, spawnPoint.position, spawnPoint.rotation,
                    Object.InputAuthority);
                Bike.transform.SetParent(transform);
            }
        }

        private void loadInputController()
        {
            BikeInputBase inputControllerPrefab = GameController.GlobalSettings.GetInputControllerPrefab(InputMode);
            if (inputControllerPrefab)
            {
                InputController = Instantiate(inputControllerPrefab, Bike.transform)
                    .GetComponent<BikeInputBase>();
            }
            else
            {
                Debug.LogError($"InputPrefab for input mode '{InputMode}' not set.");
            }
        }
    }
}