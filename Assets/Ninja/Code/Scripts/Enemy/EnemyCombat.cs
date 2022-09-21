using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyCombat : MonoBehaviour
{
    #region Public Properties
    
    public GameObject Target { get; private set; }
    public bool ChaseTarget { get; private set; }
    public bool InCombat { get; private set; }
    public bool LightAttackPerformed { get; set; }
    public bool SlowAttackPerformed { get; set; }
    
    #endregion
    
    #region Public Fields
    
    [Header("Attack Damage Settings")]
    
    [Tooltip("Light attack damage amount")] 
    public float lightAttackDmg = 12.5f;

    [Tooltip("heavy attack damage amount")] 
    public float heavyAttackDmg = 25f;
    
    #endregion
    
    #region Serialized Fields

    [Header("Combat Timing Settings")]
    
    [Tooltip("Wait time between attacks")] 
    [SerializeField] private float timeBetweenAttacks = 1.5f;

    [Header("Combat Distance Settings")]
    
    [Tooltip("The distance from the target that is acceptable to trigger combat")] 
    [SerializeField] private float combatReach = 2f;
    
    [Tooltip("Distance on the X-axis to cause disengage from target")] 
    [SerializeField] private float disengageXDistance = 5f;
    
    [Tooltip("Distance on the Y-axis to cause disengage from target")] 
    [SerializeField] private float disengageYDistance = 2f;

    #endregion
    
    #region Private Fields
    
    private Animator _animator;
    private float _timeBetweenAttacksTimer;

    // Enemy Scripts
    private Health _health;
    
    // Target Scripts
    private Health _targetHealth;
    
    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _timeBetweenAttacksTimer = timeBetweenAttacks;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_health.Dead)
            return;
        
        CombatStateUpdate();
        TargetRangeUpdate();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player") || Target)
            return;

        Debug.Log("TARGET ACQUIRED!");
        
        _targetHealth = col.gameObject.GetComponentInParent<Health>();
        
        if (!_targetHealth || _targetHealth.Dead)
            return;

        SetTarget(col.gameObject);
    }

    #endregion
    
    #region Update Methods
    
    /// <summary>
    /// Update AI's combat states. If the target is dead, then the target and combat states will be reset. If the target
    /// is outside of combat reach, then the AI will try to continue pursuing.
    /// </summary>
    private void CombatStateUpdate()
    {
        if (!Target)
            return;

        if (Target && _targetHealth.Dead)
        {
            ResetCombatStates();
            ResetTarget();
            return;
        }

        if (Vector2.Distance(Target.transform.position, transform.position) <= combatReach)
        {
            InCombat = true;
            ExecuteRandomMeleeAttack();
        }
        else
            InCombat = false;
    }
    
    /// <summary>
    /// Update AI's current target depending on range. If out of range, the target and combat states will be reset.
    /// </summary>
    private void TargetRangeUpdate()
    {
        if (!Target)
            return;

        Vector2 targetPosition = Target.transform.position;
        Vector2 currentPosition = transform.position;

        float yDist = Mathf.Abs(targetPosition.y - currentPosition.y);
        float xDist = Mathf.Abs(targetPosition.x - currentPosition.x);

        if (xDist < disengageXDistance && yDist < disengageYDistance)
            return;

        Debug.Log("Target out of range!");
        
        ResetCombatStates();
        ResetTarget();
    }
    
    /// <summary>
    /// Execute a random melee attack, will only attack based on the set time that has passed from the last attack.
    /// </summary>
    private void ExecuteRandomMeleeAttack()
    {
        if (_timeBetweenAttacksTimer <= 0)
        {
            switch (Random.Range(1, 100))
            {
                case <= 25:
                    _animator.SetTrigger("LightAttackLeft");
                    LightAttackPerformed = true;
                    break;
                case > 25 and <= 50:
                    _animator.SetTrigger("LightAttackRight");
                    LightAttackPerformed = true;
                    break;
                default:
                    _animator.SetTrigger("SlowAttack");
                    SlowAttackPerformed = true;
                    break;
            }

            _timeBetweenAttacksTimer = timeBetweenAttacks;
        }
        else
            _timeBetweenAttacksTimer -= Time.deltaTime;
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Notify the AI of an engagement when not already in chase movement or combat.
    /// </summary>
    /// <param name="invoker">The invoker that started the engagement.</param>
    public void NotifyEngagement(GameObject invoker)
    {
        if (InCombat || ChaseTarget)
            return;
        
        FaceTarget(invoker);
    }

    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Set the current target that the AI is engaging.
    /// </summary>
    /// <param name="target">The target the AI is initiating combat with.</param>
    private void SetTarget(GameObject target)
    {
        Debug.Log("Setting Target!");
        Target = target;
        ChaseTarget = true;
    }

    /// <summary>
    /// Reset the target that the AI was engaging.
    /// </summary>
    private void ResetTarget()
    {
        Debug.Log("Resetting Target!");
        Target = null;
        _targetHealth = null;
    }

    /// <summary>
    /// Face the target if the AI is not already facing it.
    /// </summary>
    /// <param name="target">The target that the AI should face.</param>
    private void FaceTarget(GameObject target)
    {
        Vector3 currentScale = transform.localScale;
        if (target.transform.localScale.x == transform.localScale.x)
            transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
    }
    
    /// <summary>
    /// Reset all animator attack triggers for the AI. 
    /// </summary>
    private void ResetAllAttackTriggers()
    {
        Debug.Log("Resetting attack triggers!");
        _animator.ResetTrigger("LightAttackLeft");
        _animator.ResetTrigger("LightAttackRight");
        _animator.ResetTrigger("SlowAttack");
    }
    
    /// <summary>
    /// Reset all combat states, including animator attack triggers, for the AI.
    /// </summary>
    private void ResetCombatStates()
    {
        Debug.Log("Resetting combat states!");
        InCombat = false;
        ChaseTarget = false;
        ResetAllAttackTriggers();
    }
    
    #endregion
}