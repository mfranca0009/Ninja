using UnityEngine.SceneManagement;
using UnityEngine;

public class RestartLevel : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Timer Settings")]
    
    [Tooltip("The delay before reloading the scene")]
    [SerializeField] private float respawnDelay = 2.0f;
    
    #endregion
    
    #region Private Fields
    
    private Health _health;
    private float _respawnTimer;
    
    #endregion

    #region Unity Events
    
    // Start is called before the first frame update
    private void Start()
    {
        _health = GetComponent<Health>();
        _respawnTimer = respawnDelay;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_health.Dead)
            return;

        if (_respawnTimer <= 0f)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            _respawnTimer -= Time.deltaTime;
    }
    
    #endregion
}