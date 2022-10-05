using System;
using UnityEngine;

public class KnifePickup : MonoBehaviour
{
    #region Serialized Fields

    [Header("Knife Pickup Settings")] 
    
    [Tooltip("The amount that will be used to increase the max amount of knives allowed on screen")]
    [SerializeField] private int knifeAmount = 1;

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

    private void OnCollisionEnter2D(Collision2D col)
    {
        LayerMask playerMask = LayerMask.NameToLayer("Player");
        LayerMask groundMask = LayerMask.NameToLayer("Ground");
        LayerMask collidingObjectLayer = col.gameObject.layer;

        if (collidingObjectLayer != playerMask && collidingObjectLayer != groundMask)
            return;

        if (collidingObjectLayer == playerMask)
        {
            PlayerCombat playerCombat = col.gameObject.GetComponent<PlayerCombat>();
        
            if (!playerCombat)
                return;

            playerCombat.MaxKnives += knifeAmount;

            _soundManager.PlaySoundEffect(AudioSourceType.ItemEffects, pickupSoundClip);
            Destroy(gameObject);   
        }
        else if (collidingObjectLayer == groundMask)
            _soundManager.PlaySoundEffect(AudioSourceType.ItemEffects, hitGroundSoundClip);
    }
    
    #endregion
}
