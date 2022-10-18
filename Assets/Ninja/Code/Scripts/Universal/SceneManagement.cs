using System.Collections;
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

    /// <summary>
    /// Load a scene by scene name.
    /// </summary>
    /// <param name="sceneString">The scene's name to load.</param>
    public void LoadSceneByString(string sceneString)
    {
        Debug.Log("sceneName to load: " + sceneString);
        StartCoroutine(LoadSceneByStringAsync(sceneString));
    }

    /// <summary>
    /// Load a scene by build index.
    /// </summary>
    /// <param name="sceneNumber">The scene's build index to load.</param>
    public void LoadSceneByIndex(int sceneNumber)
    {
        Debug.Log("sceneBuildIndex to load: " + sceneNumber);
        StartCoroutine(LoadSceneByIndexAsync(sceneNumber));
    }

    /// <summary>
    /// Retrieve the active scene's debug information.
    /// </summary>
    public void GetActiveScene() 
    {
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log(currentScene.name);
        Debug.Log(currentScene.buildIndex);
    }

    /// <summary>
    /// Load the next scene from current scene.
    /// </summary>
    public void LoadNextScene() 
    {
        StartCoroutine(LoadNextSceneAsync());
    }

    /// <summary>
    /// Determine if the scene's build index matches any of the following build indices provided.
    /// </summary>
    /// <param name="scene">The scene to check.</param>
    /// <param name="buildIndices">The build indices to compare with the scene to check.</param>
    /// <returns>Returns true if the scene contains the build index or indices provides, otherwise false.</returns>
    public bool HasBuildIndex(Scene scene, params int[] buildIndices)
    {
        foreach (int buildIndex in buildIndices)
            if (scene.buildIndex == buildIndex)
                return true;

        return false;
    }
    
    /// <summary>
    /// Quit the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Load a scene by scene name asynchronously.
    /// </summary>
    /// <param name="sceneString">The name of the scene to load.</param>
    /// <returns>IEnumerator for use with starting a coroutine.</returns>
    private IEnumerator LoadSceneByStringAsync(string sceneString)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneString);

        if (_uiManager)
            _uiManager.ShowLoadingUI(true);
        
        while (!asyncOperation.isDone)
        {
            if (_uiManager)
                _uiManager.UpdateLoadingProgressUI(Mathf.Clamp01(asyncOperation.progress / 0.9f));

            yield return null;
        }
        
        if (_uiManager)
            _uiManager.ShowLoadingUI(false);
        
    }
    
    /// <summary>
    /// Load a scene by build index asynchronously.
    /// </summary>
    /// <param name="sceneNumber">The scene's build index to load.</param>
    /// <returns>IEnumerator for use with starting a coroutine.</returns>
    private IEnumerator LoadSceneByIndexAsync(int sceneNumber)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneNumber);

        if (_uiManager)
            _uiManager.ShowLoadingUI(true);
        
        while (!asyncOperation.isDone)
        {
            if (_uiManager)
                _uiManager.UpdateLoadingProgressUI(Mathf.Clamp01(asyncOperation.progress / 0.9f));

            yield return null;
        }
        
        if (_uiManager)
            _uiManager.ShowLoadingUI(false);
    }
    
    /// <summary>
    /// Load the next scene from current scene asynchronously
    /// </summary>
    /// <returns>IEnumerator for use with starting a coroutine.</returns>
    private IEnumerator LoadNextSceneAsync()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

        if (_uiManager)
            _uiManager.ShowLoadingUI(true);
        
        while (!asyncOperation.isDone)
        {
            if (_uiManager)
                _uiManager.UpdateLoadingProgressUI(Mathf.Clamp01(asyncOperation.progress / 0.9f));

            yield return null;
        }
        
        if (_uiManager)
            _uiManager.ShowLoadingUI(false);
    }
    
    #endregion
}
