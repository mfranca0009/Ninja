using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    #region Public Properties
    
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
    
    /// <summary>
    /// The maximum amount of knives that can be active at one time within the scene.
    /// </summary>
    public int MaxKnives { get; set; }
    
    /// <summary>
    /// The amount of active knives within the scene. The counter will be decremented when a knife is destroyed
    /// from the scene. A knife can be destroyed when leaving camera viewport, hitting the ground or platform, or
    /// hitting an enemy and damaging them.
    /// </summary>
    public int ActiveKnives { get; set; }
    
    /// <summary>
    /// Whether the player has an active melee strength boost or not.
    /// </summary>
    public bool HasMeleeStrengthBoost { get; set; }
    
    /// <summary>
    /// Timer that will tick when a melee strength boost is active.
    /// </summary>
    public float StrengthBoostTimer { get; set; }
    
    #endregion

    #region Serialized Fields

    [Header("Throwing Knife Settings")]
    
    [Tooltip("The knife prefab that will be instantiated into the scene")]
    [SerializeField] private GameObject throwKnifePrefab;
    
    [Tooltip("The knife sprite that will be visible when the knife is active within the scene")]
    [SerializeField] private Sprite throwKnifeSprite;
    
    [Tooltip("The transform the knife will use as a starting spawn position")]
    [SerializeField] private Transform throwKnifeSpawnTransform;

    [Tooltip("A position offset to adjust spawn positioning further in relation to the spawn transform used")]
    [SerializeField] private Vector2 throwKnifeSpawnOffset = new (1f, 0f);

    #endregion

    [Header("Melee Weapon Settings")] 
    
    [Tooltip("The left melee weapon gameobject")] 
    public GameObject meleeWeaponLeft;
    
    [Tooltip("The right melee weapon gameobject")] 
    public GameObject meleeWeaponRight;
    
    [Tooltip("The left melee weapon sprite that will be visible when the weapon is active within the scene")]
    [SerializeField] private Sprite meleeWeaponLeftSprite;
    
    [Tooltip("The right melee weapon sprite that will be visible when the weapon is active within the scene")]
    [SerializeField] private Sprite meleeWeaponRightSprite;
    
    #region Private Fields
    
    // Input
    private PlayerInputActions _playerInputActions;
    private InputAction _throwKnife;
    private InputAction _lightAttack;
    private InputAction _slowAttack;

    // Animator / animations
    private Animator _animator;

    // Player Scripts
    private PlayerMovement _playerMovement;

    #endregion
    
    #region Unity Events
    
    private void OnEnable()
    {
        _lightAttack = _playerInputActions.Player.LightAttack;
        _lightAttack.performed += OnLightAttack;
        _lightAttack.canceled += OnLightAttackCancel;
        _lightAttack.Enable();
        
        _slowAttack = _playerInputActions.Player.SlowAttack;
        _slowAttack.performed += OnSlowAttack;
        _slowAttack.canceled += OnSlowAttackCancel;
        _slowAttack.Enable();
        
        _throwKnife = _playerInputActions.Player.ThrowKnife;
        _throwKnife.performed += OnThrowKnife;
        _throwKnife.canceled += OnThrowKnifeCancel;
        _throwKnife.Enable();
    }

    private void OnDisable()
    {
        _lightAttack.Disable();
        _slowAttack.Disable();
        _throwKnife.Disable();
    }

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        MaxKnives = 1;
        
        SetupMeleeWeapons();
    }

    private void Update()
    {
        StrengthBoostUpdate();
        _animator.SetFloat("MaxKnives", MaxKnives);
    }
    
    #endregion
    
    #region Input Callbacks
    
    // Note: Animation event is configured through the throwing knife animation. Triggering the animation will also
    // trigger `InstantiateThrowingKnife` at the proper frame when the rogue's dagger has been disabled replicating
    // it has been thrown.
    private void OnThrowKnife(InputAction.CallbackContext obj)
    {
        if (!CanThrowKnife())
            return;

        Debug.Log("[PlayerCombat/OnThrowKnife] Knife throw performed!");
        ActiveKnives++;
        
        _animator.SetTrigger("ThrowKnife");
    }

    private void OnThrowKnifeCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerCombat/OnThrowKnifeCancel] Knife throw ended!");
    }
    
    private void OnSlowAttack(InputAction.CallbackContext obj)
    {
        if (!CanAttack() || !_playerMovement.IsGrounded())
            return;

        Debug.Log("[PlayerCombat/OnSlowAttack] Slow attack performed!");
        
        _animator.SetTrigger("SlowAttack");
    }

    private void OnSlowAttackCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerCombat/OnSlowAttackCancel] Slow attack ended!");
    }
    
    private void OnLightAttack(InputAction.CallbackContext obj)
    {
        if (!CanAttack())
            return;

        Debug.Log("[PlayerCombat/OnLightAttack] Light attack performed!");
        
        _animator.SetTrigger("LightAttack");
    }

    private void OnLightAttackCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerCombat/OnLightAttackCancel] Light attack ended!");
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Update strength boost timer, the strength boost expires once the timer expires.
    /// </summary>
    private void StrengthBoostUpdate()
    {
        if (!HasMeleeStrengthBoost)
            return;

        if (StrengthBoostTimer <= 0)
        {
            HasMeleeStrengthBoost = false;
            MeleeWeapon meleeWeapon;
            
            if (meleeWeaponLeft)
            {
                meleeWeapon = meleeWeaponLeft.GetComponent<MeleeWeapon>();

                if (!meleeWeapon)
                    return;
                
                meleeWeapon.LightAttackDmg = meleeWeapon.baseLightAttackDmg;
                meleeWeapon.HeavyAttackDmg = meleeWeapon.baseHeavyAttackDmg;
            }

            if (meleeWeaponRight)
            {
                meleeWeapon = meleeWeaponRight.GetComponent<MeleeWeapon>();
                
                if (!meleeWeapon)
                    return;
                
                meleeWeapon.LightAttackDmg = meleeWeapon.baseLightAttackDmg;
                meleeWeapon.HeavyAttackDmg = meleeWeapon.baseHeavyAttackDmg;
            }
        }
        else
            StrengthBoostTimer -= Time.deltaTime;
    }

    #endregion
    
    #region Public Helper Methods
    
    /// <summary>
    /// Checks if the player is in an attack state, pressing an attack key.
    /// </summary>
    /// <returns>Returns true if player is pressing an attack key, otherwise false.</returns>
    public bool IsInAttackState()
    {
        bool inAttackState = _lightAttack.inProgress || _slowAttack.inProgress || _throwKnife.inProgress;
        
        return inAttackState;
    }

    /// <summary>
    /// Checks if the player is in an attack animation, if the player is processing an attack.
    /// </summary>
    /// <returns>Returns true if player is playing an attack animation, otherwise false.</returns>
    public bool IsInAttackAnim()
    {
        bool inAttackAnim = _animator.IsPlayingAnimation("Light Attack",
                                (int)AnimationLayers.AttackAnimLayer) ||
                            _animator.IsPlayingAnimation("Slow Attack",
                                (int)AnimationLayers.AttackAnimLayer) ||
                            _animator.IsPlayingAnimation("Throw Knife",
                                (int)AnimationLayers.AttackAnimLayer);

        return inAttackAnim;
    }

    /// <summary>
    /// Retrieve the current attack state that is being performed.
    /// </summary>
    /// <returns>Returns an attack state from the AttackState enum [See Defines.cs]</returns>
    public AttackState GetAttackState()
    {
        AttackState attackState =
            _animator.IsPlayingAnimation("Light Attack", (int)AnimationLayers.AttackAnimLayer) switch
            {
                true when _lightAttack.inProgress || !_lightAttack.inProgress => AttackState.LightAttack,
                false when _animator.IsPlayingAnimation("Slow Attack", (int)AnimationLayers.AttackAnimLayer) &&
                    (_slowAttack.inProgress || !_slowAttack.inProgress) => AttackState.SlowAttack,
                false when _animator.IsPlayingAnimation("Throw Knife", (int)AnimationLayers.AttackAnimLayer) &&
                    (_throwKnife.inProgress || !_throwKnife.inProgress) => AttackState.ThrowKnife,
                _ => AttackState.None
            };

        return attackState;
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
        knifeToCreate.transform.Find("Hitbox").gameObject.layer = LayerMask.NameToLayer("PlayerRangeAttack");

        knifeToCreate.transform.position +=
            new Vector3(throwKnife.ThrowLeft ? -throwKnifeSpawnOffset.x + -0.5f : throwKnifeSpawnOffset.x,
                throwKnifeSpawnOffset.y, 0f);

        // Activate throwing knife object
        knifeRigidBody2D.bodyType = RigidbodyType2D.Dynamic;
        knifeToCreate.SetActive(true);
    }
    
    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Checks if the player is allowed to attack
    /// </summary>
    /// <returns>Returns true if player is allowed to attack, otherwise false.</returns>
    private bool CanAttack()
    {
        return IsInAttackState() && !IsInAttackAnim();
    }
    
    /// <summary>
    /// Checks if the player is allowed to throw a knife
    /// </summary>
    /// <returns>Returns true if player is allowed to throw a knife, otherwise false.</returns>
    private bool CanThrowKnife()
    {
        return IsInAttackState() && ActiveKnives != MaxKnives;
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

            meleeWeapon = meleeWeaponRight.GetComponent<MeleeWeapon>();
            meleeWeapon.Owner = gameObject;
        }
    }
    
    #endregion
}