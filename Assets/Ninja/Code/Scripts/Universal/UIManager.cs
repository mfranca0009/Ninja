using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// TODO: pausing the game via Time.timeScale not working anymore.

public class UIManager : MonoBehaviour
{
	// UI Gameobjects / Canvases
	private GameObject[] _pauseObjects;
	private GameObject[] _finishObjects;
	public Canvas mainMenuCanvas;
	public Canvas settingsCanvas;
	public Canvas soundSettingsCanvas;
	public Canvas scrollCanvas;
	public Canvas healthCanvas;

	// Sound Settings 
	private Dictionary<string, Slider> _slidersChanged;
	private float[] _currSoundSettings;
	private float[] _soundSettingChanges;
	private bool _hasAppliedSoundSettings;

	// Scene
	private Scene _currentScene;
	private bool _paused;
	
	// UI States
	private bool _pauseShown;
	
	// Scripts
	private Health _playerHealth;
	private SoundManager _soundManager;

	private void Start()
	{
		_soundManager = FindObjectOfType<SoundManager>();
		_currSoundSettings = new float[(int)AudioMixerGroup.Max];

		// default to max volume for current sound settings.
		Array.Fill(_currSoundSettings, 1f);

		_soundSettingChanges = new float[(int)AudioMixerGroup.Max];
		_slidersChanged = new Dictionary<string, Slider>();
		
		Time.timeScale = 1f;
		
		// gets all objects with tag ShowOnPause
		_pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
		// gets all objects with tag ShowOnFinish
		_finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
		//scrollCanvas = GameObject.FindGameObjectWithTag("ScrollCanvas");

		ShowSettingsUI(false);
		ShowSoundSettingsUI(false);
		ShowPauseUI(false);
		ShowFinishedUI(false);
		ShowScrollUI(false);

		//Checks to make sure MainLevel is the loaded level
		Scene currentScene = SceneManager.GetActiveScene();
		if (currentScene.name == "_Main") {
			// TODO turn on once we determine the bool for when the player is dead/alive
			_playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
			//playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}
	}

	// Update is called once per frame
	private void Update()
	{
		_currentScene = SceneManager.GetActiveScene();

		ShowMainMenuUI(HasBuildIndex(_currentScene, 0));

		//uses the p button to pause and unpause the game
		if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) && !HasBuildIndex(_currentScene, 0))
			ShowPauseUI(!_pauseShown);

		//shows finish gameobjects if player is dead and timescale = 0 (DO NOT REMOVE THIS LINE)
		//if (Time.timeScale == 0 && playerController.alive == false)
		// if (Time.timeScale == 0f && _playerHealth.Dead)
		// {
		// 	showFinished();
		// }
		ShowFinishedUI(_playerHealth.Dead);
		ShowHealthUI(!HasBuildIndex(_currentScene, 0));
	}


	//controls the pausing of the scene
	public void PauseControl()
	{
		if (Time.timeScale == 1f)
		{
			Time.timeScale = 0f;
			ShowPauseUI(true);
			_paused = true;
		}
		else if (Time.timeScale == 0f)
		{
			Time.timeScale = 1f;
			ShowPauseUI(false);
			_paused = false;
		}
	}

	/// <summary>
	/// Show/hide main menu UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowMainMenuUI(bool show)
	{
		mainMenuCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Show/hide pause menu UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowPauseUI(bool show)
	{
		if (_playerHealth && _playerHealth.Dead && show)
			return;
		
		Time.timeScale = show ? 0f : 1f;
		_paused = show;

		foreach (GameObject g in _pauseObjects)
			g.SetActive(show);

		_pauseShown = show;
	}

	/// <summary>
	/// Show/hide settings menu UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowSettingsUI(bool show)
	{
		settingsCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Show/hide sound settings UI.<br></br><br></br>
	/// Note: When sound settings menu is hidden but sound settings were not applied,
	/// it will restore the current sound levels.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowSoundSettingsUI(bool show)
	{
		if (!show)
		{
			if (!_hasAppliedSoundSettings && _slidersChanged.Count > 0)
			{
				// reset changes that were never applied
				for (int i = 0; i < (int)AudioMixerGroup.Max; i++)
					_soundSettingChanges[i] = _currSoundSettings[i];

				// Reset sliders to show appropriate volume levels
				if (_slidersChanged.TryGetValue("MasterVol", out Slider slider))
					slider.value = _currSoundSettings[(int)AudioMixerGroup.Master];

				if(_slidersChanged.TryGetValue("SFXVol", out slider))
					slider.value = _currSoundSettings[(int)AudioMixerGroup.SoundEffects];

				if (_slidersChanged.TryGetValue("BGMusicVol", out slider))
					slider.value = _currSoundSettings[(int)AudioMixerGroup.BgMusic];
			
				_slidersChanged.Clear();
			}
			
			_hasAppliedSoundSettings = false;
			
			Button applyBtn = soundSettingsCanvas.transform.Find("btn_apply").GetComponent<Button>();
			if (applyBtn && applyBtn.interactable)
				applyBtn.interactable = false;
		}
		
		soundSettingsCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Apply sound settings that were changed<br></br><br></br>
	/// Used during the sound settings menu button press for the `Apply` button on the `OnClick` hook.
	/// </summary>
	public void ApplySoundSettings()
	{
		_hasAppliedSoundSettings = true;
		
		foreach (KeyValuePair<string, Slider> pair in _slidersChanged)
		{
			switch (pair.Key)
			{
				case "MasterVol":
					_soundManager.MainAudioMixer.SetFloat("MasterVol",
						Mathf.Log10(_soundSettingChanges[(int)AudioMixerGroup.Master]) * 20);

					_currSoundSettings[(int)AudioMixerGroup.Master] = 
						_soundSettingChanges[(int)AudioMixerGroup.Master];
					break;
				case "SFXVol":
					_soundManager.MainAudioMixer.SetFloat("SFXVol",
						Mathf.Log10(_soundSettingChanges[(int)AudioMixerGroup.SoundEffects]) * 20);

					_currSoundSettings[(int)AudioMixerGroup.SoundEffects] =
						_soundSettingChanges[(int)AudioMixerGroup.SoundEffects];
					break;
				case "BGMusicVol":
					_soundManager.MainAudioMixer.SetFloat("BGMusicVol",
						Mathf.Log10(_soundSettingChanges[(int)AudioMixerGroup.BgMusic]) * 20);

					_currSoundSettings[(int)AudioMixerGroup.BgMusic] =
						_soundSettingChanges[(int)AudioMixerGroup.BgMusic];
					break;
			}
		}

		_slidersChanged.Clear();
		
		Button applyBtn = soundSettingsCanvas.transform.Find("btn_apply").GetComponent<Button>();
		if (applyBtn && applyBtn.interactable)
			applyBtn.interactable = false;
	}

	/// <summary>
	/// Triggered when a sound slider is changed within the sound settings menu<br></br><br></br>
	/// Invoked through the `OnSliderChanged` hook.
	/// </summary>
	/// <param name="slider">The slider that has invoked this method</param>
	public void OnSoundSliderChanged(Slider slider)
	{
		string volumeParam = string.Empty;
		
		switch (slider.name)
		{
			case "MasterVolumeSlider":
				_soundSettingChanges[(int)AudioMixerGroup.Master] = slider.value;
				volumeParam = "MasterVol";
				break;
			case "SFXVolumeSlider":
				_soundSettingChanges[(int)AudioMixerGroup.SoundEffects] = slider.value;
				volumeParam = "SFXVol";
				break;
			case "BGMusicVolumeSlider":
				_soundSettingChanges[(int)AudioMixerGroup.BgMusic] = slider.value;
				volumeParam = "BGMusicVol";
				break;
		}

		if (volumeParam != string.Empty && !_slidersChanged.ContainsKey(volumeParam))
			_slidersChanged.Add(volumeParam, slider);
		
		Button applyBtn = soundSettingsCanvas.transform.Find("btn_apply").GetComponent<Button>();
		
		if (applyBtn && !applyBtn.interactable)
			applyBtn.interactable = true;
	}

	public void ShowHealthUI(bool show)
	{
		healthCanvas.gameObject.SetActive(show);
	}

	public void ShowScrollUI(bool show)
	{
		//foreach (GameObject g in scrollCanvas)
		//{
			//scrollCanvas.SetActive(true);
			scrollCanvas.gameObject.SetActive(show);
		//}
	}

	public void ShowFinishedUI(bool show)
	{
		if (!_paused)
			Time.timeScale = show ? 0f : 1f;
		
		foreach (GameObject g in _finishObjects)
		{
			g.SetActive(show);
		}
	}

	///
	/// SCENE MANAGEMENT CODE
	///


	public void LoadSceneByString(string sceneString)
	{
		Debug.Log($"sceneName to load: {sceneString}");
		SceneManager.LoadScene(sceneString, LoadSceneMode.Single);
	}

	public void LoadSceneByIndex(int sceneNumber)
	{
		Debug.Log($"sceneBuildIndex to load: {sceneNumber}");
		SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
	}

	public void GetActiveScene()
	{
		Scene currentScene = SceneManager.GetActiveScene();
		Debug.Log(currentScene.name);
		Debug.Log(currentScene.buildIndex);
	}

	//Reloads the Level
	public void ReloadCurrentScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public bool HasBuildIndex(Scene scene, params int[] buildIndices)
	{
		foreach (int buildIndex in buildIndices)
			if (scene.buildIndex == buildIndex)
				return true;

		return false;
	}
	
	public void QuitGame()
	{
		Application.Quit();
	}

}
