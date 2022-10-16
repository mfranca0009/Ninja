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

    // Managers
    private UIManager _uiManager;
    private AchievementManager _achievementManager;
    private SceneManagement _sceneManagement;
    private GameManager _gameManager;
    #endregion

    #region Unity Events

    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
        _sceneManagement = FindObjectOfType<SceneManagement>();
        _gameManager = FindObjectOfType<GameManager>();


        if (!_uiManager)
            return;
        
        _uiManager.ShowScrollUI(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !_uiManager)
            return;
        
        _uiManager.ShowScrollUI(true);

        CheckAchievements();
        

    }

    private void CheckAchievements()
    {
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

        _achievementManager.ObtainAchievement(achievementName);
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

        _achievementManager.ObtainAchievement(achievementName);

        #endregion

        //Achievements 10, 11, 12, 13, 14, 15
        #region Genecide/Pacifist Achievements

        ////GENOCIDE
        //Determine which Achievement we should be checking by scene number
        if (_gameManager.GetEnemyCount() == 0)
        {
            achievementName = sceneNum switch
            {
                1 => "Level 1 Genocide",
                2 => "Level 2 Genocide",
                3 => "Level 3 Genocide",
                _ => string.Empty
            };

            _achievementManager.ObtainAchievement(achievementName);
        }

        ////PACIFIST
        if (_gameManager.EnemyCount - _gameManager.GetEnemyCount() <= 1)
        {
            achievementName = sceneNum switch
            {
                1 => "Level 1 Mostly Pacifist",
                2 => "Level 2 Mostly Pacifist",
                3 => "Level 3 Mostly Pacifist",
                _ => string.Empty
            };

            _achievementManager.ObtainAchievement(achievementName);
        }

        #endregion

        //End of Level 3 checks
        //Achievement 18. No Traps Activated
        //Achievement 19. Proud Ninja
        if (sceneNum == 3)
        {
            _achievementManager.ObtainAchievement("No Traps Activated");
            _achievementManager.ObtainAchievement("Proud Ninja");
        }

        //End of Level
        //Achievements 21 - 23
        if (sceneNum == 4)
        {
            _achievementManager.ObtainAchievement("Martial Ninja");
            _achievementManager.ObtainAchievement("Distance Ninja");
            _achievementManager.ObtainAchievement("Expert Ninja");
        }

    }

    #endregion
}
