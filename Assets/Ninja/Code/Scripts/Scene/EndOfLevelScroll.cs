using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelScroll : MonoBehaviour
{
    #region Private Fields

    // Managers
    private UIManager _uiManager;
    private AchievementManager _achievementManager;
    private GameManager _gameManager;
    #endregion

    #region Unity Events

    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
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
        
        int sceneNum = SceneManager.GetActiveScene().buildIndex;
        CheckAllEndOfLevelAchievements(sceneNum);

    }

    /// <summary>
    /// Call all achievement check methods relevant to touching end-of-level scroll.<br></br><br></br>
    /// </summary>
    private void CheckAllEndOfLevelAchievements(int sceneNum)
    {
        CheckEndOfLevelAchievements(sceneNum);
        CheckTimeClearAchievements(sceneNum);
        CheckGenocidePacifistAchievements(sceneNum);
        CheckLevelThreeAchievements(sceneNum);
        CheckLevelFourAchievements(sceneNum);
    }
    
    /// <summary>
    /// Check end of level achievements.<br></br><br></br>
    /// Note: Achievements 1, 2, 3, 8, 9
    /// </summary>
    private void CheckEndOfLevelAchievements(int sceneNum)
    {
        if (!_achievementManager)
            return;
        
        bool goodEndingTrigger = false;

        if (sceneNum == 4)
        {
            if (_achievementManager.Achievements.Find(achievement => achievement.Title == "The corruption is cleansed")
                is not CounterAchievement counterAchievement)
                return;
            
            goodEndingTrigger = counterAchievement.Counter == 3;
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
    }

    /// <summary>
    /// Check level speed clear achievements.<br></br><br></br>
    /// Note: Achievements 4, 5, 6, 7
    /// </summary>
    private void CheckTimeClearAchievements(int sceneNum)
    {
        if (!_achievementManager)
            return;
        
        //Determine which Achievement we should be checking by scene number
        string achievementName = sceneNum switch
        {
            1 => "Quick Ninja",
            2 => "Hasty Ninja",
            3 => "Untraceable Ninja",
            4 => "Coup de Gr�ce",
            _ => string.Empty
        };

        //Store the matching Achievement as a SpeedAchievement so we can access time based properties
        SpeedAchievement speedAchievement =
            _achievementManager.Achievements.Find(achievement => achievement.Title == achievementName) as
                SpeedAchievement;

        _achievementManager.ObtainAchievement(achievementName);
    }

    /// <summary>
    /// Check genocide/pacifist achievements.<br></br><br></br>
    /// Note: Achievements 10, 11, 12, 13, 14, 15
    /// </summary>
    private void CheckGenocidePacifistAchievements(int sceneNum)
    {
        if (!_achievementManager)
            return;
        
        string achievementName;
        
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
        
        if (_gameManager.EnemyCount - _gameManager.GetEnemyCount() > 1)
            return;
        
        ////PACIFIST
        achievementName = sceneNum switch
        {
            1 => "Level 1 Mostly Pacifist",
            2 => "Level 2 Mostly Pacifist",
            3 => "Level 3 Mostly Pacifist",
            _ => string.Empty
        };

        _achievementManager.ObtainAchievement(achievementName);
    }

    /// <summary>
    /// Check level three achievements.<br></br><br></br>
    /// Note: Achievements 18, 19 [No Traps Activated, Proud Ninja]
    /// </summary>
    private void CheckLevelThreeAchievements(int sceneNum)
    {
        if (!_achievementManager || sceneNum != 3)
            return;
        
        _achievementManager.ObtainAchievement("No Traps Activated");
        _achievementManager.ObtainAchievement("Proud Ninja");
    }

    /// <summary>
    /// Check level four achievements.<br></br><br></br>
    /// Note: Achievements 21, 22, 23
    /// </summary>
    private void CheckLevelFourAchievements(int sceneNum)
    {
        if (!_achievementManager || sceneNum != 4)
            return;
        
        _achievementManager.ObtainAchievement("Martial Ninja");
        _achievementManager.ObtainAchievement("Distance Ninja");
        _achievementManager.ObtainAchievement("Expert Ninja");
    }
    
    #endregion
}