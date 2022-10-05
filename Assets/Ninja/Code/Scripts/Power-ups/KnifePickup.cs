using UnityEngine;

public class KnifePickup : MonoBehaviour
{
    #region Serialized Fields

    [Header("Knife Pickup Settings")] 
    
    [Tooltip("The amount that will be used to increase the max amount of knives allowed on screen")]
    [SerializeField] private int knifeAmount = 1;

    #endregion

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        PlayerCombat playerCombat = col.gameObject.GetComponent<PlayerCombat>();
        
        if (!playerCombat)
            return;

        playerCombat.MaxKnives += knifeAmount;

        Destroy(gameObject);
    }
}
