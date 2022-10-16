using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    #region Private Fields

    // UI Manager
    private UIManager _uiManager;
    
    // Achievement Manager
    private AchievementManager _achievementManager;
    
    #endregion
    
    #region Unity Events
    
    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
    }
    
    #endregion
    
    #region Public Helper Methods
    
    public void LoadSceneByString(string sceneString)
    {
        AchievementCleanUp(SceneManager.GetSceneByName(sceneString).buildIndex);
        SceneManager.LoadScene(sceneString);
        Debug.Log("sceneName to load: " + sceneString);
        _uiManager.showLoadingUI(false);
    }

    public void LoadSceneByIndex(int sceneNumber)
    {
        AchievementCleanUp(sceneNumber);
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

    #endregion
    
    #region Private Helper Methods
    
    private void AchievementCleanUp(int sceneNum)
    {
        _achievementManager.ResetTimers();

        if (sceneNum != 1) 
            return;
        
        _achievementManager.ResetCounters();
        _achievementManager.Achievements.Find(achievement => achievement.Title == "Martial Ninja").Eligible = true;
        _achievementManager.Achievements.Find(achievement => achievement.Title == "Distance Ninja").Eligible = true;
        _achievementManager.Achievements.Find(achievement => achievement.Title == "Expert Ninja").Eligible = true;
    }
    
    #endregion
}
