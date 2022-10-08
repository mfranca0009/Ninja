using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class ItemEntry
{
    public GameObject item;
    public ItemType itemType;
    public float dropChance;
    public float conditionalDropChance;
}

public class ItemDrop : MonoBehaviour
{
    #region Public Properties

    /// <summary>
    /// State to notify when items were dropped
    /// </summary>
    public bool HasDroppedItems { get; private set; }

    #endregion
    
    #region Serialized Fields

    [Header("Item Drop Mechanic Settings")]
    
    [Tooltip("Should the item drop on death?")] 
    [SerializeField] private bool dropOnDeath = true;

    [Tooltip("Should the item drop when activated?")] 
    [SerializeField] private bool dropOnActivate;

    [Tooltip("The amount of items to drop")]
    [SerializeField, Range(1,3)] private int amountToDrop = 1;

    [Header("Item Settings")]
    
    [Tooltip("The items that could potentially drop")]
    [SerializeField] private ItemEntry[] itemDrops;
    
    [Header("Item condition Settings")]
    
    [Tooltip("Knife drop threshold, where if this value is surpassed, the conditional drop chance is used")]
    [SerializeField] private int lowerDropChanceKnivesThreshold = 2;
    
    [Tooltip("Maximum Knife drop threshold, where if this value is met, knife drops will stop dropping")]
    [SerializeField] private int maxKnivesThreshold = 4;

    [Tooltip("The maximum duration threshold, where if this value is met, strength potions will stop dropping")]
    [SerializeField] private float maxStrengthBoostDuration = 60f;

    [Header("Random Number Generator Settings")]
    
    [Tooltip("Use current date as random number generator seed, if enabled it will ignore the integer seed setting")]
    [SerializeField] private bool useCurrentDateTicks = true;
    
    [Tooltip("Seed used for the random number generator")] 
    [SerializeField] private int seed;

    #endregion

    #region Private Fields
    
    // Item drop
    private int _droppedItemsCount;
    
    // Health script
    private Health _health;
    
    // RNG
    private Random _random;
    
    #endregion
    
    #region Unity Events
    
    private void Awake()
    {
        _health = GetComponent<Health>();

        // RNG seed setup
        _random = new Random(useCurrentDateTicks ? (int)DateTime.Now.Ticks : seed);

        // Default to drop on death if both drop types are active.
        if (dropOnDeath && dropOnActivate)
            dropOnActivate = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!CanDropItems())
            return;
        
        DropItemsOnDeath();
    }
    
    #endregion
    
    #region Update Methods
    
    /// <summary>
    /// Drop items on death.
    /// </summary>
    private void DropItemsOnDeath()
    {
        if (!CanDropOnDeath())
            return;

        HasDroppedItems = true;
        
        SpawnRandomItemDrops();
    }

    /// <summary>
    /// Drop items on activation.
    /// </summary>
    private void DropItemOnActivate()
    {
        // TODO: should it have the same item drop concept as "drop on death"?
    }
    
    #endregion
    
    #region Private Helper Methods

    /// <summary>
    /// Spawn a random item drop after shuffling the available items to drop and making sure that the item type is
    /// allowed to be dropped. Conditional checks include normal/conditional drop chances, as well as additional
    /// specialized conditions such as strength boost already active, number of max knives, or current health.
    /// </summary>
    private void SpawnRandomItemDrops()
    {
        // shuffle items
        itemDrops = itemDrops.OrderBy(x => _random.Next(0,itemDrops.Length)).ToArray();

        foreach (ItemEntry itemEntry in itemDrops)
        {
            // Determine if drop count has already met its amount required
            if (_droppedItemsCount >= amountToDrop)
                return;

            // If this item cannot be dropped, just continue.
            if (!CanDropItemType(itemEntry))
                continue;
            
            GameObject itemToDrop = Instantiate(itemEntry.item, transform.position,
                new Quaternion());

            if (!itemToDrop)
                return;
            
            ItemDropEffectRocket itemDropEffectRocket = itemToDrop.GetComponent<ItemDropEffectRocket>();

            if (!itemDropEffectRocket)
                return;

            itemDropEffectRocket.HorizontalDistance = _droppedItemsCount switch
            {
                1 => 0.5f,
                2 => -0.5f,
                _ => 0f
            };

            itemToDrop.SetActive(true);
            _droppedItemsCount++;
        }
    }

    /// <summary>
    /// Retrieve a random float multiplied by 100.
    /// </summary>
    /// <returns>Returns a value between 1 and 100.</returns>
    private float GetRandomFloat()
    {
        return (float)_random.NextDouble() * 100f;
    }
    
    /// <summary>
    /// Determine if dropping item(s) on death is allowed.
    /// </summary>
    /// <returns>Returns true if item(s) can be dropped on death, otherwise false.</returns>
    private bool CanDropOnDeath()
    {
        return dropOnDeath && _health.Dead && !HasDroppedItems;
    }

    /// <summary>
    /// Determine if the item can be dropped filtered by the item type.
    /// </summary>
    /// <param name="itemEntry">The item that is being processed for a drop.</param>
    /// <returns>Returns true if the item is allowed to be dropped, otherwise false.</returns>
    private bool CanDropItemType(ItemEntry itemEntry)
    {
        // Potential scripts to be used to check certain conditions
        Health killerHealth;
        PlayerCombat playerCombat;
        
        switch (itemEntry.itemType)
        {
            case ItemType.HealthPotion:
            {
                killerHealth = _health.Killer.GetComponent<Health>();
                
                return killerHealth && killerHealth.HealthPoints < killerHealth.maxHealth &&
                       itemEntry.dropChance >= GetRandomFloat();
            }
            case ItemType.StrengthPotion:
            {
                playerCombat = _health.Killer.GetComponent<PlayerCombat>();
                
                return playerCombat.StrengthBoostTimer < maxStrengthBoostDuration &&
                       !playerCombat.HasMeleeStrengthBoost && itemEntry.dropChance >= GetRandomFloat() ||
                        playerCombat.HasMeleeStrengthBoost && itemEntry.conditionalDropChance >= GetRandomFloat();
            }
            case ItemType.ThrowingKnife:
            {
                playerCombat = _health.Killer.GetComponent<PlayerCombat>();

                return playerCombat.MaxKnives != maxKnivesThreshold &&
                       playerCombat.MaxKnives < lowerDropChanceKnivesThreshold &&
                       itemEntry.dropChance >= GetRandomFloat() ||
                       playerCombat.MaxKnives >= lowerDropChanceKnivesThreshold &&
                       itemEntry.conditionalDropChance >= GetRandomFloat();
            }
            default:
                return false;
        }
    }

    /// <summary>
    /// Determine if any items can be dropped.
    /// </summary>
    /// <returns>Returns true if any items can be dropped, otherwise false.</returns>
    private bool CanDropItems()
    {
        return (dropOnDeath || dropOnActivate) && itemDrops.Length != 0 && amountToDrop != 0;
    }
    
    #endregion
}
