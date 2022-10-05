using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
	GameObject[] pauseObjects;
	GameObject[] finishObjects;
	public Canvas scrollCanvas;
	public Canvas healthCanvas;

	//
	private Health _playerHealth;


	void Start()
	{
		Time.timeScale = 1;

		// gets all objects with tag ShowOnPause
		 pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
		//gets all objects with tag ShowOnFinish
		finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
		//scrollCanvas = GameObject.FindGameObjectWithTag("ScrollCanvas");

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
	void Update()
	{
		Scene currentScene = SceneManager.GetActiveScene();
		//uses the p button to pause and unpause the game
		if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) && currentScene.buildIndex != 0)
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

		if (currentScene.buildIndex == 0)
		{
			hideHealth();
		}
		else if (currentScene.buildIndex != 0)
		{
			showHealth();
		}
	}//close update


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

	//Health Canvas
	public void showHealth()
	{
		healthCanvas.gameObject.SetActive(true);
	}

	public void hideHealth()
	{
		healthCanvas.gameObject.SetActive(false);
	}


	//Scroll Canvas
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
