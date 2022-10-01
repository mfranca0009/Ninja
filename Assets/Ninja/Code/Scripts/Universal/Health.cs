using UnityEngine;

public class Health : MonoBehaviour
{
    #region Public Properties
    
    public bool Dead { get; private set; }
    public float HealthPoints { get; private set; }
    public GameObject Killer { get; private set; }
    
    #endregion
    
    #region Public Fields

    [Tooltip("Maximum health points to start with on spawn")]
    public float maxHealth = 100f;
    
    #endregion

    #region Private Fields
    
    private Animator _animator;
    private BoxCollider2D _boxCollider2D;
    private Rigidbody2D _rigidbody2D;
    private bool _playedDeathAnimation;
    
    #endregion

    #region Unity Events
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        HealthPoints = maxHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Dead || _playedDeathAnimation)
            return;
        
        _animator.SetTrigger("Dead");
        _playedDeathAnimation = true;
        _boxCollider2D.enabled = false;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    
    #endregion

    #region Public Methods
    
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
            
            Debug.Log($"[Health/DealDamage] {gameObjectName} damaged for {damage}. {gameObjectName} has been killed!");
        }
        else
        {
            HealthPoints -= damage;
            
            EnemyCombat enemyCombat = GetComponent<EnemyCombat>();
            if (enemyCombat && invoker && !enemyCombat.ChaseTarget && !enemyCombat.InCombat)
                enemyCombat.NotifyEngagement(invoker);
            
            Debug.Log($"[Health/DealDamage] {gameObjectName} damaged for {damage}; {HealthPoints} remaining.");
        }
    }
    
    /// <summary>
    /// Instantly kill the gameobject that has this health script attached.
    /// </summary>
    /// <param name="skipDeathAnimation">Whether the death animation should be skipped or not.</param>
    public void InstaKill(bool skipDeathAnimation)
    {
        HealthPoints = 0f;
        Dead = true;
        _playedDeathAnimation = skipDeathAnimation;
    }
    
    #endregion
}