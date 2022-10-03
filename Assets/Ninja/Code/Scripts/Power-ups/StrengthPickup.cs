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

    #endregion

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player"))
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

        Destroy(gameObject);
    }
}
