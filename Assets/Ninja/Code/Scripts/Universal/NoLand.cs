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
    [SerializeField] private float positionXTolerance = 0.001f;

    [Tooltip("Velocity X tolerance (both left and right) to which the provided force will be used" +
             " instead of the colliding object's velocity X")]
    [SerializeField] private float velocityXTolerance = 0.5f;

    [Header("Damage Settings")]
    
    [Tooltip("Enable damage dealt to the owning gameobject when an invoking gameobject lands on it")]
    [SerializeField] private bool shouldDamage;

    [Tooltip("Damage amount that will applied to the owning gameobject")] 
    [SerializeField] private float landingDamage = 10f;

    [Header("Sound Effect Settings")] 
    
    [Tooltip("Land hit sound effect")] 
    [SerializeField] private AudioClip landHitSoundClip;
    
    #endregion

    #region Private Fields

    private SoundManager _soundManager;
    private AchievementManager _achievementManager;

    #endregion
    
    #region Unity Events

    private void Awake()
    {
        _soundManager = FindObjectOfType<SoundManager>();
        _achievementManager = FindObjectOfType<AchievementManager>();
    }
    
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

        // If no damage should occur, then leave the method body now.
            if (!shouldDamage || col.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        // Play hit sound effect
        if (_soundManager)
            _soundManager.PlaySoundEffect(AudioSourceType.DamageEffects, landHitSoundClip);
        
        // Damage victim
        gameObject.GetComponent<Health>().DealDamage(landingDamage, col.gameObject);
        
        /* ACHIEVEMENT CHECK */

        if (!_achievementManager || col.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        Achievement noLandAchievement =
            _achievementManager.Achievements.Find(achievement => achievement.Title == "Not an Italian Plumber");

        if (noLandAchievement == null)
            return;

        _achievementManager.ObtainAchievement(noLandAchievement.Title);
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
