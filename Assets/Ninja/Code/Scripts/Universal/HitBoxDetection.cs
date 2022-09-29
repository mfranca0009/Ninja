using System.Diagnostics;
using UnityEngine;

public class HitBoxDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        Health victimHealth = col.GetComponent<Health>();

        // If no health attached to victim, then we have nothing to do.
        if (!victimHealth)
            return;

        float damageToApply = 0f;
        GameObject invoker = null;
        
        // Combat scripts
        PlayerCombat playerCombat = GetComponentInParent<PlayerCombat>();
        EnemyCombat enemyCombat = GetComponentInParent<EnemyCombat>();
        
        // Weapon scripts
        MeleeWeapon meleeWeapon = GetComponentInParent<MeleeWeapon>();
        ThrowKnife throwKnife = GetComponentInParent<ThrowKnife>();

        // Process type of damage, the amount to apply, and the invoker who caused it
        if (meleeWeapon)
        {
            damageToApply = !playerCombat switch
            {
                false when !enemyCombat => playerCombat.LightAttackPerformed switch
                {
                    1 when playerCombat.SlowAttackPerformed == 0 => meleeWeapon.lightAttackDmg,
                    0 when playerCombat.SlowAttackPerformed == 1 => meleeWeapon.heavyAttackDmg,
                    _ => damageToApply
                },
                true when enemyCombat => enemyCombat.LightAttackPerformed switch
                {
                    1 when enemyCombat.SlowAttackPerformed == 0 => meleeWeapon.lightAttackDmg,
                    0 when enemyCombat.SlowAttackPerformed == 1 => meleeWeapon.heavyAttackDmg,
                    _ => damageToApply
                },
                _ => damageToApply
            };

            invoker = meleeWeapon.Owner;
        }
        else if (throwKnife)
        {
            damageToApply = throwKnife.throwKnifeDmg;
            invoker = throwKnife.Owner;
            throwKnife.UpdateActiveKnives();
            Destroy(throwKnife.gameObject);
        }
        
        // Deal damage to victim
        victimHealth.DealDamage(damageToApply, invoker);
    }
}