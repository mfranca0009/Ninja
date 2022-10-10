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

    public void LoadNextScene() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
