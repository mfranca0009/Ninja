using UnityEngine;

public class NoLand : MonoBehaviour
{
    #region Serialized Field
    
    [Header("Force Settings")]
    
    [Tooltip("Given force to be used when landing on top of the object")]
    [SerializeField] private Vector2 force = new (5, 5);
    
    [Tooltip("The force mode to be used when landing on top of the object")]
    [SerializeField] private ForceMode2D forceMode2D = ForceMode2D.Impulse;

    [Header("Tolerance Settings")]
    
    [Tooltip("X-axis position tolerance to compare, as best as possible," +
             " matching x-position with colliding and collided object")]
    [SerializeField]
    private float positionXTolerance = 0.001f;

    [Tooltip("Velocity X tolerance (both left and right) to which the provided force will be used" +
             " instead of the colliding object's velocity X")]
    [SerializeField] private float velocityXTolerance = 0.5f;

    #endregion
    
    #region Unity Events
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        Rigidbody2D collidingRigidBody2D = col.gameObject.GetComponent<Rigidbody2D>();
        
        // The gameobject that is entering collision must have a Rigidbody2D to add force.
        if (!collidingRigidBody2D)
            return;

        Vector2 collidingGoPosition = col.gameObject.transform.position;
        Vector2 position = transform.position;
        float absPositionDiff = Mathf.Abs(collidingGoPosition.x - position.x);
        
        // Only apply the force when landing on top of the gameobject.
        if (collidingGoPosition.y <= position.y && absPositionDiff > positionXTolerance ||
            absPositionDiff < positionXTolerance)
            return;

        // Randomize the provided force X velocity so it doesn't always go in the same direction.
        force.x = Random.Range(1, 50) < 25 ? force.x : -force.x;
        Vector2 collidingGoVelocity = collidingRigidBody2D.velocity;
        
        // If present, use the colliding rigidbody2D's X velocity; otherwise use provided force X velocity.
        collidingRigidBody2D.AddForce(
            ShouldUseProvidedForce(collidingGoVelocity) ? force : new Vector2(collidingGoVelocity.x, force.y),
            forceMode2D);
    }
    
    #endregion

    #region Helper Methods

    /// <summary>
    /// Determines whether the provided force or colliding gameobject's rigidbody velocity should be used.
    /// </summary>
    /// <param name="collidingGoVelocity">The current velocity of the colliding gameobject.</param>
    /// <returns></returns>
    private bool ShouldUseProvidedForce(Vector2 collidingGoVelocity)
    {
        return collidingGoVelocity.x > -velocityXTolerance && collidingGoVelocity.x < 0f ||
               collidingGoVelocity.x > 0f && collidingGoVelocity.x < velocityXTolerance;
    }

    #endregion
}
