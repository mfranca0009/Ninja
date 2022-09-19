using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private PlayerInputActions _playerInputActions; 
    private InputAction _lightAttackLeft;
    private InputAction _lightAttackRight;
    private InputAction _slowAttack;
    private InputAction _throwKnife;
    private Animator _animator;
    
    // Player Scripts
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _animator = GetComponent<Animator>();

        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
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
        _lightAttackLeft.Disable();
        _lightAttackRight.Disable();
        _slowAttack.Disable();
        _throwKnife.Disable();
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
    
    }
    
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
    
    /// <summary>
    /// Checks if the player is allowed to attack
    /// </summary>
    /// <returns>Returns true if player is allowed to attack, otherwise false.</returns>
    public bool CanAttack()
    {
        return IsInAttackState() && !_playerMovement.IsMoving() && !IsInAttackAnim() && _playerMovement.IsGrounded();
    }

    /// <summary>
    /// Checks if the player is in an attack state, pressing an attack key.
    /// </summary>
    /// <returns>Returns true if player is pressing an attack key, otherwise false.</returns>
    public bool IsInAttackState()
    {
        bool inAttackState = _lightAttackLeft.inProgress || _lightAttackRight.inProgress ||
                             _slowAttack.inProgress || _throwKnife.inProgress;
        
        return inAttackState;
    }

    /// <summary>
    /// Checks if the player is in an attack animation, if the player is processing an attack.
    /// </summary>
    /// <returns>Returns true if player is playing an attack animation, otherwise false.</returns>
    public bool IsInAttackAnim()
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
}
