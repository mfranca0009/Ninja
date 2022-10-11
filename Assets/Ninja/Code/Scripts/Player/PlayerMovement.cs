using UnityEngine;
using UnityEngine.InputSystem;

// TODO (WANT) -
// If at any point we add ladders, we should allow the player to use up/down arrows to climb up or down the ladder.
// Jump is handled from the spacebar (default keybind) so pass 0 to the Vector2 Y plane value in MovementFixedUpdate()
// for now.

public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Movement Speed Settings")]
    
    [Tooltip("Player walk movement speed")]
    [SerializeField] private float walkSpeed = 70f;
    
    [Tooltip("Player run movement speed")]
    [SerializeField] private float runSpeed = 120f;
    
    [Tooltip("The velocity the player will move upward at")]
    [SerializeField] private float jumpSpeed = 160f;
    
    [Header("Movement Distances")]
    
    [Tooltip("The distance to move the player upward")]
    [SerializeField] private float jumpDistance = 2.5f;

    [Header("Movement Restrictions")]
    
    [Tooltip("The amount of allowable jumps the player can use while in-air")]
    [SerializeField] private int maxJumps = 2;
    
    [Header("Movement Tolerances")]
    
    [Tooltip("Raycast distance for transitioning from in-air to land")]
    [SerializeField] private float landingTolerance = 0.003f;

    [Header("Animator Settings")]
    
    [Tooltip("Animator speed adjustment, which will be used for better sync of certain combat/movement animations")]
    [SerializeField] private float animatorSpeed = 1.25f;

    [Header("Audio Source Settings")]
    
    [Tooltip("Dedicated movement audio source for walk/run loop")]
    [SerializeField] private AudioSource movementAudioSource;

    [Tooltip("Normal pitch amount during walk speed for walk/run sound effect")] 
    [SerializeField] private float normalWalkPitch;

    [Tooltip("Fast pitch amount during run speed for walk/run sound effect")] 
    [SerializeField] private float fastRunPitch;
    
    [Tooltip("Dedicated movement audio source for simultaneous one shot sound effects")] 
    [SerializeField] private AudioSource movementOneShotAudioSource;
    
    [Header("Sound Effect Settings")]

    [Tooltip("Walk/Run sound effect to play")]
    [SerializeField] private AudioClip walkRunSoundClip;

    [Tooltip("Jump sound effect to play")]
    [SerializeField] private AudioClip jumpSoundClip;
    
    #endregion

    #region Public Field

    [Tooltip("Determines if the player's movement needs to be restricted for knockback")]
    public bool IncomingKnockback { get; set; }

    #endregion

    #region Private Fields

    // Input
    private PlayerInputActions _playerInputActions;
    private InputAction _movement;
    private InputAction _jump;
    private InputAction _sprint;
    
    // Animator / Animation
    private Animator _animator;
    
    // Rigidbody / Physics
    private Rigidbody2D _rigidBody2D;
    
    // Movement
    private Vector2 _movePos;

    // Input states
    private bool _moved;
    private bool _jumped;
    
    // Raycast landing
    private bool _isLanding;
    private bool _inAir;
    private BoxCollider2D _boxCollider2D;
    private Bounds _boxBounds;
    
    // Multi-jump system
    private bool _handledJump;
    private int _jumpCount;
    
    // Player Scripts
    private Health _health;
    private PlayerCombat _playerCombat;
    
    #endregion

    #region Unity Events
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _boxBounds = _boxCollider2D.bounds;

        _health = GetComponent<Health>();
        _playerCombat = GetComponent<PlayerCombat>();

        // Override default walk/run sound effect clip if one is present
        if (walkRunSoundClip)
            movementAudioSource.clip = walkRunSoundClip;
    }

    private void OnEnable()
    { _movement = _playerInputActions.Player.Movement;
        _movement.performed += OnMove;
        _movement.canceled += OnMoveCancel;
        _movement.Enable();

        _jump = _playerInputActions.Player.Jump;
        _jump.performed += OnJump;
        _jump.canceled += OnJumpCancel;
        _jump.Enable();

        _sprint = _playerInputActions.Player.Sprint;
        _sprint.performed += OnSprint;
        _sprint.canceled += OnSprintCancel;
        _sprint.Enable();
    }

    private void OnDisable()
    {
        _movement.Disable();
        _jump.Disable();
        _sprint.Disable();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_health.Dead)
            return;
        
        GroundCheckUpdate();
        MovementInputUpdate();
        AnimatorSpeedUpUpdate();

        // Update velocity every update.
        _animator.SetFloat("VelocityX", _rigidBody2D.velocity.x);
        _animator.SetFloat("VelocityY", _rigidBody2D.velocity.y);
        _animator.SetBool("HasVelocityX", _rigidBody2D.velocity.x != 0);

        // Update sprint every update.
        _animator.SetBool("IsSprinting", _sprint.inProgress);
    }

    private void FixedUpdate()
    {
        if (_health.Dead)
        {
            movementAudioSource.Pause();
            return;
        }

        MovementFixedUpdate();
        JumpFixedUpdate();
    }
    
    #endregion
    
    #region Input Callbacks
    
    private void OnSprint(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerMovement/OnSprint] Sprinting!");
    }

    private void OnSprintCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerMovement/OnSprintCancel] Sprint ended!");
    }
    
    private void OnMove(InputAction.CallbackContext obj)
    {
        if (!CanMove())
            return;

        Debug.Log("[PlayerMovement/OnMove] Moving!");
    }

    private void OnMoveCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerMovement/OnMoveCancel] Movement ended!");
        
        // Clear horizontal velocity
        if (IsMoving() && IsGrounded())
            _rigidBody2D.velocity = new Vector2(0, _rigidBody2D.velocity.y);
    }
    
    private void OnJump(InputAction.CallbackContext obj)
    {
        if (!CanJump())
            return;
        
        _jumpCount++;

        Debug.Log($"[PlayerMovement/OnJump] Jumping!\n Jumps Performed: {_jumpCount}");
    }

    private void OnJumpCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("[PlayerMovement/OnJumpCancel] Jump ended!");
        _handledJump = false;
    }
    
    #endregion

    #region Update Methods
    
    /// <summary>
    /// Update player ground state to determine if grounded, in-air, or landing.
    /// </summary>
    private void GroundCheckUpdate()
    {
        // Retrieve initial ground check
        bool isGrounded = IsGrounded();
        _animator.SetBool("IsGrounded", isGrounded);

        switch (isGrounded)
        {
            // Prevent the remaining fall velocity when landed so it does not trigger fall animation. 
            case true when IsFalling():
                _rigidBody2D.velocity = new Vector2(_rigidBody2D.velocity.x, 0f);
                break;
            case false:
                _inAir = true;
                break;
        }

        // Update box collider bounds, then proceed to raycast landing check.
        _boxBounds = _boxCollider2D.bounds;
        RaycastHit2D hitResult = Physics2D.BoxCast(_boxBounds.center, _boxBounds.size,
            0f, Vector2.down, landingTolerance, LayerMask.NameToLayer("Ground"));

        if (_inAir && !hitResult.collider)
            _isLanding = true;
        
        // Update animator landing state
        _animator.SetBool("IsLanding", _isLanding);
        
        if (!isGrounded)
            return;

        // From this point onward, execute clean-up from a complete jump (jump, in-air, land).
        
        // Clear horizontal velocity when landing from a jump.
        if (_inAir && _isLanding)
            _rigidBody2D.velocity = new Vector2(0, _rigidBody2D.velocity.y);

        // Reset
        _inAir = false;
        _jumpCount = 0;
        _isLanding = false;
    }

    /// <summary>
    /// Process player's movement input, movement info will be used to handle actual movement in FixedUpdate.
    /// </summary>
    private void MovementInputUpdate()
    {
        switch (_movement.inProgress)
        {
            case true when _jump.inProgress && !_handledJump:
                _moved = true;
                _jumped = true;
                _movePos = new Vector2(Input.GetAxisRaw("Horizontal"), jumpDistance);
                break;
            case false when _jump.inProgress && !_handledJump:
                _moved = false;
                _jumped = true;
                _movePos = new Vector2(Input.GetAxisRaw("Horizontal"), jumpDistance);
                break;
            case true when !_jump.inProgress:
                _moved = true;
                _jumped = false;
                _movePos = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                break;
            default:
                _moved = false;
                _jumped = false;
                _movePos = Vector2.zero;
                break;
        }
    }
    
    /// <summary>
    /// Update animator speed to better sync certain movement/combat animations
    /// </summary>
    private void AnimatorSpeedUpUpdate()
    {
        bool shouldUpdate =
            (_animator.IsPlayingAnimation("Walk", (int)AnimationLayers.BaseAnimLayer) &&
             _playerCombat.GetAttackState() == AttackState.LightAttack) ||
            (_animator.IsPlayingAnimation("Run", (int)AnimationLayers.BaseAnimLayer) &&
             _playerCombat.GetAttackState() == AttackState.ThrowKnife);

        if (_animator.speed <= 1f && !shouldUpdate)
            return;
        
        _animator.speed = shouldUpdate ? animatorSpeed : 1f;
    }
    
    #endregion
    
    #region Fixed Update Methods
    
    /// <summary>
    /// Handle player movement
    /// </summary>
    private void MovementFixedUpdate()
    {
        FlipPlayerSprite();
        
        if (!CanWalk())
            _moved = false;

        if (!_moved)
        {
            movementAudioSource.Pause();
            return;   
        }

        float moveSpeed = _sprint.IsInProgress() switch
        {
            true  => runSpeed,
            false  => walkSpeed
        };

        UpdateWalkRunSFXPitch(moveSpeed);
        
        _rigidBody2D.velocity = new Vector2(_movePos.x * moveSpeed, 0f) * Time.deltaTime;
        
        if (movementAudioSource.isPlaying)
            return;

        // Play movement sound effect
        movementAudioSource.Play();
    }
    
    /// <summary>
    /// Handle player jump movement
    /// </summary>
    private void JumpFixedUpdate()
    {
        if (!CanJump())
            _jumped = false;

        if (!_jumped)
            return;

        _handledJump = true;
        _rigidBody2D.velocity = new Vector2(_movePos.x, _movePos.y) * (jumpSpeed * Time.deltaTime);
        _animator.SetTrigger("Jump");

        // Play one shot jump sound effect
        movementOneShotAudioSource.PlayOneShot(jumpSoundClip, movementOneShotAudioSource.volume);
    }
    
    #endregion
    
    #region Public Helper Methods

    /// <summary>
    /// Checks if the player is grounded
    /// </summary>
    /// <returns>Returns true if player is grounded, otherwise false.</returns>
    public bool IsGrounded()
    {
        return _rigidBody2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }
    
    /// <summary>
    /// Checks if the player is moving.
    /// </summary>
    /// <returns>Returns true if player is moving, otherwise false.</returns>
    public bool IsMoving()
    {
        return _rigidBody2D.velocity.x != 0;
    }

    /// <summary>
    /// Checks if the player is jumping (positive velocity on Y axis)
    /// </summary>
    /// <returns>Returns true if player is jumping, otherwise false.</returns>
    public bool IsJumping()
    {
        return _rigidBody2D.velocity.y > 0;
    }

    /// <summary>
    /// Checks if the player is falling (negative velocity on Y axis)
    /// </summary>
    /// <returns>Returns true if player is falling, otherwise false.</returns>
    public bool IsFalling()
    {
        return _rigidBody2D.velocity.y < 0;
    }
    
    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Universal method to flip the player sprite either left or right by local x coordinate scale value
    /// depending on the sprite's current facing direction.
    /// </summary>
    private void FlipPlayerSprite()
    {
        bool isFacingLeft = transform.localScale.x < 0;
        
        if (_movement.ReadValue<Vector2>() == Vector2.zero ||
            (_movement.ReadValue<Vector2>() == Vector2.left && isFacingLeft) ||
            (_movement.ReadValue<Vector2>() == Vector2.right && !isFacingLeft) ||
            _playerCombat.GetAttackState() == AttackState.SlowAttack)
            return;
        
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
    }

    /// <summary>
    /// Determine pitch of movement sound effect based on current speed.
    /// <param name="currentSpeed">The gameobject's current speed</param>
    /// </summary>
    private void UpdateWalkRunSFXPitch(float currentSpeed)
    {
        if (movementAudioSource.pitch != normalWalkPitch && currentSpeed == walkSpeed)
            movementAudioSource.pitch = normalWalkPitch;
        else if (movementAudioSource.pitch != fastRunPitch && currentSpeed == runSpeed)
            movementAudioSource.pitch = fastRunPitch;
    }
    
    /// <summary>
    /// Checks if the player is allowed to move at all.
    /// </summary>
    /// <returns>Returns true if player is allowed to move, otherwise false.</returns>
    private bool CanMove()
    {
        return _playerCombat.GetAttackState() != AttackState.SlowAttack && !IncomingKnockback;
    }

    /// <summary>
    /// Checks if the player is allowed to walk.
    /// </summary>
    /// <returns>Returns true if player is allowed to walk, otherwise false.</returns>
    private bool CanWalk()
    {
        return _playerCombat.GetAttackState() != AttackState.SlowAttack && IsGrounded() && !IncomingKnockback;
    }
    
    /// <summary>
    /// Checks if the player is allowed to jump
    /// </summary>
    /// <returns>Returns true if player is allowed to jump, otherwise false.</returns>
    private bool CanJump()
    {
        return _playerCombat.GetAttackState() != AttackState.SlowAttack && _jumpCount < maxJumps && !IncomingKnockback;
    }

    #endregion
}
