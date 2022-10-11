using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // UI
    public Image healthBar;

    // States
    private bool _healthSet;
    
    // Health
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
        if (!_gameManager)
            return;
        
        if (!_health || !_sceneManagement.HasBuildIndex(SceneManager.GetActiveScene(), 0))
            _health = _gameManager.Player.GetComponent<Health>();
        
        if (!_health)
            return;
        
        healthBar.fillAmount = _health.HealthPoints / _health.maxHealth;
    }
}