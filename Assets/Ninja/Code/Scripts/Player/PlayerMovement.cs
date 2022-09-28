using UnityEngine;
using UnityEngine.InputSystem;

// TODO - CanMove() is technically useless right now, unless we were to implement a stun effect. Remove if not needed.
// TODO - Cancel attack if start to move or jump for slow attack, get rid of "force" stand-in-place while attacking.
// TODO - Allow moving with light attacks (maybe just slow down velocity when performing light attacks)

// TODO -
// If at any point we add ladders, we should allow the player to use up/down arrows to climb up or down the ladder.
// Jump is handled from the spacebar (default keybind) so pass 0 to the Vector2 Y plane value in MovementFixedUpdate()
// for now.

public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Movement Speeds")]
    [Tooltip("Player walk speed")]
    [SerializeField] private float walkSpeed = 70f;
    
    [Tooltip("Player run speed")]
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
    
    #endregion
    
    #region Private Fields
    
    private PlayerInputActions _playerInputActions;
    private InputAction _movement;
    private InputAction _jump;
    private InputAction _sprint;
    private Vector2 _movePos;
    private Rigidbody2D _rigidBody2D;
    private Animator _animator;
    private BoxCollider2D _boxCollider2D;
    private Bounds _boxBounds;
    private bool _moved;
    private bool _jumped;
    private bool _handledJump;
    private bool _isLanding;
    private bool _inAir;
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
        
        // Update ground check every update.
        GroundCheckUpdate();
        
        // Update player movement input.
        MovementInputUpdate();
        
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
            return;
        
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
    
    #endregion
    
    #region Fixed Update Methods
    
    /// <summary>
    /// Handle player movement
    /// </summary>
    private void MovementFixedUpdate()
    {
        // Flip sprite to correct facing direction regardless if they can actually move.
        // For instance, flip sprite while in-air.
        FlipPlayerSprite();
        
        if (!CanWalk())
            _moved = false;

        if (!_moved)
            return;
        
        _rigidBody2D.velocity = new Vector2(_movePos.x * (_sprint.IsInProgress() ? runSpeed : walkSpeed), 0f) *
                              Time.deltaTime;
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
            _movement.ReadValue<Vector2>() == Vector2.left && isFacingLeft ||
            _movement.ReadValue<Vector2>() == Vector2.right && !isFacingLeft ||
            _playerCombat.IsInAttackAnim() || _playerCombat.IsInAttackState())
            return;
        
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
    }
    
    /// <summary>
    /// Checks if the player is allowed to move at all.
    /// </summary>
    /// <returns>Returns true if player is allowed to move, otherwise false.</returns>
    private bool CanMove()
    {
        return !_playerCombat.IsInAttackState() && !_playerCombat.IsInAttackAnim();
    }

    /// <summary>
    /// Checks if the player is allowed to walk.
    /// </summary>
    /// <returns>Returns true if player is allowed to walk, otherwise false.</returns>
    private bool CanWalk()
    {
        return !_playerCombat.IsInAttackState() && !_playerCombat.IsInAttackAnim() && IsGrounded();
    }
    
    /// <summary>
    /// Checks if the player is allowed to jump
    /// </summary>
    /// <returns>Returns true if player is allowed to jump, otherwise false.</returns>
    private bool CanJump()
    {
        return !_playerCombat.IsInAttackState() && !_playerCombat.IsInAttackAnim() && _jumpCount < maxJumps;
    }

    #endregion
}
