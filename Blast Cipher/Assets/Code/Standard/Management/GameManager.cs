﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public sealed class GameManager
{
	#region Singleton Implementation
	private GameManager() { }
	private static GameManager instance;
	public static GameManager Instance { get => instance ?? (instance = new GameManager()); }
	#endregion

	#region Properties
	private GameManagerBootstrapper _bootstrapper;
	private GameManagerBootstrapper bootstrapper;

	public readonly InputDevice[] inputDevices = new InputDevice[2];
	public bool playerInputsActive = true;
    public int maxRounds;

	private int roundCount;
	private bool nextRoundStarterInProgress = false;
	private Scene asyncEssentials;
	private Scene currentMainScene;
	#endregion

	public delegate void ExtendedUpdate();

	public readonly List<GameObject> temporaryObjects = new List<GameObject>();
	public readonly List<ExtendedUpdate> extendedUpdates = new List<ExtendedUpdate>();
	private readonly List<PlayerCharacter> registeredPlayerCharacters = new List<PlayerCharacter>(2);

	#region Mono Registrations
	internal void RegisterBootstrapper(GameManagerBootstrapper bootstrapper)
	{
		this.bootstrapper = bootstrapper;

		bootstrapper.PostProcessing.profile.TryGetSettings<Vignette>(out var vignette);
		Effects.Init(bootstrapper.PostProcessing);
	}
	internal void UnregisterBootstrapper() => bootstrapper = null;

	public int RegisterPlayerCharacter(PlayerCharacter playerCharacter)
	{
		registeredPlayerCharacters.Add(playerCharacter);
		return registeredPlayerCharacters.Count - 1;
	}
	public bool UnregisterPlayerCharacter(PlayerCharacter playerCharacter)
	{
		return registeredPlayerCharacters.Remove(playerCharacter);
	}
	#endregion

	#region Scene Loader
	//public void LoadScene(string sceneName) => bootstrapper.StartCoroutine(LoadSceneCo(SceneManager.GetSceneByName(sceneName).buildIndex));
	public void LoadScene(int buildIndex) => bootstrapper.StartCoroutine(LoadSceneCo(buildIndex));
	private IEnumerator LoadSceneCo(int buildIndex)
	{
		var token = new LoadingScreenHandler.LoadingScreenProgressToken();
		LoadingScreenHandler.ShowLoadingScreen(token);

		while(!token.ScreenFullyShown) { yield return null; }

		// unload all unwanted scenes
		Scene scene;
		List<int> scenesToUnload = new List<int>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			scene = SceneManager.GetSceneAt(i);

			if (scene.buildIndex == asyncEssentials.buildIndex) continue;
			else scenesToUnload.Add(scene.buildIndex);
		}

		// load new scene
		var operation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

		// wait until new scene is fully loaded
		while (!operation.isDone) yield return null;

		// unload old scenes
		for(int i = 0; i < scenesToUnload.Count; i++)
		{
			SceneManager.UnloadSceneAsync(scenesToUnload[i], UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		}

		// set new scene active after single frame delay
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
	}
	#endregion

	internal void TriggerExtendedUpdates()
	{
		for (int i = 0; i < extendedUpdates.Count; i++) extendedUpdates[i]();
	}

	internal void SetAsyncEssentialsScene(Scene scene)
	{
		asyncEssentials = scene;
	}

	public PlayerCharacter RequestNearestPlayer(PlayerCharacter requestSender)
	{
		if (registeredPlayerCharacters.Count < 2) return null;
		return registeredPlayerCharacters[registeredPlayerCharacters.IndexOf(requestSender) == 0 ? 1 : 0];
	}

	public GameObject SpawnObject (GameObject prefab)
	{
		var go = GameObject.Instantiate(prefab);
		temporaryObjects.Add(go);
		return go;
	}

	public void StartNextRound()
	{
		if (!nextRoundStarterInProgress)
		{
			if (roundCount >= maxRounds - 1)
			{
				BackToMenu();
				return;
			}

			nextRoundStarterInProgress = true;
			if (/*roundCount != 0 &&*/ roundCount % 2 == 0)
			{
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar, true);
			}
			else
			{
				MusicManager.Instance.RoundTransitionSmoother(OnNextMusicBar, false);
			}
			bootstrapper.StartCoroutine(TimeScalerOnRoundTransition());

			roundCount++;
		}
	}

    private void BackToMenu()
    {
        LoadScene(0);
        roundCount = 0;
        maxRounds = 0;
        inputDevices[0] = null;
        inputDevices[1] = null;
    }

	private IEnumerator TimeScalerOnRoundTransition()
	{
		float scaleModPerFrame = 1f;
		float fixedDeltaTime = Time.fixedDeltaTime;
		while (nextRoundStarterInProgress)
		{
			Time.timeScale -= scaleModPerFrame * Time.deltaTime;
			Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
			yield return new WaitForEndOfFrame();
		}
		Time.fixedDeltaTime = fixedDeltaTime;
		Time.timeScale = 1f;
	}

	private void OnNextMusicBar()
	{
		//LoadScene("Gameplay Proto");
		ResetLevel();

		playerInputsActive = true;
		nextRoundStarterInProgress = false;
	}

	private void ResetLevel()
	{
		for (int i = 0; i < temporaryObjects.Count; i++)
		{
			GameObject.Destroy(temporaryObjects[i]);
		}
		for (int i = 0; i < registeredPlayerCharacters.Count; i++)
		{
			registeredPlayerCharacters[i].Reset();
		}
	}
}