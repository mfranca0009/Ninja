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

        //Achievements 1, 2, 3, 8, 9
        #region Level Completion Achievements

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

        _achievementManager.Achievements.Find(achi => achi.Title == achievementName).Obtained = true;
        #endregion

        //Achievements 4, 5, 6, 7
        #region Level Speed Clear Achievements

        //Determine which Achievement we should be checking by scene number
        achievementName = sceneNum switch
        {
            1 => "Quick Ninja",
            2 => "Hasty Ninja",
            3 => "Untraceable Ninja",
            4 => "Coup de Grâce",
            _ => string.Empty
        };

        //Store the matching Achievement as a SpeedBasedAchievement so we can access time based properties
        SpeedBasedAchievement sAchi = _achievementManager.Achievements.Find(achi => achi.Title == achievementName) as SpeedBasedAchievement;

        //If the time is low enough, and hasn't been awarded already, award the achievement.
        if (sAchi.TimeElapsed <= sAchi.TimeToBeat && sAchi.Obtained == false)
            sAchi.Obtained = true;

        #endregion
    }

    #endregion
}