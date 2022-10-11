using UnityEngine;

public class Health : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The living state of the gameobject
    /// </summary>
    public bool Dead { get; private set; }
    
    /// <summary>
    /// The current health points of the gameobject
    /// </summary>
    public float HealthPoints { get; private set; }
    
    /// <summary>
    /// The gameobject who has killed the owning gameobject of this script
    /// </summary>
    public GameObject Killer { get; private set; }
    
    #endregion
    
    #region Public Fields

    [Header("Health Settings")]
    
    [Tooltip("Maximum health points to start with on spawn")]
    public float maxHealth = 100f;

    [Tooltip("Is this the player gameobject?")]
    public bool isPlayer;

    #endregion

    #region Serialized Fields

    [Header("Enemy Death Settings")] 
    
    [Tooltip("Enable destroy of gameobject after death and item drop")] 
    [SerializeField] private bool shouldDestroyAfterDeath = true;

    [Tooltip("Delay before destroying gameobject")] 
    [SerializeField] private float destroyDelay = 2.0f;
    
    [Header("Player Sound Effect Settings")] 
    
    [Tooltip("Death sound effect to play when player dies")] 
    [SerializeField] private AudioClip playerDeathSoundClip;

    [Header("Enemy Sound Effect Settings")] 
    
    [Tooltip("Should randomize enemy death sound effect pitch?")] 
    [SerializeField] private bool shouldRandomizePitch;
    
    [Tooltip("Should sound effect be simultaneous with other sound effects?")] 
    [SerializeField] private bool isOneShot;
    
    [Tooltip("Death sound effects to use at random")] 
    [SerializeField] private AudioClip[] enemyDeathSoundClips;

    #endregion
    
    #region Private Fields

    // Rigidbody / Physics
    private Rigidbody2D _rigidbody2D;
    
    // Animator / animations
    private Animator _animator;
    
    // Sound Effects / Music
    private SoundManager _soundManager;

    // Death
    private BoxCollider2D _boxCollider2D;
    private bool _skipDeathAnimation;
    private ItemDrop _enemyItemDrop;
    
    // Game Manager
    private GameManager _gameManager;
    
    // UI Manager
    private UIManager _uiManager;
    
    // Player Scripts
    private PlayerMovement _playerMovement;
    
    #endregion

    #region Unity Events
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _soundManager = FindObjectOfType<SoundManager>();
        _gameManager = FindObjectOfType<GameManager>();
        _uiManager = FindObjectOfType<UIManager>();
        HealthPoints = maxHealth;
    }
    
    // Update is called once per frame
    private void Update()
    {
        // Call this in Update to retrieve once, cannot call from Awake/Start because the game manager retrieves
        // the player// through the Update loop due to it being a persistent object.
        if (!_playerMovement && _gameManager && _gameManager.Player && isPlayer)
            _playerMovement = _gameManager.Player.GetComponent<PlayerMovement>();

        switch (Dead)
        {
            case true when !_skipDeathAnimation:
                _animator.SetBool("IsDead", Dead);
                break;
            case false:
                _animator.SetBool("IsDead", Dead);
                return;
        }

        if (_enemyItemDrop && _enemyItemDrop.HasDroppedItems && shouldDestroyAfterDeath)
            Destroy(gameObject, destroyDelay);
        
        // FIXME: this should be checking if player is grounded, but death animation triggers
        // not being grounded due to fall back. Player falls through floor in this case even when he was on ground.
        if (_playerMovement &&  (_playerMovement.IsJumping() || _playerMovement.IsFalling()))
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX;
        else
        {
            _boxCollider2D.enabled = false;
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
    
    #endregion

    #region Public Helper Methods
    
    /// <summary>
    /// Deal damage and reduce the health of the gameobject that has this script attached.
    /// </summary>
    /// <param name="damage">The damage amount that will be subtracted from health.</param>
    /// <param name="invoker">The invoker who caused the damage.</param>
    public void DealDamage(float damage, GameObject invoker = null)
    {
        if (HealthPoints == 0 || damage == 0)
            return;
        
        string gameObjectName = gameObject.name;
        
        if (HealthPoints - damage <= 0f)
        {
            HealthPoints = 0f;
            Dead = true;

            if (invoker)
                Killer = invoker;

            switch (isPlayer)
            {
                case false:
                    _enemyItemDrop = GetComponent<ItemDrop>();
                    break;
                // Verify if the player is dying and reduce the lives count if not 0.
                case true when _gameManager.Lives > 0 && _gameManager && _uiManager:
                    _gameManager.Lives--;
                    _uiManager.UpdateLivesUI(_gameManager.Lives);
                    break;
            }

            Debug.Log($"[Health/DealDamage] {gameObjectName} damaged for {damage}. {gameObjectName} has been killed!");
        }
        else
        {
            HealthPoints -= damage;

            EnemyCombat enemyCombat = GetComponent<EnemyCombat>();
            if (enemyCombat)
            {
                if(invoker && !enemyCombat.ChaseTarget && !enemyCombat.InCombat)
                    enemyCombat.NotifyEngagement(invoker);

                if (!_gameManager.TappedEnemiesHealth.Contains(this))
                    _gameManager.TappedEnemiesHealth.Add(this);
            }

            Debug.Log($"[Health/DealDamage] {gameObjectName} damaged for {damage}; {HealthPoints} remaining.");
        }
    }

    /// <summary>
    /// Deal a heal and increase the health of the gameobject that has this script attached.
    /// </summary>
    /// <param name="heal">The heal amount that will be added to health.</param>
    public void DealHeal(float heal)
    {
        if (HealthPoints >= maxHealth || heal == 0)
            return;

        string gameObjectName = gameObject.name;
        
        if (HealthPoints + heal >= maxHealth)
        {
            HealthPoints = 100f;
            Debug.Log($"[Health/DealHeal] {gameObjectName} healed to full health!");
        }
        else
        {
            HealthPoints += heal;
            Debug.Log($"[Health/DealHeal] {gameObjectName} healed for {heal}! Now has {HealthPoints} health.");
        }
    }
    
    /// <summary>
    /// Instantly kill the gameobject that has this health script attached.
    /// </summary>
    /// <param name="skipDeathAnimation">Whether the death animation should be skipped or not.</param>
    /// <param name="isPlayer">Whether this gameobject that is invoking this method is a player or not.</param>
    public void InstaKill(bool skipDeathAnimation)
    {
        HealthPoints = 0f;
        Dead = true;
        _skipDeathAnimation = skipDeathAnimation;

        if (!_gameManager || !isPlayer)
            return;
        
        _gameManager.Lives--;
        _uiManager.UpdateLivesUI(_gameManager.Lives);
    }

    /// <summary>
    /// Reset necessary attributes to bring the health component back to its clean state<br></br><br></br>
    /// </summary>
    public void Reset()
    {
        HealthPoints = maxHealth;
        Dead = false;
        _boxCollider2D.enabled = true;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    /// <summary>
    /// Play death sound effect.<br></br><br></br>
    /// Note: This is currently executed on its own by an Animation Event through the `Rogue_death_01` animation.
    /// </summary>
    public void ExecuteDeathSoundEffect()
    {
        if (!Dead || !_soundManager)
            return;
        
        if(gameObject.CompareTag("Enemy"))
            _soundManager.RandomSoundEffect(AudioSourceType.DamageEffects, isOneShot, shouldRandomizePitch,
                enemyDeathSoundClips);
        else
            _soundManager.PlaySoundEffect(AudioSourceType.DamageEffects, playerDeathSoundClip);
    }
    
    #endregion
}