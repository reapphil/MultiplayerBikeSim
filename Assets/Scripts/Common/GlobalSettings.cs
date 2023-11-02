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
        public BikeInputBase GamePadControllerPrefab;
        public BikeInputBase KeyboardWASDControllerPrefab;
        public BikeInputBase KeyboardCursorControllerPrefab;
        public BikeInputBase VRControllerPrefab;
        public BikeInputBase CustomInput1Prefab;
        public BikeInputBase CustomInput2Prefab;
        public BikeInputBase CustomInput3Prefab;
        

        [Header("Other Prefabs")] 
        public BikeController BikePrefab;
        public BikeRider BikeRiderPrefab;
        public GameController GameControllerPrefab;
        
        
        
        public BikeInputBase GetInputControllerPrefab(InputMode inputMode)
        {
            BikeInputBase inputController = null;
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
    }
}