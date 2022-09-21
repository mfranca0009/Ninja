using UnityEngine;

public class HitBoxDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        Health victimHealth = col.GetComponent<Health>();

        if (!victimHealth)
            return;

        PlayerCombat playerCombat = GetComponentInParent<PlayerCombat>();
        EnemyCombat enemyCombat = GetComponentInParent<EnemyCombat>();
        
        float damageToApply = 0f;
        
        if (playerCombat)
        {
            damageToApply = playerCombat.LightAttackPerformed switch
            {
                true when !playerCombat.SlowAttackPerformed => playerCombat.lightAttackDmg,
                false when playerCombat.SlowAttackPerformed => playerCombat.heavyAttackDmg,
                _ => damageToApply
            };
            
            // Reset
            playerCombat.LightAttackPerformed = false;
            playerCombat.SlowAttackPerformed = false;
            
            victimHealth.DealDamage(damageToApply, playerCombat.gameObject);
        }
        else if (enemyCombat)
        {
            damageToApply = enemyCombat.LightAttackPerformed switch
            {
                true when !enemyCombat.SlowAttackPerformed => enemyCombat.lightAttackDmg,
                false when enemyCombat.SlowAttackPerformed => enemyCombat.heavyAttackDmg,
                _ => damageToApply
            };
        
            // Reset
            enemyCombat.LightAttackPerformed = false;
            enemyCombat.SlowAttackPerformed = false;
            
            victimHealth.DealDamage(damageToApply);
        }
    }
}
