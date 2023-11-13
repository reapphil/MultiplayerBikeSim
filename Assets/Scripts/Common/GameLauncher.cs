using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Fusion;
using Fusion.Sockets;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    [RequireComponent(typeof(GlobalSettings))]
    public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static bool IsClone
        {
            get
            {

#if UNITY_EDITOR
                return ParrelSync.ClonesManager.IsClone();
#else
                return false;
#endif
            }
        }

        public static GameLauncher Instance;

        [Header("Startup")]
        [Tooltip("Set to true, if you are testing a specific scene (Not the lobby) to accept the connection request")]
        public bool IsDirectStart;
        public string SessionId = "FHOOE";
        [Header("Master Startup")] 
        public string PlayerName; 
        public InputMode InputMode;
        public bool AddCameraController = true;
        public VehiclePrefab VehiclePrefab;
        public CameraMode CameraMode;
        public GameMode NetworkInstanceType;
        [Header("Clone Startup")] 
        public string PlayerNameClone;
        public InputMode InputModeClone;
        public bool AddCameraControllerClone = true;
        public VehiclePrefab VehiclePrefabClone;
        public CameraMode CameraModeClone;
        public GameMode NetworkInstanceTypeClone;


        private GameMode _gameMode;
        private NetworkRunner _runner;
        public GameMode GameMode => _gameMode;
        public NetworkRunner Runner => _runner;
        private BikeSimulatorNetworkPool _pool;
        private BikeSimulatorSceneManager _simulatorSceneManager;
        
        public static ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

        private GameController _gameController;

        public void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void Start()
        {
            GameLauncher[] gameLauncher = FindObjectsOfType<GameLauncher>();
            if (gameLauncher.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            _simulatorSceneManager = GetComponent<BikeSimulatorSceneManager>();
            DontDestroyOnLoad(gameObject);
            setStartParameter();
            JoinOrCreateLobby();
        }

        private void setStartParameter()
        {
            _gameMode = IsClone ? NetworkInstanceTypeClone : NetworkInstanceType;
        }

        public void JoinOrCreateLobby()
        {
            //LoadingController.Instance.SetImmediately(true);
            StartCoroutine(joinOrCreateLobby());
        }


        IEnumerator joinOrCreateLobby()
        {
            // while (LoadingController.Instance.Fadeing)
            // {
            //     yield return null;
            // }
            yield return new WaitForEndOfFrame(); // Just to yield a result, till the loading controller is implemented 
            SetConnectionStatus(ConnectionStatus.Connecting);
            if (!_runner)
            {
                _runner = FindObjectOfType<NetworkRunner>();
            }

            if (_runner != null)
            {
                LeaveSession();
            }

            GameObject go = new GameObject("Session");
            DontDestroyOnLoad(go);

            _runner = go.AddComponent<NetworkRunner>();
            _runner.ProvideInput = _gameMode != Fusion.GameMode.Server;
            _runner.AddCallbacks(this);

            if (_pool == null)
                _pool = gameObject.AddComponent<BikeSimulatorNetworkPool>();

            string gameId = getSessionId();
            
            UnityEngine.Debug.Log($"Created gameobject {go.name} - starting game");
            _runner.StartGame(new StartGameArgs
            {
                GameMode = _gameMode,
                SessionName = gameId, //_gameMode == GameMode.Shared ? getLobbyName().ToString() : "test",
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = _simulatorSceneManager, //go.AddComponent<NetworkSceneManagerDefault>(),
                ObjectPool = _pool,
                DisableClientSessionCreation = true,
                PlayerCount = 20
                
            });
        }
        
        private string getSessionId()
        {
            //implement custom session id during runtime
            return SessionId;
        }
        
        private void SetConnectionStatus(ConnectionStatus status)
        {
            UnityEngine.Debug.Log($"Setting connection status to {status}");

            ConnectionStatus = status;

            if (!Application.isPlaying)
                return;

            if (status == ConnectionStatus.Disconnected || status == ConnectionStatus.Failed)
            {
                //OnShutdown(null,ShutdownReason.ConnectionTimeout);
                //LOAD LOBBY
                //SceneManager.LoadScene(LevelManager.LOBBY_SCENE);
                //UIScreen.BackToInitial();
            }
        }
        
        public void LeaveSession(ShutdownReason shutdownReason = ShutdownReason.Ok)
        {
            if (_runner != null)
                _runner.Shutdown(true,shutdownReason);
            else
                SetConnectionStatus(ConnectionStatus.Disconnected);
        }

        #region Net-Callbacks
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!_gameController && (runner.IsServer || runner.IsSharedModeMasterClient))
            {
                _gameController = runner.Spawn(GlobalSettings.Instance.GameControllerPrefab, Vector3.zero, Quaternion.identity, player);
            }

            VehicleRider vehicleRider = null;
            if ((runner.Topology == SimulationConfig.Topologies.Shared && runner.LocalPlayer == player) ||
                runner.IsServer)
            {
                vehicleRider = spawnPlayer(runner, player);
                PlayerManager.AddPlayer(vehicleRider);
            }
            
            
            Debug.Log($"Player {player} Joined Scene: {_runner.CurrentScene}!", runner);
            SetConnectionStatus(ConnectionStatus.Connected);
        }
        
        private VehicleRider spawnPlayer(NetworkRunner runner, PlayerRef? player)
        {
            return runner.Spawn(GlobalSettings.Instance._vehicleRiderPrefab, Vector3.zero, Quaternion.identity, player);
        }


        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"{player.PlayerId} disconnected.", runner);


            RemovePlayer(runner, player);

            SetConnectionStatus(ConnectionStatus);
        }

        private void RemovePlayer(NetworkRunner runner, PlayerRef player)
        {
            if (_gameMode == GameMode.Host || runner.IsSharedModeMasterClient)
            {
                var rider = PlayerManager.Get(player);
                if (rider != null)
                {
                    if (rider.Object.HasStateAuthority)
                    {
                        despawnBikeRider(runner, rider);
                    }

                    PlayerManager.RemovePlayer(rider);
                }
            }
        }

        private void despawnBikeRider(NetworkRunner runner, VehicleRider vehicleRider)
        {
            UnityEngine.Debug.Log($"PlayerRemoved from inside GameLauncher.despawnBuPlayer");
            Runner.Despawn(vehicleRider.Object);
            Debug.Log($"{vehicleRider}.name}} removed from GameController and despawned.", runner);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            UnityEngine.Debug.Log($"Connected to server.");
            SetConnectionStatus(ConnectionStatus.Connected);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            UnityEngine.Debug.Log("Disconnected from server");
            LeaveSession();
            SetConnectionStatus(ConnectionStatus.Disconnected);
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            if (IsDirectStart)
            {
                request.Accept();
                return;
            }
            if (runner.CurrentScene != BikeSimulatorSceneManager.LOBBY_SCENE)
            {
                UnityEngine.Debug.LogWarning($"Refused connection requested by {request.RemoteAddress}. Make sure the scene is added to the build settings and that IsDirectStart is checked if the StartUp scene is not the LobbyScene(BuildIndex: 0)");
                request.Refuse();
            }
            else
                request.Accept();
        }

        

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            UnityEngine.Debug.Log($"Connect failed {reason}");
            ShutdownReason shutdownReason = ShutdownReason.Ok;
            switch (reason)
            {
                case NetConnectFailedReason.Timeout:
                    shutdownReason = ShutdownReason.ConnectionTimeout;
                    break;
                case NetConnectFailedReason.ServerFull:
                    shutdownReason = ShutdownReason.GameIsFull;
                    break;
                case NetConnectFailedReason.ServerRefused:
                    shutdownReason = ShutdownReason.ConnectionRefused;
                    break;
            }
            LeaveSession(shutdownReason);
            SetConnectionStatus(ConnectionStatus.Failed);
            //string message = ConnectFailedReasonToHuman(reason);
            //_disconnectUI.ShowMessage(status,message);
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
            
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            
        }
        
        #endregion
        
        public void ShutDown()
        {
            StartCoroutine(closeGame());
        }
	    
        IEnumerator closeGame()
        {
            // while (LoadingController.Instance.Fadeing)
            // {
            //     yield return null;
            // }
            yield return null;
            if (Runner)
            {
                Runner.Shutdown(false, ShutdownReason.GameClosed);
            }
        }
    }
}