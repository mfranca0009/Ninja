using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{

    public UIManager _uIManager;
    public AchievementManager _achievementManager;

    #region Public Helper Methods

    private void Start()
    {
        _achievementManager = FindObjectOfType<AchievementManager>();
    }

    public void LoadSceneByString(string sceneString)
    {
        AchievementCleanUp(SceneManager.GetSceneByName(sceneString).buildIndex);
        SceneManager.LoadScene(sceneString);
        Debug.Log("sceneName to load: " + sceneString);
        _uIManager.showLoadingUI(false);
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

    private void AchievementCleanUp(int sceneNum)
    {
        _achievementManager.ResetTimers();

        if (sceneNum == 1)
        {
            _achievementManager.ResetCounters();
            _achievementManager.Achievements.Find(achi => achi.Title == "Martial Ninja").Eligible = true;
            _achievementManager.Achievements.Find(achi => achi.Title == "Distance Ninja").Eligible = true;
            _achievementManager.Achievements.Find(achi => achi.Title == "Expert Ninja").Eligible = true;
        }
    }

    #endregion
}
