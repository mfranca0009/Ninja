using UnityEngine;

public class Health : MonoBehaviour
{
    public bool Dead { get; private set; }
    public float HealthPoints { get; private set; }
    
    [Tooltip("Maximum health points to start with on spawn")]
    [SerializeField] private float maxHealth = 100f;

    private Animator _animator;
    private BoxCollider2D _boxCollider2D;
    private Rigidbody2D _rigidbody2D;
    private bool _playedDeathAnimation;

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

    public void DealDamage(float damage)
    {
        if (HealthPoints == 0)
            return;
        
        string gameObjectName = gameObject.name;
        
        if (HealthPoints - damage <= 0f)
        {
            HealthPoints = 0f;
            Dead = true;
            Debug.Log($"{gameObjectName} damaged for {damage}. {gameObjectName} has been killed!");
        }
        else
        {
            HealthPoints -= damage; 
            Debug.Log($"{gameObjectName} damaged for {damage}; {HealthPoints} remaining.");
        }
    }
    
    public void InstaKill(bool skipDeathAnimation)
    {
        HealthPoints = 0f;
        Dead = true;
        _playedDeathAnimation = skipDeathAnimation;
    }
}