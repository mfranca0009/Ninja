using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    #region Private Fields

    // UI Manager
    private UIManager _uiManager;
    
    #endregion
    
    #region Unity Events
    
    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
    }
    
    #endregion
    
    #region Public Helper Methods
    
    public void LoadSceneByString(string sceneString)
    {
        SceneManager.LoadScene(sceneString);
        Debug.Log("sceneName to load: " + sceneString);

        if (!_uiManager)
            _uiManager.ShowLoadingUI(false);
    }

    public void LoadSceneByIndex(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
        Debug.Log("sceneBuildIndex to load: " + sceneNumber);

        if (!_uiManager)
            _uiManager.ShowLoadingUI(false);
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
        
        if (!_uiManager)
            _uiManager.ShowLoadingUI(false);
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

    #endregion
}
