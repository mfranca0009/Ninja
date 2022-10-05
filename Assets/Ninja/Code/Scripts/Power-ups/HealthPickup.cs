using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    #region Serialized Fields

    [Header("Health Pickup Settings")] 
    
    [Tooltip("The amount of health the pickup will give the player")]
    [SerializeField] private float healthAmount = 25f;

    #endregion

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        Health health = col.gameObject.GetComponent<Health>();
        
        if (!health)
            return;

        health.DealHeal(healthAmount);
        
        Destroy(gameObject);
    }
}
