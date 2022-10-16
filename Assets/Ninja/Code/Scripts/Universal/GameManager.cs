using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#region Helper Classes

[Serializable]
public class LevelPositions
{
    public string levelName;
    public Vector2 startPosition;
    public Vector2 midpointPosition;
}

#endregion

public class GameManager : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The player instance of the game
    /// </summary>
    public GameObject Player { get; private set; }
    
    /// <summary>
    /// The current amount of lives the player has left.
    /// </summary>
    public int Lives { get; set; }
    
    /// <summary>
    /// Whether the player has reached the level's midpoint or not.
    /// </summary>
    public bool LevelMidpointReached { get; set; }

    /// <summary>
    /// Whether the game was restarted after game over or not.
    /// </summary>
    public bool Restarted { get; set; }

    /// <summary>
    /// Active item drops within current level
    /// </summary>
    public List<GameObject> ActiveItemDrops { get; private set; }

    /// <summary>
    /// Enemies health that have been "tapped" by the player but not yet killed.
    /// </summary>
    public List<Health> TappedEnemiesHealth { get; private set; }

    /// <summary>
    /// Total amount of enemies within the current level
    /// </summary>
    public int EnemyCount { get; private set; }
    
    #endregion

    #region Serialized Fields

    [Header("Game Manager Settings")] 
    
    [Tooltip("Respawn positions for game levels")] 
    [SerializeField] private List<LevelPositions> levelRespawnPositions;

    [Tooltip("Delay time before player resets after dying with lives remaining")] 
    [SerializeField] private float resetDelay = 3f;

    [Tooltip("Delay time before game over occurs with no lives remaining")] 
    [SerializeField] private float gameOverDelay = 3f;
    
    [Header("Lives Settings")] 
    
    [Tooltip("The amount of lives available")] 
    [SerializeField] private int livesAmount;
    
    #endregion

    #region Private Fields
    
    // Scene Management
    private Scene _currScene;
    private int _currSceneBuildIndex;
    private SceneManagement _sceneManagement;

    // UI Manager
    private UIManager _uiManager;
    
    // Achievement Manager
    private AchievementManager _achievementManager;
    
    // Player Scripts
    private Health _playerHealth;
    private PlayerCombat _playerCombat;
    private PlayerCamera _playerCamera;
    
    // Timers
    private float _resetDelayTimer;
    private float _gameOverDelayTimer;
    
    // States
    private bool _livesSet;
    private bool _gameOver;

    // Achievements
    private SpeedAchievement _speedAchievement;

    #endregion

    #region Unity Events

    private void Awake()
    {
        _sceneManagement = FindObjectOfType<SceneManagement>();
        _uiManager = FindObjectOfType<UIManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
        _resetDelayTimer = resetDelay;
        _gameOverDelayTimer = gameOverDelay;
        ActiveItemDrops = new List<GameObject>();
        TappedEnemiesHealth = new List<Health>();
    }

    // Update is called once per frame
    // Note: order of execution is important here! Be wary if you attempt to add/change something within the Update loop.
    private void Update()
    {
        // Retrieve current scene.
        _currScene = SceneManager.GetActiveScene();

        // If the scene is changed, update appropriate states and clean-ups.
        if (_currScene.buildIndex != _currSceneBuildIndex)
        {
            EnemyCount = GetEnemyCount();
            LevelMidpointReached = false;
            ActiveItemDrops.Clear();
            TappedEnemiesHealth.Clear();
        }

        // If the player does not exist or is invalid for this scene, then retrieve the player again.
        if (!Player || !Player.scene.IsValid())
            Player = GameObject.FindWithTag("Player");

        // Retrieve the required player scripts for the game manager if necessary.
        RetrievePlayerScripts();
        
        // If the scene is not the menu, then we should be updating these timers and clean-ups when necessary.
        if (!_sceneManagement.HasBuildIndex(_currScene, 0))
        {
            UpdateDeathResetTimer();
            WipeActiveItemDrops();
            WipePlayerBuffs();
            RestoreEnemiesHealth();
            UpdateGameOverTimer();
        }

        // Update lives and health when necessary.
        LivesUpdate();
        HealthUpdate();

        // If the game has restarted or game over was reached, reset the appropriate attributes.
        if (Restarted || _gameOver)
        {
            _resetDelayTimer = resetDelay;
            _gameOverDelayTimer = gameOverDelay;
            
            Restarted = false;
            _gameOver = false;
            LevelMidpointReached = false;
        }

        //increment the proper level timer
        IncrementLevelTimer();


        // Update the scene's build index
        _currSceneBuildIndex = _currScene.buildIndex;

    }

    #endregion

    #region Update Methods
    
      /// <summary>
    /// Update lives when needed based on certain conditions.
    /// </summary>
    private void LivesUpdate()
    {
        // Update states to signal a lives reset.
        if (_sceneManagement.HasBuildIndex(_currScene, 0) ||
            (!_sceneManagement.HasBuildIndex(_currScene, 0) && Restarted))
            _livesSet = false;

        // if lives are not set yet, then set the appropriate lives total and update the state.
        if (!_livesSet)
        {
            Lives = livesAmount;
            _livesSet = true;
        }
        
        // Only add a life if the scene has changed and lives are not at max.
        if (Lives != livesAmount && _currScene.buildIndex != _currSceneBuildIndex)
            Lives++;

        if (!_uiManager)
            return;
        
        // Update the lives UI
        _uiManager.UpdateLivesUI(Lives);
    }

    /// <summary>
    /// Update the health after death, on restart, or progressing to the next level
    /// </summary>
    private void HealthUpdate()
    {
        if (!_playerHealth || _playerHealth.HealthPoints >= _playerHealth.maxHealth ||
            (_currScene.buildIndex == _currSceneBuildIndex && !Restarted) ||
            _sceneManagement.HasBuildIndex(_currScene, 0))
            return;
        
        _playerHealth.Reset();
    }

    /// <summary>
    /// Timer update for reset after death
    /// </summary>
    private void UpdateDeathResetTimer()
    {
        if (!_playerHealth.Dead || Lives == 0)
            return;
        
        if (_resetDelayTimer <= 0f)
        {
            Vector2 relocatePos = GetRespawnPositionForLevel(_currScene, LevelMidpointReached);
            _playerHealth.Reset();
            _playerHealth.gameObject.transform.position = relocatePos;
            
            // Camera is deactivated on boss level, add a null check to prevent runtime error.
            if (_playerCamera && _playerCamera.isActiveAndEnabled)
                _playerCamera.SetCameraPosition(relocatePos);
            
            _resetDelayTimer = resetDelay;
        }
        else
            _resetDelayTimer -= Time.deltaTime;

        //Disable no death eligability
        _achievementManager.Achievements.Find(achi => achi.Title == "Expert Ninja").Eligible = false;
    }

    /// <summary>
    /// Timer update for the game over UI to appear after death with no lives remaining
    /// </summary>
    private void UpdateGameOverTimer()
    {
        if (!_uiManager || !_playerHealth.Dead || Lives > 0 || _gameOver)
            return;
        
        if (_gameOverDelayTimer <= 0f)
        {
            _uiManager.ShowFinishedUI(true);
            _gameOverDelayTimer = gameOverDelay;
            _gameOver = true;
        }
        else
            _gameOverDelayTimer -= Time.deltaTime;
    }




    #endregion

    #region Achievement Helper Methods

    private void IncrementLevelTimer()
    {
        if (_currSceneBuildIndex != _currScene.buildIndex)
        {
            SpeedAchievement speedAchievement = _currScene.buildIndex switch
            {
                1 => _achievementManager.Achievements.Find(achievement => achievement.Title == "Quick Ninja") as SpeedAchievement,
                2 => _achievementManager.Achievements.Find(achievement => achievement.Title == "Hasty Ninja") as SpeedAchievement,
                3 => _achievementManager.Achievements.Find(achievement => achievement.Title == "Untrackable Ninja") as SpeedAchievement,
                4 => _achievementManager.Achievements.Find(achievement => achievement.Title == "Coup de Grace") as SpeedAchievement,
                _ => new SpeedAchievement(AchievementType.SpeedType, "", "", 0.0f)
            };
            _speedAchievement = speedAchievement;
        }

        if (_speedAchievement == null || _speedAchievement.Title == "")
            return;
        
        _speedAchievement.TimeElapsed += Time.deltaTime;
        
        if (_speedAchievement.TimeElapsed > _speedAchievement.TimeToBeat)
            _speedAchievement.Eligible = false;
    }

    public int GetEnemyCount()
    {
        return FindObjectsOfType<EnemyCombat>().Length;
    }

    public void CollectScroll(int scrollNum)
    {
        //TODO: Tell UI to add visual indicator of having collected the matching scroll

        ((CounterAchievement)_achievementManager.Achievements.Find(achievement =>
            achievement.Title == "The corruption is cleansed")).Counter++;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Wipe all active item drops as long as they are still valid references in the current level.
    /// </summary>
    private void WipeActiveItemDrops()
    {
        if (!_playerHealth.Dead || Lives == 0 || ActiveItemDrops.Count == 0)
            return;

        ActiveItemDrops.FindAll(activeObj => activeObj).ForEach(Destroy);
        ActiveItemDrops.Clear();
    }

    /// <summary>
    /// Wipe all active buffs from item pickups from player.
    /// </summary>
    private void WipePlayerBuffs()
    {
        if (!_playerHealth.Dead || Lives == 0 || (_playerCombat.MaxKnives == 1 && _playerCombat.HasMeleeStrengthBoost ==
                false && _playerCombat.StrengthBoostTimer <= 0f))
            return;

        _playerCombat.MaxKnives = 1;
        _playerCombat.HasMeleeStrengthBoost = false;
        _playerCombat.StrengthBoostTimer = 0f;
    }
    
    /// <summary>
    /// Restore health of enemies that were tapped by player and have not died as long as they
    /// are still valid references in the current level.
    /// </summary>
    private void RestoreEnemiesHealth()
    {
        if (!_playerHealth.Dead || Lives == 0 || TappedEnemiesHealth.Count == 0)
            return;

        TappedEnemiesHealth.FindAll(health => health && !health.Dead).ForEach(health => health.Reset());
        TappedEnemiesHealth.Clear();
    }
    
    /// <summary>
    /// Retrieve player scripts required for the game manager
    /// </summary>
    private void RetrievePlayerScripts()
    {
        if (_sceneManagement.HasBuildIndex(_currScene, 0) || !Player ||
            (Player && _playerHealth && _playerCombat && _playerCamera))
            return;

        _playerHealth = Player.GetComponent<Health>();
        _playerCombat = Player.GetComponent<PlayerCombat>();
        _playerCamera = Player.GetComponent<PlayerCamera>();
    }
    
    /// <summary>
    /// Retrieve the appropriate respawn position based on scene criteria
    /// </summary>
    /// <param name="scene">The current scene that is active.</param>
    /// <param name="midpointReached">Whether the midpoint of the scene was reached or not.</param>
    /// <returns></returns>
    private Vector2 GetRespawnPositionForLevel(Scene scene, bool midpointReached)
    {
        LevelPositions levelPos = levelRespawnPositions.Find(levelPos => levelPos.levelName == scene.name);
        return !midpointReached ? levelPos.startPosition : levelPos.midpointPosition;
    }
    
    #endregion
}