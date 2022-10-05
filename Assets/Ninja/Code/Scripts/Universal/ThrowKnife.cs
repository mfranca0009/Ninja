using UnityEngine;

public class ThrowKnife : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The owner who has thrown the knife.
    /// </summary>
    public GameObject Owner { get; set; }
    
    /// <summary>
    /// The state in which the knife was thrown while facing left or not.
    /// </summary>
    public bool ThrowLeft { get; set; }
    
    #endregion
    
    #region Public Fields

    [Header("Damage Settings")]
    
    [Tooltip("Throwing knife damage that will be applied when hitting a target")]
    public float throwKnifeDmg = 18.75f;

    #endregion
    
    #region Serialized Fields

    [Header("Horizontal Movement Settings")] 
    
    [Tooltip("The maximum X velocity threshold")] 
    [SerializeField] private float maximumXVelocity = 15f;
    
    [Tooltip("The distance the throwing knife will move horizontally")]
    [SerializeField] private float xDistance = 10f;
    
    [Tooltip("The speed the throwing knife will move horizontally")]
    [SerializeField] private float xSpeed = 20f;
    
    [Header("Vertical Movement Settings")]
    
    [Tooltip("The maximum X velocity threshold")] 
    [SerializeField] private float maximumYVelocity = 10f;
    
    [Tooltip("The distance the throwing knife will move horizontally")]
    [SerializeField] private float yDistance = 3f;
    
    [Tooltip("The speed the throwing knife will move horizontally")]
    [SerializeField] private float ySpeed = 15f;
    
    [Header("Rotation Movement Settings")]
    
    [Tooltip("The maximum angular velocity threshold")]
    [SerializeField] private float maximumAngularVelocity = 180f;
    
    [Tooltip("The speed of rotation/torque")]
    [SerializeField] private float rotationSpeed = 150f;

    [Header("Sound Effect Settings")] 
    
    [Tooltip("Throwing knife spin sound effect")] 
    [SerializeField] private AudioClip knifeSpinSoundClip;
    
    #endregion
    
    #region Private Fields
    
    // Rigidbody / Physics
    private Rigidbody2D _rigidbody2D;

    // Sound Effects / Music
    private SoundManager _soundManager;
    private bool _spinSfxPlayed;
    
    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _soundManager = FindObjectOfType<SoundManager>();
    }

    private void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (!_spinSfxPlayed)
        {
            _spinSfxPlayed = true;
            _soundManager.PlaySoundEffect(AudioSourceType.AttackEffects, knifeSpinSoundClip);
        }
        
        HorizontalMoveFixedUpdate();
        VerticalMoveFixedUpdate();
        RotateFixedUpdate();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        UpdateActiveKnives();
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        UpdateActiveKnives();
        Destroy(gameObject);
    }

    #endregion
    
    #region Fixed Update Methods
    
    /// <summary>
    /// Horizontal (X-axis) movement fixed update
    /// </summary>
    private void HorizontalMoveFixedUpdate()
    {
        if (Mathf.Abs(_rigidbody2D.velocity.x) >= maximumXVelocity)
            return;

        _rigidbody2D.AddForce(ThrowLeft ? new Vector2(-(xDistance * xSpeed), 0f) 
                : new Vector2(xDistance * xSpeed, 0f), ForceMode2D.Force);
    }

    /// <summary>
    /// Vertical (Y-axis) movement fixed update
    /// </summary>
    private void VerticalMoveFixedUpdate()
    {
        if (Mathf.Abs(_rigidbody2D.velocity.x) >= maximumYVelocity)
            return;
        
        _rigidbody2D.AddForce(new Vector2(0f, yDistance * ySpeed), ForceMode2D.Force);
    }
    
    /// <summary>
    /// Rotation/Torque movement fixed update
    /// </summary>
    private void RotateFixedUpdate()
    {
        if (Mathf.Abs(_rigidbody2D.angularVelocity) >= maximumAngularVelocity)
            return;

        _rigidbody2D.AddTorque(ThrowLeft ? _rigidbody2D.position.y * -rotationSpeed :
                _rigidbody2D.position.y * rotationSpeed, ForceMode2D.Force);
    }
    
    #endregion
    
    #region Public Helper Methods

    /// <summary>
    /// Decrements the active knife counter for the player, if the owner of the knife was the player.
    /// </summary>
    public void UpdateActiveKnives()
    {
        if (Owner.layer != LayerMask.NameToLayer("Player"))
            return;

        PlayerCombat playerCombat = Owner.GetComponent<PlayerCombat>();

        if (!playerCombat || playerCombat.ActiveKnives < 1)
            return;
        
        playerCombat.ActiveKnives--;
    }
    
    #endregion
}