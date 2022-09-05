using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO - fix jump animation and create one
// TODO - fix the "multi-jump" mechanic [primarily used for double-jump but open-ended just in case]
// TODO - Change attack cancels, move them from cancel callback to its own method and only call when needed.
// TODO - Cancel attack if start to move or jump for slow attack, get rid of "force" stand-in-place while attacking.
// TODO - Allow moving with light attacks and POSSIBLY medium attack (maybe just slow down velocity with light/medium attacks)

// TODO -
// If at any point we add ladders, we should allow the player to use up/down arrows to climb up or down the ladder.
// Jump is handled from the spacebar (default keybind) so pass 0 to the Vector2 Y plane value in MovementFixedUpdate()

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float walkSpeed = 100;
    [SerializeField]
    private float runSpeed = 150;
    [SerializeField] 
    private float jumpDistance = 1;
    [SerializeField] 
    private float jumpSpeed = 200;
    [SerializeField] 
    private int maxJumps = 2;
    
    private PlayerInputActions _playerInputActions;
    private InputAction _movement;
    private InputAction _jump;
    private InputAction _sprint;
    private InputAction _lightAttackLeft;
    private InputAction _lightAttackRight;
    private InputAction _slowAttack;
    private InputAction _mediumAttack;
    private Vector2 _movePos;
    private Vector2 _jumpPos;
    private Rigidbody2D _rigidBody;
    private Animator _animator;
    private bool _isGrounded;
    private bool _inAir;
    private int _jumpCount;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
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
        
        _mediumAttack = _playerInputActions.Player.MediumAttack;
        _mediumAttack.performed += OnMediumAttack;
        _mediumAttack.canceled += OnMediumAttackCancel;
        _mediumAttack.Enable();
    }

    private void OnDisable()
    {
        _movement.Disable();
        _jump.Disable();
        _sprint.Disable();
        _lightAttackLeft.Disable();
        _lightAttackRight.Disable();
        _slowAttack.Disable();
        _mediumAttack.Disable();
    }
    
    private void OnMediumAttack(InputAction.CallbackContext obj)
    {
        Debug.Log("Medium attack performed!");

        if (!CanAttack())
        {
            OnMediumAttackCancel(obj);
            return;
        }
        
        _animator.SetTrigger("MediumAttack");
    }

    private void OnMediumAttackCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Medium attack ended!");
        
        _animator.ResetTrigger("MediumAttack");
    }
    
    private void OnSlowAttack(InputAction.CallbackContext obj)
    {
        Debug.Log("Slow attack performed!");

        if (!CanAttack())
        {
            OnSlowAttackCancel(obj);
            return;
        }

        _animator.SetTrigger("SlowAttack");
    }

    private void OnSlowAttackCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Slow attack ended!");
        
        _animator.ResetTrigger("SlowAttack");
    }
    
    private void OnLightAttackRight(InputAction.CallbackContext obj)
    {
        Debug.Log("Light attack with right hand performed!");

        if (!CanAttack())
        {
            OnLightAttackRightCancel(obj);
            return;
        }
        
        _animator.SetTrigger("LightAttackRight");
    }

    private void OnLightAttackRightCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Light right-hand attack ended!");
        
        _animator.ResetTrigger("LightAttackRight");
    }
    
    private void OnLightAttackLeft(InputAction.CallbackContext obj)
    {
        Debug.Log("Light attack with left hand performed!");

        if (!CanAttack())
        {
            OnLightAttackLeftCancel(obj);
            return;
        }
        
        _animator.SetTrigger("LightAttackLeft");
    }

    private void OnLightAttackLeftCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Light left-hand attack ended!");
        
        _animator.ResetTrigger("LightAttackLeft");
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
        {
            OnMoveCancel(obj);
            return;   
        }

        Debug.Log("Moving!");
    }

    private void OnMoveCancel(InputAction.CallbackContext obj)
    {
        if (!IsMoving() || !_isGrounded)
            return;
        
        Debug.Log("Movement ended!");

        // zero out horizontal velocity
        _rigidBody.velocity = new Vector2(0, _rigidBody.velocity.y);
    }
    
    private void OnJump(InputAction.CallbackContext obj)
    {
        if (!CanJump())
        {
            OnJumpCancel(obj);
            return;
        }

        Debug.Log("Jumping!");
        
        if (_jumpCount < maxJumps)
            _jumpCount++;
    }

    private void OnJumpCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump ended!");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        // Update ground check every update.
        _isGrounded = _rigidBody.IsTouchingLayers(LayerMask.GetMask("Ground"));
        _animator.SetBool("IsGrounded", _isGrounded);
        if (_isGrounded && _inAir)
        {
            _inAir = false;
            _jumpCount = 0;
        }

        // Movement - only grab a move position if movement is in progress (key pressed / held down)
        if (_movement.inProgress)
            _movePos = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Jump - only grab a jump position if jump is in progress (key pressed)
        if (_jump.inProgress)
            _jumpPos = new Vector2(Input.GetAxisRaw("Horizontal"), jumpDistance);

        // Update velocity every update.
        _animator.SetFloat("Velocity", _rigidBody.velocity.magnitude);
        
        // Update sprint every update.
        _animator.SetBool("IsSprinting", _sprint.inProgress);
    }

    private void FixedUpdate()
    {
        MovementFixedUpdate();
        JumpFixedUpdate();
    }

    /// <summary>
    /// Update player movement [FixedUpdate]
    /// </summary>
    private void MovementFixedUpdate()
    {
        if (!_movement.inProgress)
            return;
        
        // Apply proper sprite facing direction based on move direction
        bool isFacingLeft = gameObject.transform.localScale.x < 0;
        if (_movement.ReadValue<Vector2>() == Vector2.left && !isFacingLeft ||
            _movement.ReadValue<Vector2>() == Vector2.right && isFacingLeft)
            FlipPlayerSprite();
        
        if (!CanMove())
            return;
        
        // Move player (walk or run speed) - check TODO comment at top of document
        Vector2 moveLeftRightPos = new Vector2(_movePos.x, 0);
        _rigidBody.velocity =
            moveLeftRightPos * ((_sprint.IsInProgress() ? runSpeed : walkSpeed) * Time.deltaTime);
    }
    
    private void JumpFixedUpdate()
    {
        if (!_jump.inProgress || !CanJump())
            return;

        // if (_jumpCount < maxJumps)
        //     _jumpCount++;
        
        Debug.Log($"Jumps performed: {_jumpCount}");
        
        _inAir = true;
        _rigidBody.velocity = _jumpPos * ( jumpSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Universal method to flip the player sprite either left or right by local x coordinate scale value
    /// depending on the sprite's current facing direction.
    /// </summary>
    private void FlipPlayerSprite()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x = -currentScale.x;
        gameObject.transform.localScale = currentScale;
    }

    /// <summary>
    /// Checks if the player is allowed to move
    /// </summary>
    /// <returns>Returns true if player is allowed to move, otherwise false.</returns>
    private bool CanMove()
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
                             _slowAttack.inProgress || _mediumAttack.inProgress;
        
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
    /// Checks if the player is moving, any time the player has velocity would indicate movement.
    /// </summary>
    /// <returns>Returns true if player is moving, otherwise false.</returns>
    private bool IsMoving()
    {
        return _rigidBody.velocity.magnitude > 0;
    }

    private void AttackCancel(string animationTriggerName)
    {
        _animator.ResetTrigger(animationTriggerName);
    }
}