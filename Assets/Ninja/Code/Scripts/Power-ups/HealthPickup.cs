using System;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    #region Serialized Fields

    [Header("Health Pickup Settings")] 
    
    [Tooltip("The amount of health the pickup will give the player")]
    [SerializeField] private float healthAmount = 25f;

    [Tooltip("The amount of time the pickup will last while untouched by player")] 
    [SerializeField] private float lifetimeTimer = 10f;

    [Header("Sound Effect Settings")] 
    
    [Tooltip("The sound effect when picking up this item")]
    [SerializeField] private AudioClip pickupSoundClip;

    [Tooltip("The sound effect when this item hits the ground")] 
    [SerializeField] private AudioClip hitGroundSoundClip;
    
    #endregion

    #region Private Fields

    private SoundManager _soundManager;
    
    #endregion
    
    #region Unity Events

    private void Awake()
    {
        _soundManager = FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        UpdateLifetime();
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        LayerMask playerMask = LayerMask.NameToLayer("Player");
        LayerMask groundMask = LayerMask.NameToLayer("Ground");
        LayerMask collidingObjectLayer = col.gameObject.layer;

        if (collidingObjectLayer != playerMask && collidingObjectLayer != groundMask)
            return;

        if (collidingObjectLayer == playerMask)
        {
            Health health = col.gameObject.GetComponent<Health>();

            if (!health)
                return;

            if (_soundManager)
                _soundManager.PlaySoundEffect(AudioSourceType.ItemEffects, pickupSoundClip);
            
            health.DealHeal(healthAmount);

            Destroy(gameObject);
        }
        else if (collidingObjectLayer == groundMask && _soundManager)
            _soundManager.PlaySoundEffect(AudioSourceType.ItemEffects, hitGroundSoundClip);
    }
    
    #endregion

    #region Update Methods

    /// <summary>
    /// Update lifetime timer, when it expires the pickup will be destroyed.
    /// </summary>
    private void UpdateLifetime()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (lifetimeTimer <= 0f)
            Destroy(gameObject);
        else
            lifetimeTimer -= Time.deltaTime;
    }

    #endregion
}
