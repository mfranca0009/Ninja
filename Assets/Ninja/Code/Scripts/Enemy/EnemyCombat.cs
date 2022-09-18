using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyCombat : MonoBehaviour
{
    public GameObject Target { get; private set; }
    public bool ChaseTarget { get; private set; }
    public bool InCombat { get; private set; }
    
    [Tooltip("Light attack damage amount")] 
    [SerializeField] private float lightAttackDmg = 12.5f;

    [Tooltip("heavy attack damage amount")] 
    [SerializeField] private float heavyAttackDmg = 25f;

    [Tooltip("The distance from the target that is acceptable to trigger combat")] 
    [SerializeField] private float combatReach = 1.5f;

    [Tooltip("Acceptable tolerance for combat reach")] 
    [SerializeField] private float combatReachTolerance = 0.03f;

    [Tooltip("Wait time between attacks")] 
    [SerializeField] private float timeBetweenAttacks = 1.5f;

    private Animator _animator;
    private float _timeBetweenAttacksTimer;

    // Enemy Scripts
    private Health _health;
    
    // Target Scripts
    private Health _targetHealth;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _timeBetweenAttacksTimer = timeBetweenAttacks;
    }

    // Update is called once per frame
    private void Update()
    {
        CombatStateUpdate();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag("Player"))
            return;
        
        Debug.Log("TARGET ENTERS!");
        
        _targetHealth = col.gameObject.GetComponent<Health>();
        
        if (_targetHealth.Dead)
            return;

        SetTarget(col.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;
        
        Debug.Log("TARGET EXITS!");
        ResetTarget();
    }

    private void CombatStateUpdate()
    {
        if (!Target)
        {
            ResetCombatStates();
            return;
        }

        if (Target && _targetHealth.Dead)
        {
            ResetCombatStates();
            ResetTarget();
            return;
        }

        if (Vector2.Distance(Target.transform.position, transform.position)
            <= combatReach + combatReachTolerance)
        {
            InCombat = true;
            ExecuteRandomMeleeAttack();
        }
        else
            InCombat = false;
    }
    
    private void ExecuteRandomMeleeAttack()
    {
        if (_timeBetweenAttacksTimer <= 0)
        {
            switch (Random.Range(1, 100))
            {
                case <= 25:
                    _animator.SetTrigger("LightAttackLeft");
                    _targetHealth.DealDamage(lightAttackDmg);
                    break;
                case > 25 and <= 50:
                    _animator.SetTrigger("LightAttackRight");
                    _targetHealth.DealDamage(lightAttackDmg);
                    break;
                default:
                    _animator.SetTrigger("SlowAttack");
                    _targetHealth.DealDamage(heavyAttackDmg);
                    break;
            }

            _timeBetweenAttacksTimer = timeBetweenAttacks;
        }
        else
            _timeBetweenAttacksTimer -= Time.deltaTime;
    }

    private void SetTarget(GameObject target)
    {
        Target = target;
        ChaseTarget = true;
    }

    private void ResetTarget()
    {
        Target = null;
        _targetHealth = null;
        ChaseTarget = false;
    }

    private void ResetAllAttackTriggers()
    {
        _animator.ResetTrigger("LightAttackLeft");
        _animator.ResetTrigger("LightAttackRight");
        _animator.ResetTrigger("SlowAttack");
    }
    
    private void ResetCombatStates()
    {
        InCombat = false;
        ChaseTarget = false;
        ResetAllAttackTriggers();
    }
}
