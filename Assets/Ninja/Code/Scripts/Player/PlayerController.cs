using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO - CanMove() is technically useless right now, unless we were to implement a stun effect. Remove if not needed.
// TODO - Cancel attack if start to move or jump for slow attack, get rid of "force" stand-in-place while attacking.
// TODO - Allow moving with light attacks (maybe just slow down velocity when performing light attacks)
// TODO - Implement falling behavior/logic (no jump used, but simply falling from a platform)

// TODO -
// If at any point we add ladders, we should allow the player to use up/down arrows to climb up or down the ladder.
// Jump is handled from the spacebar (default keybind) so pass 0 to the Vector2 Y plane value in MovementFixedUpdate()
// for now.

public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Speeds")]
    [Tooltip("Player walk speed")]
    [SerializeField] private float walkSpeed = 70f;
    
    [Tooltip("Player run speed")]
    [SerializeField] private float runSpeed = 120f;
    
    [Tooltip("The velocity the player will move upward at")]
    [SerializeField] private float jumpSpeed = 150f;
    
    [Header("Movement Distances")]
    [Tooltip("The distance to move the player upward")]
    [SerializeField] private float jumpDistance = 2f;

    [Header("Movement Restrictions")]
    [Tooltip("The amount of allowable jumps the player can use while in-air")]
    [SerializeField] private int maxJumps = 2;

    [Header("Movement Tolerances")]
    [Tooltip("Raycast distance for transitioning from in-air to land")]
    [SerializeField] private float landingTolerance = 0.006f;
    #endregion
    
    #region Private Fields
    private PlayerInputActions _playerInputActions;
    private InputAction _movement;
    private InputAction _jump;
    private InputAction _sprint;
    private InputAction _lightAttackLeft;
    private InputAction _lightAttackRight;
    private InputAction _slowAttack;
    private InputAction _throwKnife;
    private Vector2 _movePos;
    private Rigidbody2D _rigidBody;
    private Animator _animator;
    private BoxCollider2D _boxCollider;
    private Bounds _boxBounds;
    private bool _moved;
    private bool _jumped;
    private bool _handledJump;
    private bool _isGrounded;
    private bool _isLanding;
    private bool _inAir;
    private int _jumpCount;
    #endregion

    #region Unity Events
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxBounds = _boxCollider.bounds;
    }

    private void OnEnable()
    {
        _movement = _playerInputActions.Player.Movement;
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

        _lightAttackLeft = _playerInputActions.Player.LightAttackLeft;
        _lightAttackLeft.performed += OnLightAttackLeft;
        _lightAttackLeft.canceled += OnLightAttackLeftCancel;
        _lightAttackLeft.Enable();
        
        _lightAttackRight = _playerInputActions.Player.LightAttackRight;
        _lightAttackRight.performed += OnLightAttackRight;
        _lightAttackRight.canceled += OnLightAttackRightCancel;
        _lightAttackRight.Enable();
        
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
        _movement.Disable();
        _jump.Disable();
        _sprint.Disable();
        _lightAttackLeft.Disable();
        _lightAttackRight.Disable();
        _slowAttack.Disable();
        _throwKnife.Disable();
    }

    // Update is called once per frame
    private void Update()
    {
        // Update ground check every update.
        GroundCheckUpdate();
        
        // Update player movement input.
        MovementInputUpdate();
        
        // Update velocity every update.
        _animator.SetFloat("VelocityX", _rigidBody.velocity.x);
        _animator.SetFloat("VelocityY", _rigidBody.velocity.y);
        _animator.SetBool("HasVelocityX", _rigidBody.velocity.x != 0);

        // Update sprint every update.
        _animator.SetBool("IsSprinting", _sprint.inProgress);
    }

    private void FixedUpdate()
    {
        MovementFixedUpdate();
        JumpFixedUpdate();
    }
    #endregion
    
    #region Input Callbacks
    private void OnThrowKnife(InputAction.CallbackContext obj)
    {
        Debug.Log("Knife throw performed!");

        if (!CanAttack())
            return;

        _animator.SetTrigger("ThrowKnife");
    }

    private void OnThrowKnifeCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Knife throw ended!");
    }
    
    private void OnSlowAttack(InputAction.CallbackContext obj)
    {
        Debug.Log("Slow attack performed!");

        if (!CanAttack())
            return;

        _animator.SetTrigger("SlowAttack");
    }

    private void OnSlowAttackCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Slow attack ended!");
    }
    
    private void OnLightAttackRight(InputAction.CallbackContext obj)
    {
        Debug.Log("Light attack with right hand performed!");

        if (!CanAttack())
            return;

        _animator.SetTrigger("LightAttackRight");
    }

    private void OnLightAttackRightCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Light right-hand attack ended!");
    }
    
    private void OnLightAttackLeft(InputAction.CallbackContext obj)
    {
        Debug.Log("Light attack with left hand performed!");

        if (!CanAttack())
            return;

        _animator.SetTrigger("LightAttackLeft");
    }

    private void OnLightAttackLeftCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Light left-hand attack ended!");
    }
    
    private void OnSprint(InputAction.CallbackContext obj)
    {
        Debug.Log("Sprinting!");
    }

    private void OnSprintCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Sprint ended!");
    }
    
    private void OnMove(InputAction.CallbackContext obj)
    {
        if (!CanMove())
            return;

        Debug.Log("Moving!");
    }

    private void OnMoveCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Movement ended!");
        
        // Clear horizontal velocity
        if (IsMoving() && _isGrounded)
            _rigidBody.velocity = new Vector2(0, _rigidBody.velocity.y);
    }
    
    private void OnJump(InputAction.CallbackContext obj)
    {
        if (!CanJump())
            return;
        
        _jumpCount++;

        Debug.Log($"Jumping!\n Jumps Performed: {_jumpCount}");
    }

    private void OnJumpCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump ended!");
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
        _isGrounded = _rigidBody.IsTouchingLayers(LayerMask.GetMask("Ground"));
        _animator.SetBool("IsGrounded", _isGrounded);

        if (!_isGrounded)
            _inAir = true;
        
        // Update box collider bounds, then proceed to raycast landing check.
        _boxBounds = _boxCollider.bounds;
        RaycastHit2D hitResult = Physics2D.BoxCast(_boxBounds.center, _boxBounds.size,
            0f, Vector2.down, landingTolerance, LayerMask.NameToLayer("Ground"));
            
        if (hitResult.collider != null)
            _isLanding = true;
        
        // Update animator landing state
        _animator.SetBool("IsLanding", _isLanding);
        
        if (!_isGrounded)
            return;

        // From this point onward, execute clean-up from a complete jump (jump, in-air, land).
        
        // Clear horizontal velocity when landing from a jump.
        if (_inAir && _isLanding)
            _rigidBody.velocity = new Vector2(0, _rigidBody.velocity.y);
        
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
        
        _rigidBody.velocity = new Vector2(_movePos.x * (_sprint.IsInProgress() ? runSpeed : walkSpeed), 0f) *
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
        _rigidBody.velocity = new Vector2(_movePos.x, _movePos.y) * (jumpSpeed * Time.deltaTime);
        _animator.SetTrigger("Jump");
    }
    #endregion
    
    #region Helper Methods
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
            IsInAttackAnim() || IsInAttackState())
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
        return !IsInAttackState() && !IsInAttackAnim();
    }

    /// <summary>
    /// Checks if the player is allowed to walk.
    /// </summary>
    /// <returns>Returns true if player is allowed to walk, otherwise false.</returns>
    private bool CanWalk()
    {
        return !IsInAttackState() && !IsInAttackAnim() && _isGrounded;
    }
    
    /// <summary>
    /// Checks if the player is allowed to jump
    /// </summary>
    /// <returns>Returns true if player is allowed to jump, otherwise false.</returns>
    private bool CanJump()
    {
        return !IsInAttackState() && !IsInAttackAnim() && _jumpCount < maxJumps;
    }
    
    /// <summary>
    /// Checks if the player is allowed to attack
    /// </summary>
    /// <returns>Returns true if player is allowed to attack, otherwise false.</returns>
    private bool CanAttack()
    {
        return IsInAttackState() && !IsMoving() && !IsInAttackAnim() && _isGrounded;
    }

    /// <summary>
    /// Checks if the player is in an attack state, pressing an attack key.
    /// </summary>
    /// <returns>Returns true if player is pressing an attack key, otherwise false.</returns>
    private bool IsInAttackState()
    {
        bool inAttackState = _lightAttackLeft.inProgress || _lightAttackRight.inProgress ||
                             _slowAttack.inProgress || _throwKnife.inProgress;
        
        return inAttackState;
    }

    /// <summary>
    /// Checks if the player is in an attack animation, if the player is processing an attack.
    /// </summary>
    /// <returns>Returns true if player is playing an attack animation, otherwise false.</returns>
    private bool IsInAttackAnim()
    {
        bool inAttackAnim = _animator.IsPlayingAnimation("Rogue_attack_01",
                                (int)AnimationLayers.BaseAnimLayer) ||
                            _animator.IsPlayingAnimation("Rogue_attack_02",
                                (int)AnimationLayers.BaseAnimLayer) ||
                            _animator.IsPlayingAnimation("Rogue_attack_03",
                                (int)AnimationLayers.BaseAnimLayer) ||
                            _animator.IsPlayingAnimation("Rogue_hit_01",
                                (int)AnimationLayers.BaseAnimLayer);

        return inAttackAnim;
    }

    /// <summary>
    /// Checks if the player is moving.
    /// </summary>
    /// <returns>Returns true if player is moving, otherwise false.</returns>
    private bool IsMoving()
    {
        return _rigidBody.velocity.x != 0;
    }

    /// <summary>
    /// Checks if the player is jumping (positive velocity on Y axis)
    /// </summary>
    /// <returns>Returns true if player is jumping, otherwise false.</returns>
    private bool IsJumping()
    {
        return _rigidBody.velocity.y > 0;
    }

    /// <summary>
    /// Checks if the player is falling (negative velocity on Y axis)
    /// </summary>
    /// <returns>Returns true if player is falling, otherwise false.</returns>
    private bool IsFalling()
    {
        return _rigidBody.velocity.y < 0;
    }
    #endregion
}