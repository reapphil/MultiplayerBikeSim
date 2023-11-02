using System.Collections;
using System.Collections.Generic;
using Common;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class BikeSceneManager: NetworkSceneManagerBase
	{
		public const int LAUNCH_SCENE = 0;
		public const int LOBBY_SCENE = 0;
		
		public static BikeSceneManager Instance { get; private set; }
		public bool IsChangingScene { get; set; }
		
		//public LoadingController LoadingController { get; private set; }

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(this);
			}
		}

		public static void LoadMenu()
		{
			Instance.Runner.SetActiveScene(LOBBY_SCENE);
		}

		public static void LoadLevel(int sceneIndex)
		{
			Instance.Runner.SetActiveScene(sceneIndex);
		}

		protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
		{
			UnityEngine.Debug.Log($"Loading scene {newScene}");

			// while (!LoadingController)
			// {
			// 	LoadingController = Extensions.FindLoadingController();
			// 	yield return null;
			// }
			
			//LoadingController.DoFadeIn();
			
			// while (LoadingController.Fading)
			// {
			// 	yield return null;
			// }
			
			PreLoadScene(newScene);

			List<NetworkObject> sceneObjects = new List<NetworkObject>();

			if (newScene != SceneManager.GetActiveScene().buildIndex)
			{
				AsyncOperation op = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
				while (!op.isDone)
				{
					yield return null;
				}

				if (prevScene == default)
				{
					op = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());	
				}
				else
				{
					op = SceneManager.UnloadSceneAsync(prevScene);
				}

				while (!op.isDone)
				{
					yield return null;
				}
				Scene loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
				SceneManager.SetActiveScene(loadedScene);
				UnityEngine.Debug.Log($"Loaded scene {newScene}: {loadedScene}");
				sceneObjects = FindNetworkObjects(loadedScene, disable: false);
			}
			else
			{
				Scene loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
				UnityEngine.Debug.Log($"Loaded scene {newScene}: {loadedScene}");
				sceneObjects = FindNetworkObjects(loadedScene, disable: false);
			}

			finished(sceneObjects);

			// Delay one frame, so we're sure level objects has spawned locally
			yield return null;
			
			PostLoadScene();
			StartCoroutine(postSwitchExecution());

		}

		IEnumerator postSwitchExecution()
		{
			yield return new WaitForEndOfFrame();
			{
				IsChangingScene = false;
				resetGameController();
			}
		}

		void resetGameController()
		{
			if(GameController.Instance)
			{
				GameController.Instance.ResetGameController();
			}
		}

		private void PreLoadScene(int scene)
		{
			IsChangingScene = true;
			if (scene != LOBBY_SCENE)
			{
				// Show an empty dummy UI screen - this will stay on during the game so that the game has a place in the navigation stack. Without this, Back() will break
				UnityEngine.Debug.Log("Showing Dummy");
			}
			else if(scene == LOBBY_SCENE)
			{

			}
			else
			{
				
			}
			// fader.gameObject.SetActive(true);
			// fader.FadeIn();
		}
	
		private void PostLoadScene()
		{
			//fader.FadeOut();
		}

		public void AddLoadingScene()
		{
			loadLoadingScene();
		}
        
		void loadLoadingScene()
		{
			SceneManager.LoadScene("Scenes/LoadingScene", LoadSceneMode.Additive);
			//LoadingController = Extensions.FindLoadingController();
		}

		// public void SetLoadingController(LoadingController loadingController)
		// {
		// 	LoadingController = loadingController;
		// }
    }
}