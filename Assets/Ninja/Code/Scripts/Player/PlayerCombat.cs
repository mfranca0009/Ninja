using UnityEngine;
using UnityEngine.InputSystem;

// TODO: Separate the attacks from base animation layer, add them to attack animation layer, allow attack animation layer
// to only trigger upper body movement by using an Avatar Mask and blending (Allows movement with attacking).

public class PlayerCombat : MonoBehaviour
{
    #region Public Properties
    
    public bool LightAttackPerformed { get; set; }
    public bool SlowAttackPerformed { get; set; }

    #endregion

    #region Public Fields
    
    [Tooltip("Light attack damage amount")] 
    public float lightAttackDmg = 12.5f;

    [Tooltip("heavy attack damage amount")] 
    public float heavyAttackDmg = 25f;
    
    #endregion
    
    #region Private Fields
    
    private PlayerInputActions _playerInputActions;
    private InputAction _lightAttackLeft;
    private InputAction _lightAttackRight;
    private InputAction _slowAttack;
    private InputAction _throwKnife;
    private Animator _animator;
    
    // Player Scripts
    private PlayerMovement _playerMovement;

    #endregion
    
    #region Unity Events
    
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
        SlowAttackPerformed = true;
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
        LightAttackPerformed = true;
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
        LightAttackPerformed = true;
    }

    private void OnLightAttackLeftCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("Light left-hand attack ended!");
    }
    
    #endregion
    
    #region Public Methods
    
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
        bool inAttackAnim = _animator.IsPlayingAnimation("Light Attack Left",
                                (int)AnimationLayers.BaseAnimLayer) ||
                            _animator.IsPlayingAnimation("Light Attack Right",
                                (int)AnimationLayers.BaseAnimLayer) ||
                            _animator.IsPlayingAnimation("Slow Attack",
                                (int)AnimationLayers.BaseAnimLayer) ||
                            _animator.IsPlayingAnimation("Rogue_attack_02",
                                (int)AnimationLayers.BaseAnimLayer);

        return inAttackAnim;
    }

    /// <summary>
    /// Retrieve the current attack state that is being performed.
    /// </summary>
    /// <returns>Returns an attack state from the AttackState enum [See Defines.cs]</returns>
    public AttackState GetAttackState()
    {
        AttackState attackState = AttackState.None;
        
        switch (_animator.IsPlayingAnimation("Light Attack Left", (int)AnimationLayers.BaseAnimLayer))
        {
            case true when _lightAttackLeft.inProgress || !_lightAttackLeft.inProgress:
            case false when _animator.IsPlayingAnimation("Light Attack Right",
                (int)AnimationLayers.BaseAnimLayer) && _lightAttackRight.inProgress || !_lightAttackRight.inProgress:
                attackState = AttackState.LightAttack;
                break;
            case false when _animator.IsPlayingAnimation("Slow Attack",
                (int)AnimationLayers.BaseAnimLayer) && _slowAttack.inProgress || !_slowAttack.inProgress:
                attackState = AttackState.SlowAttack;
                break;
            case false when _animator.IsPlayingAnimation("Rogue_attack_02",
                (int)AnimationLayers.BaseAnimLayer) && _throwKnife.inProgress || !_throwKnife.inProgress:
                attackState = AttackState.ThrowKnife;
                break;
        }

        return attackState;
    }
    
    #endregion
}
