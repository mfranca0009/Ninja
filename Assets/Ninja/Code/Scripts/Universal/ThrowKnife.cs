using UnityEngine;

public class ThrowKnife : MonoBehaviour
{
    #region Public Properties
    
    public GameObject Owner { get; set; }
    public bool ThrowLeft { get; set; }
    
    #endregion
    
    #region Public Fields

    public float throwKnifeDmg = 18.75f;

    #endregion
    
    #region Serialized Fields

    [Header("Movement Settings")] 
    
    [Tooltip("The maximum X velocity threshold")] 
    [SerializeField] private float maximumXVelocity = 5f;
    
    [Tooltip("The distance the throwing knife will move horizontally")]
    [SerializeField] private float xDistance = 2f;
    
    [Tooltip("The speed the throwing knife will move horizontally")]
    [SerializeField] private float xSpeed = 5f;
    
    [Tooltip("The maximum X velocity threshold")] 
    [SerializeField] private float maximumYVelocity = 5f;
    
    [Tooltip("The distance the throwing knife will move horizontally")]
    [SerializeField] private float yDistance = 2f;
    
    [Tooltip("The speed the throwing knife will move horizontally")]
    [SerializeField] private float ySpeed = 5f;
    
    [Header("Rotation Settings")]
    
    [Tooltip("The maximum angular velocity threshold")]
    [SerializeField] private float maximumAngularVelocity = 5f;
    
    [Tooltip("The speed of rotation/torque")]
    [SerializeField] private float rotationSpeed = 5f;

    #endregion
    
    #region Private Fields
    
    private Rigidbody2D _rigidbody2D;

    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy)
            return;

        HorizontalMoveFixedUpdate();
        VerticalMoveFixedUpdate();
        RotateFixedUpdate();
    }

    #endregion
    
    #region Fixed Update Methods
    
    private void HorizontalMoveFixedUpdate()
    {
        if (Mathf.Abs(_rigidbody2D.velocity.x) >= maximumXVelocity)
            return;

        _rigidbody2D.AddForce(ThrowLeft ? new Vector2(-(xDistance * xSpeed), 0f) 
                : new Vector2(xDistance * xSpeed, 0f), ForceMode2D.Force);
    }

    private void VerticalMoveFixedUpdate()
    {
        if (Mathf.Abs(_rigidbody2D.velocity.x) >= maximumYVelocity)
            return;
        
        _rigidbody2D.AddForce(new Vector2(0f, yDistance * ySpeed), ForceMode2D.Force);
    }
    
    private void RotateFixedUpdate()
    {
        if (Mathf.Abs(_rigidbody2D.angularVelocity) >= maximumAngularVelocity)
            return;

        _rigidbody2D.AddTorque(ThrowLeft ? _rigidbody2D.position.y * -rotationSpeed :
                _rigidbody2D.position.y * rotationSpeed, ForceMode2D.Force);
    }
    
    #endregion
}
