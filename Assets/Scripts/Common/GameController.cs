using System;
using Enums;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering;

namespace Common
{
    public class GameController : NetworkBehaviour
    {
        private static GameController _instance;
        public static GameController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<GameController>();
                }

                return _instance;
            }
        }
        
        private static GlobalSettings _globalSettings;
        public static GlobalSettings GlobalSettings
        {
            get
            {
                if (!_globalSettings)
                {
                    _globalSettings = FindObjectOfType<GlobalSettings>();
                }

                return _globalSettings;
            }
        }

        public bool IsInHub { get; set; }


        private int _spawnPointIndex = 0;

        private void Awake()
        {
            if (_instance)
            {
                Destroy(this);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }




        public Transform GetSpawnPoint()
        {
            Transform spawnPoint = null;
            if (LevelSettings.Instance)
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                if (LevelSettings.Instance.SpawnPoints.Length == 0)
                {
                    Debug.LogWarning("No SpawnPoints are defined in the LevelSettings");
                }
                else
                {
                    spawnPoint = LevelSettings.Instance.SpawnPoints[_spawnPointIndex];
                    _spawnPointIndex = (_spawnPointIndex + 1) % LevelSettings.Instance.SpawnPoints.Length;
                }
            }
            else
            {
                Debug.LogError("LevelSettings are missing in the scene!");
            }

            return spawnPoint;
        }

        public void ResetGameController()
        {
            
        }
    }
}