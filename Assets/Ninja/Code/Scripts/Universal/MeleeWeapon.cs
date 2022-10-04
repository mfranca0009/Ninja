using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The owner who is using the melee weapon.
    /// </summary>
    public GameObject Owner { get; set; }
    
    /// <summary>
    /// The light attack damage that will be applied once a light attack is performed and hits a target
    /// </summary>
    public float LightAttackDmg { get; set; }
    
    /// <summary>
    /// The heavy attack damage that will be applied once a slow attack is performed and hits a target
    /// </summary>
    public float HeavyAttackDmg { get; set; }

    #endregion
    
    #region Public Fields
    
    [Header("Damage Settings")]
    
    [Tooltip("Base light attack damage amount")] 
    public float baseLightAttackDmg = 12.5f;

    [Tooltip("Base heavy attack damage amount")] 
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
