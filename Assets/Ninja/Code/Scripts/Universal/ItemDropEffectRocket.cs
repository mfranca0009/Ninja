using UnityEngine;

public class ItemDropEffectRocket : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The distance to move the target horizontally 
    /// </summary>
    public float HorizontalDistance { get; set; } = 2f;
    
    #endregion
    
    #region Serialized Fields
    [Tooltip("Enable rotate effect")]
    [SerializeField] private bool rotate = true;
    
    [Tooltip("The distance to move the target upward")]
    [SerializeField] private float rocketDistance = 2f;
    
    [Tooltip("The speed the target will move upward at")]
    [SerializeField] private float rocketSpeed = 4f;

    [Tooltip("The speed the target will move horizontally")] 
    [SerializeField] private float horizontalSpeed = 4f;
    
    [Tooltip("The speed the target will rotate at")]
    [SerializeField] private float rotationSpeed = 2f;
    #endregion

    #region Private Fields
    private Rigidbody2D _rigidbody2D;
    private bool _impulseApplied;
    private bool _applyImpulse;
    private bool _torqueApplied;
    private bool _applyTorque;
    #endregion
    
    #region Unity Events
    // Start is called before the first frame update
    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _impulseApplied = false;
        _torqueApplied = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isActiveAndEnabled)
            return;
        
        if(!_impulseApplied)
            _applyImpulse = true;

        if (!rotate)
            return;
        
        if (!_torqueApplied)
            _applyTorque = true;
    }

    private void FixedUpdate()
    {
        RocketEffectFixedUpdate();
        RotationEffectFixedUpdate();
    }
    #endregion

    #region Fixed Update Methods
    /// <summary>
    /// Apply upward force, like a rocket, onto the gameobject
    /// </summary>
    private void RocketEffectFixedUpdate()
    {
        if (!_applyImpulse || _impulseApplied)
            return;

        _applyImpulse = false;
        _impulseApplied = true;
        _rigidbody2D.AddForce(new Vector2(HorizontalDistance * horizontalSpeed, rocketDistance * rocketSpeed),
            ForceMode2D.Impulse);
    }

    /// <summary>
    /// Apply torque to make the gameobject spin
    /// </summary>
    private void RotationEffectFixedUpdate()
    {
        if (!_applyTorque || _torqueApplied)
            return;

        _applyTorque = false;
        _torqueApplied = true;
        _rigidbody2D.AddTorque(_rigidbody2D.position.y * rotationSpeed, ForceMode2D.Impulse);
    }
    #endregion
}
