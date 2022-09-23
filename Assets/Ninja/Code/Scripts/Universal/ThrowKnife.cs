using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowKnife : MonoBehaviour
{
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
        RotateFixedUpdate();
    }

    #endregion
    
    #region Fixed Update Methods
    
    private void HorizontalMoveFixedUpdate()
    {
        if (_rigidbody2D.velocity.x >= maximumXVelocity)
            return;

        _rigidbody2D.AddForce(new Vector2(xDistance * xSpeed, 0f), ForceMode2D.Force);
    }
    
    private void RotateFixedUpdate()
    {
        if (_rigidbody2D.angularVelocity >= maximumAngularVelocity)
            return;

        _rigidbody2D.AddTorque(_rigidbody2D.position.y * rotationSpeed, ForceMode2D.Force);
    }
    
    #endregion
}
