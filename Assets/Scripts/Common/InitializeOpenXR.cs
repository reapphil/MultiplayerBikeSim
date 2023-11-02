using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Management;

namespace _BoilUp.Scripts.Common
{


    public class InitializeOpenXR : MonoBehaviour
    {
        bool _isShutdownComplete = false;

        public static InitializeOpenXR Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance.isActiveAndEnabled)
                {
                    Destroy(this);
                    return;
                }
                else
                {
                    Destroy(Instance);
                    Instance = this;
                }
            }
            Application.wantsToQuit += ApplicationWantsToQuit;
        }

        private void OnDestroy()
        {
            Application.wantsToQuit -= ApplicationWantsToQuit;
        }

        private void OnEnable()
        {
            UnityEngine.Debug.Log("XR: Start OpenXR");

            if (XRGeneralSettings.Instance.Manager.activeLoaders.Count == 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("No XR Plugin selected", "Please select an XR Plugin. This Sample was tested with OpenXR. For more detailed setup information please refer to Assets/README.md", "Ok");
                UnityEditor.SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
#endif
                UnityEngine.Debug.LogError("No XR Loader is selected in xr Managment");
            }

            StartCoroutine(Restart(true, null));
        }

        bool ApplicationWantsToQuit()
        {
            if (_isShutdownComplete)
            {
                return true;
            }

            StartCoroutine(Restart(false, OnShutdownComplete));
            return false;
        }

        void OnShutdownComplete()
        {
            _isShutdownComplete = true;
            Application.Quit();
        }

        IEnumerator Restart(bool shouldRestart, UnityAction onShutdown)
        {
            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                UnityEngine.Debug.Log("XR: Stop subsystems." + XRGeneralSettings.Instance.Manager.isInitializationComplete + ", " + XRGeneralSettings.Instance.Manager.activeLoaders.Count);
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                //yield return null;

                UnityEngine.Debug.Log("XR: Deinitizalize Loader." + XRGeneralSettings.Instance.Manager.isInitializationComplete + ", " + XRGeneralSettings.Instance.Manager.activeLoaders.Count);
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
                yield return null;
            }

            onShutdown?.Invoke();
            if (shouldRestart)
            {
                UnityEngine.Debug.Log("XR: Initialize Loader");
                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
                while (XRGeneralSettings.Instance.Manager.isInitializationComplete == false)
                {
                    yield return null;
                }

                UnityEngine.Debug.Log("XR: Start Subsystems");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                yield return null;

                if (XRGeneralSettings.Instance.Manager.activeLoader != null)
                {
                    UnityEngine.Debug.Log("XR: Successful restart.");
                }
                else
                {
                    UnityEngine.Debug.LogError("XR: Failure to restart OpenXRLoader after shutdown.");
                }
            }
        }

    }
}
