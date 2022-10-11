using UnityEngine;
using Random = UnityEngine.Random;

// TODO (NEED): Cancel movement when AI is performing slow attack

// TODO (WANT): Use a second animation layer and blending to allow combat/movement at same time for
// light and throw attack

public class EnemyCombat : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The current target the AI has acquired.
    /// </summary>
    public GameObject Target { get; private set; }
    
    /// <summary>
    /// Chase state to enable chase movement<br></br><br></br>
    /// Occurs during initial combat engagement if the AI is not within range for melee or ranged combat.
    /// </summary>
    public bool ChaseTarget { get; private set; }
    
    /// <summary>
    /// Advance state to enable advance to target movement<br></br><br></br>
    /// Occurs during ranged combat to advance the AI toward the target without requiring the target to move closer
    /// to initiate melee combat.
    /// </summary>
    public bool AdvanceTarget { get; private set; }
    
    /// <summary>
    /// Investigate state to enable investigate movement<br></br><br></br>
    /// Occurs when a target initiates combat from the back of the AI, which will result in the AI investigating the
    /// engagement.
    /// </summary>
    public bool InvestigateEngagement { get; private set; }
    
    /// <summary>
    /// The stored destination position used where the AI will move to when investigating. Once reaching this point,
    /// the AI will complete its investigation.
    /// </summary>
    public Vector2 InvestigateDestPos { get; private set; }
    
    /// <summary>
    /// In combat state <br></br><br></br>
    /// To determine when the AI is in actual melee or ranged combat with a target.
    /// </summary>
    public bool InCombat { get; private set; }
    
    /// <summary>
    /// The current attack state that the AI is performing.<br></br><br></br>
    /// Currently not used, but should be transitioned into for a more state-like system. In example, use this to
    /// control what attack state should be performed by changing this state variable.
    /// </summary>
    public AttackState AttackState { get; private set; }
    
    /// <summary>
    /// The current attack type that the AI is performing.<br></br><br></br>
    /// Currently not used, but should be transitioned into for a more state-like system. In example, use this to
    /// control what attack type should be performed by changing this state variable.
    /// </summary>
    public AttackType AttackType { get; private set; }
    
    /// <summary>
    /// Light attack perform state, used to differentiate damage amounts when attacking a target within
    /// `HitBoxDetection.cs`<br></br><br></br>
    /// Note: Animation Event during light attack animation(s) will trigger this state to true. Must be an int type,
    /// animation events do not cover boolean.
    /// </summary>
    public int LightAttackPerformed { get; set; }
    
    /// <summary>
    /// Slow attack perform state, used to differentiate damage amounts when attacking a target within
    /// `HitBoxDetection.cs`<br></br><br></br>
    /// Note: Animation Event during slow attack animation(s) will trigger this state to true. Must be an int type,
    /// animation events do not cover boolean.
    /// </summary>
    public int SlowAttackPerformed { get; set; }
    
    #endregion

    #region Serialized Fields

    [Header("Combat Attack Timing Settings")]
    
    [Tooltip("Initial wait time for melee attacks")] 
    [SerializeField] private float initialMeleeAttackTime = 0.5f;

    [Tooltip("Repeat wait time between melee attacks")] 
    [SerializeField] private float repeatMeleeAttackTime = 1.5f;
    
    [Tooltip("Initial wait time for range attacks")] 
    [SerializeField] private float initialRangeAttackTime = 0.5f;
    
    [Tooltip("Repeat wait time between range attacks")] 
    [SerializeField] private float repeatRangeAttackTime = 1.5f;
    
    [Header("Combat Movement Timing Settings")]

    [Tooltip("The wait time between advancing the target while at a ranged attack distance")] 
    [SerializeField] private float advanceTargetTime = 2.25f;

    [Tooltip("The wait time to disable the advancing of the target while at a ranged attack distance")] 
    [SerializeField] private float stopAdvanceTargetTime = 1.25f;

    [Header("Combat Movement Settings")] 
    
    [Tooltip("Whether the AI should advance towards the target when within ranged combat distance")]
    [SerializeField] private bool shouldAdvanceToTarget = true;
    
    [Header("Combat Distance Settings")]
    
    [Tooltip("The distance from the target that is acceptable to trigger melee combat")] 
    [SerializeField] private float meleeCombatReach = 2f;

    [Tooltip("The distance from the target that is acceptable to trigger range combat")] 
    [SerializeField] private float rangeCombatReach = 4f;

    [Tooltip("The distance the AI will move when investigating when an invoker engages combat from behind")]
    [SerializeField] private float investigateMoveDistance = 3f;

    [Tooltip("The tolerance distance that will trigger investigate completion reaching the investigate" +
             " destination position.")]
    [SerializeField] private float investigateCompletionDistance = 0.5f;
    
    [Tooltip("Distance on the X-axis to cause disengage from target")] 
    [SerializeField] private float disengageXDistance = 7f;
    
    [Tooltip("Distance on the Y-axis to cause disengage from target")] 
    [SerializeField] private float disengageYDistance = 2f;

    [Header("Sight Detection Settings")]
    
    [Tooltip("The size of the sight detection 2D box collider")]
    [SerializeField] private Vector2 sightCollider2DSize = new(5f, 0.5f);
    
    [Tooltip("The offset of the sight detection 2D box collider")]
    [SerializeField] private Vector2 sightCollider2DOffset = new(0f, 0f);
    
     [Header("Throwing Knife Settings")]
        
     [Tooltip("The knife prefab that will be instantiated into the scene")]
     [SerializeField] private GameObject throwKnifePrefab;
        
     [Tooltip("The knife sprite that will be visible when the knife is active within the scene")]
     [SerializeField] private Sprite throwKnifeSprite;
        
     [Tooltip("The transform the knife will use as a starting spawn position")]
     [SerializeField] private Transform throwKnifeSpawnTransform;
    
     [Tooltip("A position offset to adjust spawn positioning further in relation to the spawn transform used")]
     [SerializeField] private Vector2 throwKnifeSpawnOffset = new (1f, 0f);
     
     [Header("Melee Weapon Settings")] 
    
     [Tooltip("The left melee weapon gameobject")] 
     [SerializeField] private GameObject meleeWeaponLeft;
    
     [Tooltip("The right melee weapon gameobject")] 
     [SerializeField] private GameObject meleeWeaponRight;
    
     [Tooltip("The left melee weapon sprite that will be visible when the weapon is active within the scene")]
     [SerializeField] private Sprite meleeWeaponLeftSprite;
    
     [Tooltip("The right melee weapon sprite that will be visible when the weapon is active within the scene")]
     [SerializeField] private Sprite meleeWeaponRightSprite;

     [Header("Sound Effect Settings")]
    
     [Tooltip("The sound effects played when using a specific attack")]
     [SerializeField] private AudioClip[] attackSoundClips;
     
    #endregion
    
    #region Private Fields
    
    // Combat timers
    private float _meleeAttackTimer;
    private float _rangeAttackTimer;
    private float _advanceTargetTimer;
    private float _stopAdvanceTargetTimer;
    
    // Animator / Animation
    private Animator _animator;

    // Sound Effects / Music
    private SoundManager _soundManager;
    
    // Sight Detection Box Collider
    private BoxCollider2D _sightCollider2D;

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
        _soundManager = FindObjectOfType<SoundManager>();
        
        _meleeAttackTimer = initialMeleeAttackTime;
        _rangeAttackTimer = initialRangeAttackTime;
        _advanceTargetTimer = advanceTargetTime;
        _stopAdvanceTargetTimer = stopAdvanceTargetTime;
        
        AttackState = AttackState.None;
        AttackType = AttackType.None;

        InvestigateDestPos = Vector2.zero;
        
        SetupSightDetection();
        SetupMeleeWeapons();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_health.Dead)
            return;
        
        CombatStateUpdate();
        TargetRangeUpdate();
        InvestigateRangeUpdate();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player") || Target)
            return;

        Debug.Log("[EnemyCombat/OnTriggerEnter2D] TARGET ACQUIRED!");

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
        
        if (Vector2.Distance(Target.transform.position, transform.position) <= meleeCombatReach)
        {
            InCombat = true;
            ChaseTarget = false;
            AdvanceTarget = false;
            ExecuteRandomMeleeAttack();
        }
        else if (Vector2.Distance(Target.transform.position, transform.position) <= rangeCombatReach)
        {
            InCombat = true;
            ChaseTarget = false;
            ExecuteRangeAttack();
        }
        else
        {
            InCombat = false;
            ChaseTarget = true;
        }
    }
    
    /// <summary>
    /// Update AI's current target depending on range. If out of range, the target and combat states will be reset.
    /// </summary>
    private void TargetRangeUpdate()
    {
        if (!Target)
            return;

        Vector2 targetPos = Target.transform.position;
        Vector2 currentPos = transform.position;

        float yDist = Mathf.Abs(targetPos.y - currentPos.y);
        float xDist = Mathf.Abs(targetPos.x - currentPos.x);

        if (xDist < disengageXDistance && yDist < disengageYDistance)
            return;

        Debug.Log("[EnemyCombat/TargetRangeUpdate] Target out of range!");
        
        ResetCombatStates();
        ResetTarget();
    }

    private void InvestigateRangeUpdate()
    {
        if (!InvestigateEngagement || Target)
            return;

        Vector2 currentPos = transform.position;

        if (Mathf.Abs(Vector2.Distance(currentPos, InvestigateDestPos)) > investigateCompletionDistance)
            return;
        
        Debug.Log("[EnemyCombat/InvestigateRangeUpdate] Completed investigation!");
        InvestigateEngagement = false;
    }
    
    /// <summary>
    /// Execute a random melee attack, will only perform attack based on the set time
    /// that has passed from the last attack.
    /// </summary>
    private void ExecuteRandomMeleeAttack()
    {
        if (_meleeAttackTimer <= 0f)
        {
            AttackType = AttackType.Melee;
            
            switch (Random.Range(1, 50))
            {
                case <= 25:
                    AttackState = AttackState.LightAttack;
                    _animator.SetTrigger("LightAttack");
                    break;
                default:
                    AttackState = AttackState.SlowAttack;
                    _animator.SetTrigger("SlowAttack");
                    break;
            }

            _meleeAttackTimer = repeatMeleeAttackTime;
        }
        else
            _meleeAttackTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Execute a ranged attack, will only perform attack based on the set time that has passed from the last attack.
    /// If the AI should advance on the target, then it will do so.
    /// </summary>
    private void ExecuteRangeAttack()
    {
        if (_rangeAttackTimer <= 0f)
        {
            AttackType = AttackType.Ranged;
            AttackState = AttackState.ThrowKnife;

            _animator.SetTrigger("ThrowKnife");
            _rangeAttackTimer = repeatRangeAttackTime;
        }
        else
        {
            _rangeAttackTimer -= Time.deltaTime;
            
        }
        
        AdvanceOnTarget();
        StopAdvanceOnTarget();
    }

    /// <summary>
    /// The timer that controls when the AI should advance on the target, currently used during ranged attacking.
    /// If `shouldAdvanceToTarget` is set to true, then the AI will advance on the target once the timer has passed
    /// when within a ranged attack distance from the target.
    /// </summary>
    private void AdvanceOnTarget()
    {
        if (!shouldAdvanceToTarget && AdvanceTarget)
            return;

        if (_advanceTargetTimer <= 0f)
        {
            AdvanceTarget = true;
            _advanceTargetTimer = advanceTargetTime;
        }
        else
            _advanceTargetTimer -= Time.deltaTime;
    }

    private void StopAdvanceOnTarget()
    {
        if (!shouldAdvanceToTarget && !AdvanceTarget)
            return;

        if (_stopAdvanceTargetTimer <= 0f)
        {
            AdvanceTarget = false;
            _stopAdvanceTargetTimer = stopAdvanceTargetTime;
        }
        else
            _stopAdvanceTargetTimer -= Time.deltaTime;
    }
    
    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Notify the AI of an engagement when not already in chase movement or combat.
    /// </summary>
    /// <param name="invoker">The invoker that started the engagement.</param>
    public void NotifyEngagement(GameObject invoker)
    {
        if (InCombat || ChaseTarget)
            return;
        
        FaceTarget(invoker);

        Debug.Log("[EnemyCombat/NotifyEngagement] Investigating!");
        
        bool facingLeft = transform.localScale.x < 0;
        
        Vector2 currentPos = transform.position;
        InvestigateDestPos =
            new Vector2(currentPos.x + (facingLeft ? -investigateMoveDistance : investigateMoveDistance), currentPos.y);
        
        InvestigateEngagement = true;
    }


    /// <summary>
    /// Method to instantiate a throwing knife into the scene. It will create the object, adjust parameters, and then
    /// activate the gameobject. Activating the gameobject will trigger the movement of the throwing knife.<br></br><br></br>
    /// Note: This is currently executed on its own by an Animation Event through the `Rogue_throw_knife` animation.
    /// </summary>
    public void InstantiateThrowingKnife()
    {
        // Create throwing knife object
        GameObject knifeToCreate = Instantiate(throwKnifePrefab, throwKnifeSpawnTransform.position,
            new Quaternion());

        if (!knifeToCreate)
            return;

        ThrowKnife throwKnife = knifeToCreate.GetComponent<ThrowKnife>();
        SpriteRenderer knifeSpriteRenderer = knifeToCreate.GetComponent<SpriteRenderer>();
        Rigidbody2D knifeRigidBody2D = knifeToCreate.GetComponent<Rigidbody2D>();

        if (!throwKnife || !knifeSpriteRenderer || !knifeRigidBody2D)
            return;

        // Throwing knife setup
        throwKnife.Owner = gameObject;
        throwKnife.ThrowLeft = transform.localScale.x < 0;
        knifeSpriteRenderer.sprite = throwKnifeSprite;
        knifeSpriteRenderer.sortingLayerID = SortingLayer.NameToID("EnemyEffects");
        knifeToCreate.transform.Find("Hitbox").gameObject.layer = LayerMask.NameToLayer("EnemyRangeAttack");

        knifeToCreate.transform.position +=
            new Vector3(throwKnife.ThrowLeft ? -throwKnifeSpawnOffset.x + -0.5f : throwKnifeSpawnOffset.x,
                throwKnifeSpawnOffset.y, 0f);

        // Activate throwing knife object
        knifeRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
        knifeToCreate.SetActive(true);
    }
    
    /// <summary>
    /// Play sound effect for light attack.<br></br><br></br>
    /// Note: This is currently executed on its own by an Animation Event through the `Rogue_attack_light` animation.
    /// </summary>
    public void PlayLightAttackSfx()
    {
        if (!_soundManager)
            return;
        
        _soundManager.PlaySoundEffect(AudioSourceType.AttackEffects, attackSoundClips[(int)AttackState.LightAttack]);
    }

    /// <summary>
    /// Play sound effect for slow attack.<br></br><br></br>
    /// Note: This is currently executed on its own by an Animation Event through the `Rogue_attack_slow` animation.
    /// </summary>
    public void PlaySlowAttackSfx()
    {
        if (!_soundManager)
            return;
        
        _soundManager.PlaySoundEffect(AudioSourceType.AttackEffects, attackSoundClips[(int)AttackState.SlowAttack]);
    }
    
    /// <summary>
    /// Play sound effect for throw knife.<br></br><br></br>
    /// Note: This is currently executed on its own by an Animation Event through the `Rogue_throw_knife` animation. 
    /// </summary>
    public void PlayThrowKnifeSfx()
    {
        if (!_soundManager)
            return;
        
        _soundManager.PlaySoundEffect(AudioSourceType.AttackEffects, attackSoundClips[(int)AttackState.ThrowKnife]);
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Set the current target that the AI is engaging.
    /// </summary>
    /// <param name="target">The target the AI is initiating combat with.</param>
    private void SetTarget(GameObject target)
    {
        Debug.Log("[EnemyCombat/SetTarget] Setting Target!");
        Target = target;
    }

    /// <summary>
    /// Reset the target that the AI was engaging.
    /// </summary>
    private void ResetTarget()
    {
        Debug.Log("[EnemyCombat/ResetTarget] Resetting Target!");
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
        Debug.Log("[EnemyCombat/ResetAllAttackTriggers] Resetting attack triggers!");
        _animator.ResetTrigger("LightAttack");
        _animator.ResetTrigger("SlowAttack");
        _animator.ResetTrigger("ThrowKnife");
    }
    
    /// <summary>
    /// Reset all combat states, including animator attack triggers and combat timers, for the AI.
    /// </summary>
    private void ResetCombatStates()
    {
        Debug.Log("[EnemyCombat/ResetCombatStates] Resetting combat states!");
        InCombat = false;
        ChaseTarget = false;
        AdvanceTarget = false;
        InvestigateEngagement = false;
        ResetAllAttackTriggers();
        ResetCombatTimers();
    }

    /// <summary>
    /// Reset all combat timers, includes melee, range, advance target, and stop advance target timer.
    /// </summary>
    private void ResetCombatTimers()
    {
        Debug.Log("[EnemyCombat/ResetCombatTimers] Resetting combat timers!");
        _meleeAttackTimer = initialMeleeAttackTime;
        _rangeAttackTimer = initialRangeAttackTime;
        _advanceTargetTimer = advanceTargetTime;
        _stopAdvanceTargetTimer = stopAdvanceTargetTime;
    }

    /// <summary>
    /// Setup sight detection box collider, includes size and offset of collider.
    /// </summary>
    private void SetupSightDetection()
    {
        _sightCollider2D = gameObject.transform.Find("SightDetection").GetComponent<BoxCollider2D>();

        if (!_sightCollider2D)
            return;
        
        _sightCollider2D.size = sightCollider2DSize;
        _sightCollider2D.offset = sightCollider2DOffset;
    }
    
    /// <summary>
    /// Setup melee weapons, includes sprites, tag, layer, and owner of left and right weapon.
    /// </summary>
    private void SetupMeleeWeapons()
    {
        SpriteRenderer meleeWeaponSpriteRenderer;
        MeleeWeapon meleeWeapon;

        if (meleeWeaponLeft)
        {
            meleeWeaponSpriteRenderer = meleeWeaponLeft.GetComponent<SpriteRenderer>();
            
            if (!meleeWeaponSpriteRenderer)
                return;
            
            meleeWeaponSpriteRenderer.sprite = meleeWeaponLeftSprite;
            
            meleeWeapon = meleeWeaponLeft.GetComponent<MeleeWeapon>();
            meleeWeapon.Owner = gameObject;
        }

        if (meleeWeaponRight)
        {
            meleeWeaponSpriteRenderer = meleeWeaponRight.GetComponent<SpriteRenderer>();
            
            if (!meleeWeaponSpriteRenderer)
                return;
            
            meleeWeaponSpriteRenderer.sprite = meleeWeaponRightSprite;
            
            meleeWeapon = meleeWeaponLeft.GetComponent<MeleeWeapon>();
            meleeWeapon.Owner = gameObject;
        }
    }
    
    #endregion
}