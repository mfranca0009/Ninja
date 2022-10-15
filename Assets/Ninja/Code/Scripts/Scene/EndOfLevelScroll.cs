using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelScroll : MonoBehaviour
{
    #region Public Fields
    
    // UNUSED - maybe in the future?
    // public string levelToLoad;

    #endregion

    #region Private Fields

    // UI Manager
    private UIManager _uiManager;
    private AchievementManager _achievementManager;
    private SceneManagement _sceneManagement;
    #endregion

    #region Unity Events

    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
        _sceneManagement = FindObjectOfType<SceneManagement>();


        if (!_uiManager)
            return;
        
        _uiManager.ShowScrollUI(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !_uiManager)
            return;
        
        _uiManager.ShowScrollUI(true);


        if (!_achievementManager)
            return;

        int sceneNum = SceneManager.GetActiveScene().buildIndex;
        bool goodEndingTrigger = false;

        if (sceneNum == 4)
        {
            CounterAchievement cAchi = _achievementManager.Achievements.Find(achi => achi.Title == "The corruption is cleansed") as CounterAchievement;
            goodEndingTrigger = cAchi.Counter == 3;
        }

        string achievementName = sceneNum switch
        {
            1 => "Enter the Jungle",
            2 => "Jungle with a View",
            3 => "Source of the corruption",
            4 => (goodEndingTrigger ? "The corruption is cleansed" : "The Corruption Lingers"),
            _ => string.Empty
        };


        
        

    }

    #endregion
}