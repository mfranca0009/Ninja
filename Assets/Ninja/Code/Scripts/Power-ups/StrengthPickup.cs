using System;
using UnityEngine;

public class StrengthPickup : MonoBehaviour
{
    #region Serialized Fields

    [Header("Strength Pickup Settings")] 
    
    [Tooltip("The multiplier applied to the damage output for melee attacks")]
    [SerializeField] private float meleeDamageMultiplier = 1.2f;

    [Tooltip("The base duration, in seconds, to start the melee strength boost timer")] 
    [SerializeField] private float baseDuration = 15f;
    
    [Tooltip("The extend duration, in seconds, to increase the melee strength boost timer")] 
    [SerializeField] private float extendDuration = 15f;
    
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
        LayerMask groundMask = LayerMask.NameToLayer("Ground");
        LayerMask collidingObjectLayer = col.gameObject.layer;

        if (collidingObjectLayer != groundMask || !_soundManager)
            return;

        _soundManager.PlaySoundEffect(AudioSourceType.ItemEffects, hitGroundSoundClip);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        LayerMask playerMask = LayerMask.NameToLayer("Player");
        LayerMask collidingObjectLayer = col.gameObject.layer;

        if (collidingObjectLayer != playerMask)
            return;

        PlayerCombat playerCombat = col.gameObject.GetComponent<PlayerCombat>();
        
        if (!playerCombat)
            return;

        if (!playerCombat.HasMeleeStrengthBoost)
        {
            playerCombat.HasMeleeStrengthBoost = true;
            playerCombat.StrengthBoostTimer = baseDuration;
            MeleeWeapon meleeWeapon;

            if (playerCombat.meleeWeaponLeft)
            {
                meleeWeapon = playerCombat.meleeWeaponLeft.GetComponent<MeleeWeapon>();

                if (!meleeWeapon)
                    return;

                meleeWeapon.LightAttackDmg *= meleeDamageMultiplier;
                meleeWeapon.HeavyAttackDmg *= meleeDamageMultiplier;
            }

            if (playerCombat.meleeWeaponRight)
            {
                meleeWeapon = playerCombat.meleeWeaponRight.GetComponent<MeleeWeapon>();

                if (!meleeWeapon)
                    return;

                meleeWeapon.LightAttackDmg *= meleeDamageMultiplier;
                meleeWeapon.HeavyAttackDmg *= meleeDamageMultiplier;
            }
        }
        else
            playerCombat.StrengthBoostTimer += extendDuration;

        if (_soundManager)
            _soundManager.PlaySoundEffect(AudioSourceType.ItemEffects, pickupSoundClip);
            
        Destroy(gameObject);   
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
