using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    #region Public Fields
    
    [Header("Health Bar Settings")]
    
    [Tooltip("Is this health bar owned by the player?")]
    public bool isPlayer;
    
    [Tooltip("Enemy health component, if this health bar is owned by an enemy")]
    public Health enemyHealth;
    
    [Tooltip("The health bar fill image")]
    public Image healthBar;
    
    #endregion

    #region Private Fields
    
    // States
    private bool _healthSet;

    // Player health
    private Health _health;
    
    // Game Manager
    private GameManager _gameManager;
    
    // Scene Manager
    private SceneManagement _sceneManagement;
    
    #endregion

    #region Unity Events
    
    // Start is called before the first frame update
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _sceneManagement = FindObjectOfType<SceneManagement>();
    }

    // Update is called once per frame
    private void Update()
    {
        switch (isPlayer)
        {
            case true when !_gameManager:
                return;
            case true when !_sceneManagement.HasBuildIndex(SceneManager.GetActiveScene(), 0, 1, 6) && !_health:
                _health = _gameManager.Player.GetComponent<Health>();
                break;
        }

        if ((isPlayer && !_health) || (!isPlayer && !enemyHealth))
            return;
        
        healthBar.fillAmount = isPlayer switch
        {
            true => _health.HealthPoints / _health.maxHealth,
            false => enemyHealth.HealthPoints / enemyHealth.maxHealth
        };
    }
    
    #endregion
}