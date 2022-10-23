using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelScroll : MonoBehaviour
{
    #region Public Fields

    [Header("End of Level Scroll Settings")]
    
    [Tooltip("The background music audio clip to be played when the end of level scroll appears.")]
    public AudioClip scrollBgMusic;
    
    #endregion
    
    #region Private Fields

    // Managers
    private UIManager _uiManager;
    private AchievementManager _achievementManager;
    private GameManager _gameManager;
    private SoundManager _soundManager;
    
    #endregion

    #region Unity Events

    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
        _gameManager = FindObjectOfType<GameManager>();
        _soundManager = FindObjectOfType<SoundManager>();
        
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

        if (!_soundManager)
            return;

        _soundManager.PlayMusic(scrollBgMusic);
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
            2 => "Enter the Jungle",
            3 => "Jungle with a View",
            4 => "Source of the corruption",
            5 => (goodEndingTrigger ? "The corruption is cleansed" : "The Corruption Lingers"),
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
            2 => "Quick Ninja",
            3 => "Hasty Ninja",
            4 => "Untraceable Ninja",
            5 => "Coup de Grâce",
            _ => string.Empty
        };
        
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
                2 => "Level 1 Genocide",
                3 => "Level 2 Genocide",
                4 => "Level 3 Genocide",
                _ => string.Empty
            };

            _achievementManager.ObtainAchievement(achievementName);
        }
        
        if (_gameManager.TotalEnemiesForLevel - _gameManager.GetEnemyCount() is > 1 or < 1)
            return;
        
        ////PACIFIST
        achievementName = sceneNum switch
        {
            2 => "Level 1 Mostly Pacifist",
            3 => "Level 2 Mostly Pacifist",
            4 => "Level 3 Mostly Pacifist",
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
