using UnityEngine;

public class HitBoxDetection : MonoBehaviour
{
    #region Serialized Fields

    [Header("Sound Effect Settings")] 
    
    [Tooltip("Sound effect for light attack hit")]
    [SerializeField] private AudioClip lightAttackHitSoundClip;

    [Tooltip("Sound effect for slow attack hit")]
    [SerializeField] private AudioClip slowAttackHitSoundClip;

    [Tooltip("Sound effect for throw knife hit")] 
    [SerializeField] private AudioClip knifeHitSoundClip;
    
    #endregion

    #region Private Fields

    // Sound Effects / Music
    private SoundManager _soundManager;
    
    #endregion
    
    #region Unity Events

    private void Awake()
    {
        _soundManager = FindObjectOfType<SoundManager>();
    }
    
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
                    1 when playerCombat.SlowAttackPerformed == 0 => meleeWeapon.LightAttackDmg,
                    0 when playerCombat.SlowAttackPerformed == 1 => meleeWeapon.HeavyAttackDmg,
                    _ => damageToApply
                },
                true when enemyCombat => enemyCombat.LightAttackPerformed switch
                {
                    1 when enemyCombat.SlowAttackPerformed == 0 => meleeWeapon.LightAttackDmg,
                    0 when enemyCombat.SlowAttackPerformed == 1 => meleeWeapon.HeavyAttackDmg,
                    _ => damageToApply
                },
                _ => damageToApply
            };
            
            invoker = meleeWeapon.Owner;

            // Hit sound effect (light / slow attack)
            bool lightAttackPerformed = !playerCombat
                ? enemyCombat.LightAttackPerformed == 1
                : playerCombat.LightAttackPerformed == 1;

            _soundManager.PlaySoundEffect(AudioSourceType.DamageEffects,
                lightAttackPerformed ? lightAttackHitSoundClip : slowAttackHitSoundClip);
        }
        else if (throwKnife)
        {
            damageToApply = throwKnife.throwKnifeDmg;
            invoker = throwKnife.Owner;
            throwKnife.UpdateActiveKnives();
            Destroy(throwKnife.gameObject);

            // Hit sound effect (throw knife)
            _soundManager.PlaySoundEffect(AudioSourceType.DamageEffects, knifeHitSoundClip);
        }
        
        // Deal damage to victim
        victimHealth.DealDamage(damageToApply, invoker);
    }
    
    #endregion
}