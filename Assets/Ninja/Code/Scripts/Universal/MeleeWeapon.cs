using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    #region Public Properties
    
    /// <summary>
    /// The owner who is using the melee weapon.
    /// </summary>
    public GameObject Owner { get; set; }
    
    #endregion
    
    #region Public Fields
    
    [Header("Damage Settings")]
    
    [Tooltip("Light attack damage amount")] 
    public float lightAttackDmg = 12.5f;

    [Tooltip("heavy attack damage amount")] 
    public float heavyAttackDmg = 25f;
    
    #endregion
}
