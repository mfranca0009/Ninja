using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    #region Serialized Fields

    [Header("Drop Mechanic Settings")] 
    
    [Tooltip("Should the item drop on death?")] 
    [SerializeField] private bool dropOnDeath = true;

    [Tooltip("Should the item drop when activated?")] 
    [SerializeField] private bool dropOnActivate;
    
    [Header("Item Drop Settings")]
    
    [Tooltip("The items that could potentially drop")]
    [SerializeField] private GameObject[] itemDrops;

    [Tooltip("The amount of items to drop")] 
    [SerializeField] private int amountToDrop = 1;
    
    #endregion

    #region Private Fields
    
    private bool _hasDroppedItems;
    private int _droppedItemsCount;
    
    // Health script
    private Health _health;
    
    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _health = GetComponent<Health>();
        
        // Default to drop on death if both drop types are active.
        if (dropOnDeath && dropOnActivate)
            dropOnActivate = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!CanDropOnDeath())
            return;

        _hasDroppedItems = true;
        
        Health killerHealth = _health.Killer.GetComponent<Health>();

        if (killerHealth.HealthPoints < killerHealth.maxHealth)
        {
            // drop health point and increment dropped items count
        }

        if (_droppedItemsCount < amountToDrop)
            SpawnRandomItemDrop();
    }
    
    #endregion
    
    #region Private Helper Methods

    private void SpawnRandomItemDrop()
    {
        
    }
    
    private bool CanDropOnDeath()
    {
        return dropOnDeath && _health.Dead && !_hasDroppedItems;
    }
    
    #endregion
}
