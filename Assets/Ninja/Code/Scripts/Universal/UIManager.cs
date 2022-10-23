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
	public Canvas introCanvas;
	public Animator introScrollAnimator;
	public Canvas transitionCanvas;
	public float transitionRate = 1f;
	public Canvas creditsCanvas;
	public Canvas settingsCanvas;
	public Canvas soundSettingsCanvas;
	public Canvas achievementsCanvas;
	public Canvas achievementsPopCanvas;
	public Canvas scrollCanvas;
	public Canvas healthCanvas;
	public Canvas loadingCanvas;
	public TMP_Text loadingCanvasText;
	public Image loadingCanvasProgress;
	public Canvas secretScrollCanvas;
	public Image[] livesImages;

	public float loadingTextUpdateDelay;
	
	// Sprite prefabs
	public Sprite[] livesSprites;

	// Scroll fragment gameobjects
	public GameObject[] scrollFragments;
	
	// End-of-level scroll messages
	public List<ScrollEntry> scrollEntries;
	
	#endregion

	#region Private Fields
	
	// UI elements
	private Image _transitionBackground;

	// Sound Settings 
	private Dictionary<string, Slider> _slidersChanged;
	private float[] _currSoundSettings;
	private float[] _soundSettingChanges;
	private bool _hasAppliedSoundSettings;

	// Scene Management
	private SceneManagement _sceneManagement;
	private Scene _currentScene;
	private bool _paused;

	// Achievement Manager
	private AchievementManager _achievementManager;

	// Game Manager
	private GameManager _gameManager;
	
	// UI States
	private bool _pauseShown;
	private bool _showTransitionBackground;
	private bool _loadLevelOneTrigger;

	// Timers
	private float _loadTextUpdateTimer;

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
		_gameManager = FindObjectOfType<GameManager>();
		_transitionBackground = transitionCanvas.GetComponentInChildren<Image>();

		_currSoundSettings = new float[(int)AudioMixerGroup.Max];

		// default to max volume for current sound settings.
		Array.Fill(_currSoundSettings, 1f);

		_soundSettingChanges = new float[(int)AudioMixerGroup.Max];
		_slidersChanged = new Dictionary<string, Slider>();
		
		Time.timeScale = 1f;

		_loadTextUpdateTimer = loadingTextUpdateDelay;

		// Note: no need to hide UI elements in Start() anymore. Since we have them referenced as public through
		// the editor we can have them hidden by default on the `PersistentObjects` prefab and not have to find them.
	}

	// Update is called once per frame
	private void Update()
	{
		_currentScene = SceneManager.GetActiveScene();

		// Load next scene from intro scene
		if (!_loadLevelOneTrigger && _transitionBackground.color.a >= 1f &&
		    _sceneManagement.HasBuildIndex(_currentScene, 1))
		{
			_loadLevelOneTrigger = true;
			_sceneManagement.LoadNextScene();
		}
		
		if (loadingCanvas.gameObject.activeInHierarchy)
			UpdateLoadingTextUI();

		ShowMainMenuUI(_sceneManagement.HasBuildIndex(_currentScene, 0));
		ShowIntroUI(_sceneManagement.HasBuildIndex(_currentScene, 1));
		ShowIntroScrollUI(_transitionBackground.color.a == 0f);
		ShowCreditsUI(_sceneManagement.HasBuildIndex(_currentScene, 6));
		ShowHealthUI(!_sceneManagement.HasBuildIndex(_currentScene, 0, 1, 6));
		ShowSecretScrollUI(!_sceneManagement.HasBuildIndex(_currentScene, 0, 1, 6));

		// Clean-ups specific to when the updated scene is level 1 when coming from the intro scene.
		if (_sceneManagement.HasBuildIndex(_currentScene, 2))
		{
			ShowTransitionBgUI(false);
			_loadLevelOneTrigger = false;
		}
		
		// Update transition background UI depending on the current alpha set.
		if (_showTransitionBackground && _transitionBackground.color.a < 255f ||
		    !_showTransitionBackground && _transitionBackground.color.a > 0f)
			UpdateTransitionBgUI();

		//uses the p or escape button to pause and unpause the game
		if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) &&
		    !_sceneManagement.HasBuildIndex(_currentScene, 0, 1, 6) && !scrollCanvas.isActiveAndEnabled)
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

	/// <summary>
	/// Show/hide loading UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowLoadingUI(bool show)
	{
		if (!show && !loadingCanvas.isActiveAndEnabled || show && loadingCanvas.isActiveAndEnabled)
			return;
		
		loadingCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Update loading text UI
	/// </summary>
	private void UpdateLoadingTextUI()
	{
		if (!loadingCanvasText)
			return;
		
		int dotCount = loadingCanvasText.text.Split('.').Length - 1;

		if (_loadTextUpdateTimer <= 0)
		{
			loadingCanvasText.text = dotCount switch
			{
				0 => "Loading.",
				1 => "Loading..",
				2 => "Loading...",
				3 => "Loading",
				_ => loadingCanvasText.text
			};
		}
		else
			_loadTextUpdateTimer -= Time.deltaTime;
	}

	/// <summary>
	/// Update loading progress bar UI
	/// </summary>
	/// <param name="progressAmount">The new progress amount to assign to the fill image.</param>
	public void UpdateLoadingProgressUI(float progressAmount)
	{
		if (!loadingCanvasProgress)
			return;
		
		loadingCanvasProgress.fillAmount = progressAmount;
	}
	
	/// <summary>
	/// Show/hide main menu UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	public void ShowMainMenuUI(bool show)
	{
		if (!show && !mainMenuCanvas.isActiveAndEnabled || show && mainMenuCanvas.isActiveAndEnabled)
			return;
		
		mainMenuCanvas.gameObject.SetActive(show);
	}
	
	/// <summary>
	/// Show/hide introduction UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	private void ShowIntroUI(bool show)
	{
		if (!show && !introCanvas.isActiveAndEnabled || show && introCanvas.isActiveAndEnabled)
			return;

		introCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Play the show or hide animation for the introduction scroll UI
	/// </summary>
	/// <param name="show"></param>
	private void ShowIntroScrollUI(bool show)
	{
		if (!introCanvas.isActiveAndEnabled ||
		    !show && introScrollAnimator.IsPlayingAnimation("Hide", (int)AnimationLayers.BaseAnimLayer) ||
		    show && introScrollAnimator.IsPlayingAnimation("Show", (int)AnimationLayers.BaseAnimLayer))
			return;
		
		introScrollAnimator.SetTrigger(show ? "Show" : "Hide");
	}
	
	/// <summary>
	/// Show/hide transition UI
	/// </summary>
	public void ShowTransitionUI(bool show)
	{
		if (!show && !transitionCanvas.isActiveAndEnabled || show && transitionCanvas.isActiveAndEnabled)
			return;

		transitionCanvas.gameObject.SetActive(show);
	}
	
	/// <summary>
	/// Show/hide transition Background UI
	/// </summary>
	public void ShowTransitionBgUI(bool show)
	{
		if (!show && !_showTransitionBackground || show && _showTransitionBackground)
			return;
		
		_showTransitionBackground = show;
	}

	/// <summary>
	/// Update the transition Background UI's color to smoothly change the alpha to visible or not visible.
	/// </summary>
	private void UpdateTransitionBgUI()
	{
		float currentAlpha = _transitionBackground.color.a;
		_transitionBackground.color = _showTransitionBackground
			? new Color(0, 0, 0, Mathf.MoveTowards(currentAlpha, 255f, transitionRate * Time.deltaTime))
			: new Color(0, 0, 0, Mathf.MoveTowards(currentAlpha, 0f, transitionRate * Time.deltaTime));
	}
	
	/// <summary>
	/// Show/hide credits UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	private void ShowCreditsUI(bool show)
	{
		if (!show && !creditsCanvas.isActiveAndEnabled || show && creditsCanvas.isActiveAndEnabled)
			return;
		
		creditsCanvas.gameObject.SetActive(show);
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
		if (!show && !achievementsCanvas.isActiveAndEnabled || show && achievementsCanvas.isActiveAndEnabled)
			return;
		
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
		if (!show && !settingsCanvas.isActiveAndEnabled || show && settingsCanvas.isActiveAndEnabled)
			return;
		
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
					_soundManager.mainAudioMixer.SetFloat("MasterVol",
						Mathf.Log10(_soundSettingChanges[(int)AudioMixerGroup.Master]) * 20);

					_currSoundSettings[(int)AudioMixerGroup.Master] = 
						_soundSettingChanges[(int)AudioMixerGroup.Master];
					break;
				case "SFXVol":
					_soundManager.mainAudioMixer.SetFloat("SFXVol",
						Mathf.Log10(_soundSettingChanges[(int)AudioMixerGroup.SoundEffects]) * 20);

					_currSoundSettings[(int)AudioMixerGroup.SoundEffects] =
						_soundSettingChanges[(int)AudioMixerGroup.SoundEffects];
					break;
				case "BGMusicVol":
					_soundManager.mainAudioMixer.SetFloat("BGMusicVol",
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
	private void ShowHealthUI(bool show)
	{
		if (!show && !healthCanvas.isActiveAndEnabled || show && healthCanvas.isActiveAndEnabled)
			return;
		
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
	/// Show/hide the Secret Scroll UI.
	/// </summary>
	/// <param name="show">Whether to show the UI or not.</param>
	private void ShowSecretScrollUI(bool show)
    {
	    if (!show && !secretScrollCanvas.isActiveAndEnabled || show && secretScrollCanvas.isActiveAndEnabled)
		    return;
	    
		secretScrollCanvas.gameObject.SetActive(show);
	}

	/// <summary>
	/// Update secret scroll UI depending on how many were collected.
	/// </summary>
	public void UpdateSecretScrollUI()
	{
		if (!_gameManager)
			return;

		bool allFragmentsObtained = _gameManager.ObtainedScrollFragmentStates[0] &&
		                            _gameManager.ObtainedScrollFragmentStates[1] &&
		                            _gameManager.ObtainedScrollFragmentStates[2];

		scrollFragments[0].SetActive(allFragmentsObtained);

		for (int i = 1; i < scrollFragments.Length; i++)
			scrollFragments[i].SetActive(!allFragmentsObtained
				? _gameManager.ObtainedScrollFragmentStates[i - 1]
				: !_gameManager.ObtainedScrollFragmentStates[i - 1]);
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
		
		ScrollEntry[] result = scrollEntries.FindAll(sEntry => sEntry.levelName == scene.name).ToArray();

		if (result.Length == 0)
			return "No scroll entry found for this level!";

		bool allFragmentsObtained = _gameManager.ObtainedScrollFragmentStates[0] &&
		                            _gameManager.ObtainedScrollFragmentStates[1] &&
		                            _gameManager.ObtainedScrollFragmentStates[2];

		ScrollEntry scrollEntry = result.Length == 1 ? result[0] : allFragmentsObtained ? result[1] : result[0];

		if (string.IsNullOrEmpty(scrollEntry.scrollMessage) || string.IsNullOrWhiteSpace(scrollEntry.scrollMessage))
			return "No message to show!";
		
		return scrollEntry.scrollMessage.Replace("\\n", "\n");
	}

	#endregion
}
