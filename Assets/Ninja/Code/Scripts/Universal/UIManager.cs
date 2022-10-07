using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private GameObject[] pauseObjects;
	private GameObject[] finishObjects;
	private GameObject[] settingsObjects;
	private GameObject[] soundSettingsObjects;
	public Canvas scrollCanvas;

	// sound setting changes
	private Dictionary<string, Slider> _slidersChanged;
	private float[] _currSoundSettings;
	private float[] _soundSettingChanges;
	private bool _hasAppliedSoundSettings;
	
	// Scripts
	private Health _playerHealth;
	private SoundManager _soundManager;


	private void Awake()
	{
		_soundManager = FindObjectOfType<SoundManager>();
		_currSoundSettings = new float[(int)AudioMixerGroup.Max];

		// default to max volume for current sound settings.
		Array.Fill(_currSoundSettings, 1f);

		_soundSettingChanges = new float[(int)AudioMixerGroup.Max];
		_slidersChanged = new Dictionary<string, Slider>();
		
		// gets all objects with tag ShowOnSettings
		settingsObjects = GameObject.FindGameObjectsWithTag("ShowOnSettings");
		// gets all objects with tag ShowOnSoundSettings
		soundSettingsObjects = GameObject.FindGameObjectsWithTag("ShowOnSoundSettings");
	}

	private void Start()
	{
		Time.timeScale = 1;
		
		// gets all objects with tag ShowOnPause
		 pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
		// gets all objects with tag ShowOnFinish
		finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
		//scrollCanvas = GameObject.FindGameObjectWithTag("ScrollCanvas");

		hideSettings();
		hideSoundSettings(true);
		hidePaused();
		hideFinished();
		hideScroll();

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
		Scene currentScene = SceneManager.GetActiveScene();
		//uses the p button to pause and unpause the game
		if ( (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) && currentScene.buildIndex != 0)
		{
			//if (Time.timeScale == 1 && playerController.alive == true) //&& !_playerHealth.Dead
			if (Time.timeScale == 1 && !_playerHealth.Dead)
			{
				Time.timeScale = 0;
				showPaused();
			}
			//else if (Time.timeScale == 0 && playerController.alive == true) //&& !_playerHealth.Dead
			else if (Time.timeScale == 0 && !_playerHealth.Dead)
			{
				Time.timeScale = 1;
				hidePaused();
			}
		}

		//shows finish gameobjects if player is dead and timescale = 0 (DO NOT REMOVE THIS LINE)
		//if (Time.timeScale == 0 && playerController.alive == false)
		if (Time.timeScale == 0 && _playerHealth.Dead)
		{
			showFinished();
		}
	}


	//controls the pausing of the scene
	public void pauseControl()
	{
		if (Time.timeScale == 1)
		{
			Time.timeScale = 0;
			showPaused();
		}
		else if (Time.timeScale == 0)
		{
			Time.timeScale = 1;
			hidePaused();
		}
	}

	//shows objects with ShowOnPause tag
	public void showPaused()
	{
		foreach (GameObject g in pauseObjects)
		{
			g.SetActive(true);
		}
	}

	//hides objects with ShowOnPause tag
	public void hidePaused()
	{
		foreach (GameObject g in pauseObjects)
		{
			g.SetActive(false);
		}
	}

	/// <summary>
	/// Show settings menu<br></br><br></br>
	/// Used during main menu button press and pause menu button press for the `Settings` button on the `OnClick` hook.
	/// </summary>
	public void showSettings()
	{
		foreach (GameObject g in settingsObjects)
			g.SetActive(true);
	}

	/// <summary>
	/// Hide settings menu<br></br><br></br>
	/// Used during settings menu button press for `Sound` and `Back` on the `OnClick` hook.
	/// </summary>
	public void hideSettings()
	{
		foreach (GameObject g in settingsObjects)
			g.SetActive(false);
	}

	/// <summary>
	/// Show sound settings menu<br></br><br></br>
	/// Used during the settings menu button press for the `Sound` button on the `OnClick` hook.
	/// </summary>
	public void showSoundSettings()
	{
		foreach (GameObject g in settingsObjects)
			g.SetActive(false);

		foreach (GameObject g in soundSettingsObjects)
			g.SetActive(true);
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
		
		Button applyBtn = soundSettingsObjects[0].transform.Find("btn_apply").GetComponent<Button>();
		
		if (applyBtn && !applyBtn.interactable)
			applyBtn.interactable = true;
	}
	
	/// <summary>
	/// Apply sound settings that were changed<br></br><br></br>
	/// Used during the sound settings menu button press for the `Apply` button on the `OnClick` hook.
	/// </summary>
	public void applySoundSettings()
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
		
		Button applyBtn = soundSettingsObjects[0].transform.Find("btn_apply").GetComponent<Button>();
		if (applyBtn && applyBtn.interactable)
			applyBtn.interactable = false;
	}
	
	/// <summary>
	/// Hide sound settings menu<br></br><br></br>
	/// Note: used during sound settings menu button press for `back` on the `OnClick` hook.
	/// </summary>
	/// <param name="sceneStart">Whether this method is being called on start of first frame</param>
	public void hideSoundSettings(bool sceneStart)
	{
		if (!_hasAppliedSoundSettings && _slidersChanged.Count > 0 && !sceneStart)
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

		Button applyBtn = soundSettingsObjects[0].transform.Find("btn_apply").GetComponent<Button>();
		if (applyBtn && applyBtn.interactable)
			applyBtn.interactable = false;

		foreach(GameObject g in soundSettingsObjects)
			g.SetActive(false);

		if (sceneStart)
			return;

		foreach (GameObject g in settingsObjects)
			g.SetActive(true);
	}
	
	public void showScroll()
	{
		//foreach (GameObject g in scrollCanvas)
		//{
			//scrollCanvas.SetActive(true);
			scrollCanvas.gameObject.SetActive(true);
		//}
	}

	public void hideScroll()
	{
		//foreach (GameObject g in scrollCanvas)
		//{
			//scrollCanvas.SetActive(false);
			scrollCanvas.gameObject.SetActive(false);
		//}
	}

	//shows objects with ShowOnFinish tag
	public void showFinished()
	{
		foreach (GameObject g in finishObjects)
		{
			g.SetActive(true);
		}
	}

	//hides objects with ShowOnFinish tag
	public void hideFinished()
	{
		foreach (GameObject g in finishObjects)
		{
			g.SetActive(false);
		}
	}


	///
	/// SCENE MANAGEMENT CODE
	///


	public void LoadSceneByString(string sceneString)
	{
		SceneManager.LoadScene(sceneString);
		Debug.Log("sceneName to load: " + sceneString);
	}

	public void LoadSceneByIndex(int sceneNumber)
	{
		SceneManager.LoadScene(sceneNumber);
		Debug.Log("sceneBuildIndex to load: " + sceneNumber);
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

}
