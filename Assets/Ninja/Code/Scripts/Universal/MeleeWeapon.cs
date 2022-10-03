using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The owner who is using the melee weapon.
    /// </summary>
    public GameObject Owner { get; set; }
    public float LightAttackDmg { get; set; }
    public float HeavyAttackDmg { get; set; }

    #endregion
    
    #region Public Fields
    
    [Header("Damage Settings")]
    
    [Tooltip("Light attack damage amount")] 
    public float baseLightAttackDmg = 12.5f;

    [Tooltip("heavy attack damage amount")] 
    public float baseHeavyAttackDmg = 25f;
    
    #endregion

    #region Unity Events

    private void Awake()
    {
        LightAttackDmg = baseLightAttackDmg;
        HeavyAttackDmg = baseHeavyAttackDmg;
    }

    #endregion
}
