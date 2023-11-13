using BaseClasses;
using Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common
{
    public class GlobalSettings : MonoBehaviour
    {
        private static GlobalSettings _instance;
        public static GlobalSettings Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<GlobalSettings>();
                }

                return _instance;
            }
        }
        
        [Header("Input Prefabs")] 
        public InputBase GamePadControllerPrefab;
        public InputBase KeyboardWASDControllerPrefab;
        public InputBase KeyboardCursorControllerPrefab;
        public InputBase VRControllerPrefab;
        public InputBase CustomInput1Prefab;
        public InputBase CustomInput2Prefab;
        public InputBase CustomInput3Prefab;
        

        [Header("Other Prefabs")] 
        public BaseVehicleController StandardBikePrefab;
        public BaseVehicleController StandardCarPrefab;
        public BaseVehicleController CustomVehiclePrefab;
        
        [FormerlySerializedAs("_riderPrefab")] [FormerlySerializedAs("BikeRiderPrefab")] public VehicleRider _vehicleRiderPrefab;
        public GameController GameControllerPrefab;
        
        
        
        public InputBase GetInputControllerPrefab(InputMode inputMode)
        {
            InputBase inputController = null;
            switch (inputMode)
            {
                case InputMode.KeyboardWASD:
                    inputController = KeyboardWASDControllerPrefab;
                    break;
                case InputMode.KeyboardCursor:
                    inputController = KeyboardCursorControllerPrefab;
                    break;
                case InputMode.Gamepad:
                    inputController = GamePadControllerPrefab;
                    break;
                case InputMode.VR:
                    inputController = VRControllerPrefab;
                    break;
                case InputMode.CustomInput1:
                    inputController = CustomInput1Prefab;
                    break;
                case InputMode.CustomInput2:
                    inputController = CustomInput2Prefab;
                    break;
                case InputMode.CustomInput3:
                    inputController = CustomInput3Prefab;
                    break;
            }

            if (!inputController)
            {
                Debug.LogError($"Selected input mode has no ControllerPrefab assigned. Please make sure that a controller is assigned int the GlobalSettings found at the GameLauncher object in the scene.");
            }

            return inputController;
        }

        public BaseVehicleController GetVehiclePrefab(VehiclePrefab vehiclePrefab)
        {
            BaseVehicleController prefab = null;
            switch (vehiclePrefab)
            {
                case VehiclePrefab.StandardBike:
                    prefab = StandardBikePrefab;
                    break;
                case VehiclePrefab.StandardCar:
                    prefab = StandardCarPrefab;
                    break;
                case VehiclePrefab.CustomVehicle:
                    prefab = CustomVehiclePrefab;
                    break;
            }

            if (!prefab)
            {
                Debug.LogError($"{vehiclePrefab.ToString()} prefab not set in GlobalSettings (Component in GameLauncher gameObject in Hierachy)");
            }

            return prefab;
        }
    }
}