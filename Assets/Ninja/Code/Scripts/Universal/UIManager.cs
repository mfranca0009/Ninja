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
	private float[] _currSoundSettings;
	private float[] _soundSettingChanges;
	
	// Scripts
	private Health _playerHealth;
	private SoundManager _soundManager;


	private void Awake()
	{
		_soundManager = FindObjectOfType<SoundManager>();
		_currSoundSettings = new float[6];
		_soundSettingChanges = new float[6];
		
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
	/// Used during main menu button press for the `Settings` button on the `OnClick` hook.
	/// </summary>
	public void showSettings()
	{
		foreach (GameObject g in settingsObjects)
			g.SetActive(true);
	}

	/// <summary>
	/// Hide settings menu
	/// </summary>
	public void hideSettings()
	{
		foreach (GameObject g in settingsObjects)
			g.SetActive(false);
	}

	/// <summary>
	/// Used during the settings menu button press for the `Sound` button on the `OnClick` hook.
	/// </summary>
	public void showSoundSettings()
	{
		foreach (GameObject g in settingsObjects)
			g.SetActive(false);

		foreach (GameObject g in soundSettingsObjects)
			g.SetActive(true);
	}

	public void OnSoundSliderChanged(Slider slider)
	{
		switch (slider.name)
		{
			case "MasterVolumeSlider":
				_soundSettingChanges[0] = Mathf.Log(slider.value) * 20;
				break;
			case "BGMusicVolumeSlider":
				_soundSettingChanges[1] = Mathf.Log(slider.value) * 20;
				break;
			case "MasterSFXVolumeSlider":
				_soundSettingChanges[2] = Mathf.Log(slider.value) * 20;
				break;
			case "AttackSFXVolumeSlider":
				_soundSettingChanges[3] = Mathf.Log(slider.value) * 20;
				break;
			case "DamageSFXVolumeSlider":
				_soundSettingChanges[4] = Mathf.Log(slider.value) * 20;
				break;
			case "ItemSFXVolumeSlider":
				_soundSettingChanges[5] = Mathf.Log(slider.value) * 20;
				break;
		}

		Button applyBtn = soundSettingsObjects[0].transform.Find("btn_apply").GetComponent<Button>();
		
		if (applyBtn && !applyBtn.interactable)
			applyBtn.interactable = true;
	}
	
	/// <summary>
	/// Used during the sound setings menu button press for the `Apply` button on the `OnClick` hook.
	/// </summary>
	public void applySoundSettings()
	{
		GameObject[] soundSliders = GameObject.FindGameObjectsWithTag("SoundSliders");

		foreach (GameObject gSlider in soundSliders)
		{
			Slider slider = gSlider.GetComponent<Slider>();

			Debug.Log(slider.value);
			switch (slider.name)
			{
				case "MasterVolumeSlider":
					if (_currSoundSettings[0] != _soundSettingChanges[0])
					{
						_soundManager.MainAudioMixer.SetFloat("MasterVol", _soundSettingChanges[0]);
						_currSoundSettings[0] = _soundSettingChanges[0];
					}
					break;
				case "BGMusicVolumeSlider":
					if (_currSoundSettings[1] != _soundSettingChanges[1])
					{
						_soundManager.MainAudioMixer.SetFloat("BGMusicVol", _soundSettingChanges[1]);
						_currSoundSettings[1] = _soundSettingChanges[1];
					}
					break;
				case "MasterSFXVolumeSlider":
					if (_currSoundSettings[2] != _soundSettingChanges[2])
					{
						_soundManager.MainAudioMixer.SetFloat("MasterSFXVol", _soundSettingChanges[2]);
						_currSoundSettings[2] = _soundSettingChanges[2];
					}
					break;
				case "AttackSFXVolumeSlider":
					if (_currSoundSettings[3] != _soundSettingChanges[3])
					{
						_soundManager.MainAudioMixer.SetFloat("AttackSFXVol", _soundSettingChanges[3]);
						_currSoundSettings[3] = _soundSettingChanges[3];
					}
					break;
				case "DamageSFXVolumeSlider":
					if (_currSoundSettings[4] != _soundSettingChanges[4])
					{
						_soundManager.MainAudioMixer.SetFloat("DamageSFXVol", _soundSettingChanges[4]);
						_currSoundSettings[4] = _soundSettingChanges[4];
					}
					break;
				case "ItemSFXVolumeSlider":
					if (_currSoundSettings[5] != _soundSettingChanges[5])
					{
						_soundManager.MainAudioMixer.SetFloat("ItemSFXVol", _soundSettingChanges[5]);
						_currSoundSettings[5] = _soundSettingChanges[5];
					}
					break;
			}
		}

		Button applyBtn = soundSettingsObjects[0].transform.Find("btn_apply").GetComponent<Button>();
		if (applyBtn && applyBtn.interactable)
			applyBtn.interactable = false;
	}
	
	/// <summary>
	/// Hide sound settings menu
	/// </summary>
	/// <param name="sceneStart">Whether this method is being called on start of first frame</param>
	public void hideSoundSettings(bool sceneStart)
	{
		for (int i = 0; i < _soundSettingChanges.Length; i++)
			_soundSettingChanges[i] = 0f;
		
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
