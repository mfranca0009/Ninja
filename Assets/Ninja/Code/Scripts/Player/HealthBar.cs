using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public bool isPlayer;
    
    // Enemy health component
    public Health enemyHealth;
    
    // UI
    public Image healthBar;

    // States
    private bool _healthSet;

    // Player health
    private Health _health;
    
    // Game Manager
    private GameManager _gameManager;
    
    // Scene Manager
    private SceneManagement _sceneManagement;

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
            case true when !_sceneManagement.HasBuildIndex(SceneManager.GetActiveScene(), 0) && !_health:
                _health = _gameManager.Player.GetComponent<Health>();
                break;
        }

        healthBar.fillAmount = isPlayer switch
        {
            true => _health.HealthPoints / _health.maxHealth,
            false => enemyHealth.HealthPoints / enemyHealth.maxHealth
        };
    }
}