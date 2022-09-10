using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
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
}
