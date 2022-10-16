using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// TODO: Move keybind related input to the `PlayerInputAction` asset.

#region Helper Classes

[Serializable]
public class ScrollEntry
{
	public string levelName;
	public string scrollMessage;
}

#endregion

public class UIManager : MonoBehaviour
{
	#region Public Fields
	
	// UI Gameobjects / Canvases
	public Canvas pauseCanvas;
	public Canvas gameOverCanvas;
	public Canvas mainMenuCanvas;
	public Canvas settingsCanvas;
	public Canvas soundSettingsCanvas;
	public Canvas achievementsCanvas;
	public Canvas achievementsPopCanvas;
	public Canvas scrollCanvas;
	public Canvas healthCanvas;
	public Canvas loadingScreen;
	public Image[] livesImages; 

	// Sprite prefabs
	public Sprite[] livesSprites;
	
	// End-of-level scroll messages
	public List<ScrollEntry> scrollEntries;

	// Achievement Manager
	public AchievementManager _achievementManager;
	
	#endregion

	#region Private Fields
	
	// Sound Settings 
	private Dictionary<string, Slider> _slidersChanged;
	private float[] _currSoundSettings;
	private float[] _soundSettingChanges;
	private bool _hasAppliedSoundSettings;

	// Scene Management
	private SceneManagement _sceneManagement;
	private Scene _currentScene;
	private bool _paused;

	// UI States
	private bool _pauseShown;

	// Scripts
	private Health _playerHealth;
	private SoundManager _soundManager;
	
	#endregion

	#region Unity Events

	private void Start()
	{
		_sceneManagement = FindObjectOfType<SceneManagement>();
		_soundManager = FindObjectOfType<SoundManager>();
		_achievementManager = FindObjectOfType<AchievementManager>();

		_currSoundSettings = new float[(int)AudioMixerGroup.Max];

		// default to max volume for current sound settings.
		Array.Fill(_currSoundSettings, 1f);

		_soundSettingChanges = new float[(int)AudioMixerGroup.Max];
		_slidersChanged = new Dictionary<string, Slider>();
		
		Time.timeScale = 1f;
		

		ShowSettingsUI(false);
		ShowSoundSettingsUI(false);

		ShowPauseUI(false);
		ShowFinishedUI(false);
		ShowScrollUI(false);
		
		// Settings
		ShowSettingsUI(false);
		ShowSoundSettingsUI(false);
		
		// Achievements
		ShowAchievementsUI(false);
		ShowAchievementsPopUI(false);	
	}

	// Update is called once per frame
	private void Update()
	{
		_currentScene = SceneManager.GetActiveScene();
		
		ShowMainMenuUI(_sceneManagement.HasBuildIndex(_currentScene, 0));
		ShowHealthUI(!_sceneManagement.HasBuildIndex(_currentScene, 0));

		//uses the p or escape button to pause and unpause the game
		if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) &&
		    !_sceneManagement.HasBuildIndex(_currentScene, 0) && !scrollCanvas.isActiveAndEnabled)
		{
			ShowPauseUI(!_pauseShown);

			// If the game is no longer paused, but the settings or sound settings menu was active as well,
			// hide the settings or sound settings menus at the same time.
			if (!_paused)
			{
				switch (settingsCanvas.gameObject.activeInHierarchy)
				{
					case true:
						ShowSettingsUI(false);
						break;
					case false when soundSettingsCanvas.gameObject.activeInHierarchy:
						ShowSoundSettingsUI(false);
						break;
				}
			}
		}
	}

	#endregion

	#region Public UI Methods

	public void showLoadingUI(bool show)
	{
		loadingScreen.gameObject.SetActive(show);
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
		pauseCanvas.gameObject.SetActive(show);
		_pauseShown = show;
	}

	/// <summary>
	/// Show/hide achievements UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowAchievementsUI(bool show)
	{
		achievementsCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Show/hide achievements pop-up UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowAchievementsPopUI(bool show)
	{
		achievementsPopCanvas.gameObject.SetActive(show);
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

	/// <summary>
	/// Show/hide the player's health UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowHealthUI(bool show)
	{
		healthCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Update the player's lives UI
	/// </summary>
	/// <param name="livesLeft">The remaining amount of lives the player has.</param>
	public void UpdateLivesUI(int livesLeft)
	{
		foreach (Image lifeImage in livesImages)
		{
			lifeImage.sprite = lifeImage.gameObject.name switch
			{
				"Live_3" => livesLeft < 3 ? livesSprites[0] : livesSprites[2],
				"Live_2" => livesLeft < 2 ? livesSprites[0] : livesSprites[2],
				"Live_1" => livesLeft < 1 ? livesSprites[0] : livesSprites[2],
				_ => lifeImage.sprite
			};
		}
	}
	
	/// <summary>
	/// Show/hide the end-of-level scroll UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowScrollUI(bool show)
	{
		if (!_paused)
			Time.timeScale = show ? 0f : 1f;

		scrollCanvas.gameObject.SetActive(show);

		if (!show)
			return;
		
		TextMeshProUGUI scrollText = scrollCanvas.transform.Find("scroll_text").GetComponent<TextMeshProUGUI>();
		
		if (!scrollText)
			return;

		scrollText.text = SelectScrollMessage(_currentScene);
		
		if (_currentScene.buildIndex != 4)
			return;
		
		// Update boss level button, so it does not say "Next Level".

		TextMeshProUGUI nextLevelBtnText =
			scrollCanvas.transform.Find("btn_next_level").GetComponentInChildren<TextMeshProUGUI>();

		if (!nextLevelBtnText)
			return;
		
		nextLevelBtnText.text = "Continue";
	}

	/// <summary>
	/// Show/hide the game over UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowFinishedUI(bool show)
	{
		if (!_paused)
			Time.timeScale = show ? 0f : 1f;
		
		gameOverCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Show/hide enemy health UI.
	/// </summary>
	/// <param name="enemyHealthUICanvas">The enemy health UI canvas.</param>
	/// <param name="show">Whether to show the UI or not.</param>
	public static void ShowEnemyHealthUI(Canvas enemyHealthUICanvas, bool show)
	{
		if (!enemyHealthUICanvas)
			return;
		
		enemyHealthUICanvas.gameObject.SetActive(show);
	}

	#endregion

	#region Private Helper Methods

	/// <summary>
	/// Select the appropriate scroll message based on the current scene.
	/// </summary>
	/// <param name="scene">The scene to be used to select the paired scroll message.</param>
	/// <returns>The scroll message selected.</returns>
	private string SelectScrollMessage(Scene scene)
	{
		if (scrollEntries.Count == 0)
			return "No scroll entries available!";
		
		ScrollEntry scrollEntry = scrollEntries.Find(sEntry => sEntry.levelName == scene.name);

		if (string.IsNullOrEmpty(scrollEntry.scrollMessage) || string.IsNullOrWhiteSpace(scrollEntry.scrollMessage))
			return "No message to show!";
		
		return scrollEntry.scrollMessage.Replace("\\n", "\n");
	}

	#endregion

	#region Scene Management [REMOVE] - use `SceneManagement` class.

	public void LoadSceneByString(string sceneString)
	{
		_achievementManager.ResetTimers();
		Debug.Log($"sceneName to load: {sceneString}");
		SceneManager.LoadScene(sceneString, LoadSceneMode.Single);
	}
	
	public void LoadSceneByIndex(int sceneNumber)
	{
		_achievementManager.ResetTimers();
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
	
	public void QuitGame()
	{
		Application.Quit();
	}

	#endregion
}
