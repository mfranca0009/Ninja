using System;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class ItemEntry
{
    public GameObject item;
    public float dropChance;
    public float conditionalDropChance;
}

public class ItemDrop : MonoBehaviour
{
    #region Serialized Fields

    [Header("Item Drop Settings")]
    
    [Tooltip("Should the item drop on death?")] 
    [SerializeField] private bool dropOnDeath = true;

    [Tooltip("Should the item drop when activated?")] 
    [SerializeField] private bool dropOnActivate;

    [Tooltip("The items that could potentially drop")]
    [SerializeField] private ItemEntry[] itemDrops;

    [Tooltip("The amount of items to drop")] 
    [SerializeField] private int amountToDrop = 1;

    [Header("Random Number Generator Settings")]
    [Tooltip("Use current date as random number generator seed, if enabled it will ignore the integer seed setting")]
    [SerializeField] private bool useCurrentDateTicks;
    
    [Tooltip("Seed used for the random number generator")] 
    [SerializeField] private int seed;

    #endregion

    #region Private Fields
    
    private bool _hasDroppedItems;
    private int _droppedItemsCount;
    
    // Health script
    private Health _health;
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
        
        DropItemOnDeath();
    }
    
    #endregion
    
    #region Private Helper Methods

    private void DropItemOnDeath()
    {
        if (!CanDropOnDeath())
            return;

        _hasDroppedItems = true;

        SpawnHealthDrop();
        SpawnRandomItemDrop();
    }

    private void DropItemOnActivate()
    {
        // TODO: should it have the same item drop concept as "drop on death"?
    }

    private void SpawnHealthDrop()
    {
        if (!CanDropHealthPotion())
            return;

        GameObject healthPotion =
            Instantiate(itemDrops[(int)ItemType.HealthPotion].item, transform.position, new Quaternion());

        if (!healthPotion)
            return;
        
        ItemDropEffectRocket itemDropEffectRocket = healthPotion.GetComponent<ItemDropEffectRocket>();

        if (!itemDropEffectRocket)
            return;
        
        itemDropEffectRocket.HorizontalDistance = 0f;

        healthPotion.SetActive(true);
        _droppedItemsCount++;
    }

    private void SpawnStrengthDrop()
    {
        if (!CanDropStrengthPotion())
            return;

        GameObject strengthPotion = Instantiate(itemDrops[(int)ItemType.StrengthPotion].item, transform.position,
            new Quaternion());
        
        if (!strengthPotion)
            return;
        
        ItemDropEffectRocket itemDropEffectRocket = strengthPotion.GetComponent<ItemDropEffectRocket>();

        if (!itemDropEffectRocket)
            return;
        
        itemDropEffectRocket.HorizontalDistance = 0.5f;

        strengthPotion.SetActive(true);
        _droppedItemsCount++;
    }

    private void SpawnKnifeDrop()
    {
        if (!CanDropKnife())
            return;

        GameObject throwKnife = Instantiate(itemDrops[(int)ItemType.ThrowingKnife].item, transform.position,
            new Quaternion());
        
        if (!throwKnife)
            return;
        
        ItemDropEffectRocket itemDropEffectRocket = throwKnife.GetComponent<ItemDropEffectRocket>();

        if (!itemDropEffectRocket)
            return;
        
        itemDropEffectRocket.HorizontalDistance = -0.5f;

        throwKnife.SetActive(true);
        _droppedItemsCount++;
    }

    private void SpawnRandomItemDrop()
    {
        if (_droppedItemsCount + 1 > amountToDrop)
            return;
        
        SpawnStrengthDrop();
        SpawnKnifeDrop();
    }

    private float GetRandomFloat()
    {
        return (float)_random.NextDouble() * 100f;
    }
    
    private bool CanDropOnDeath()
    {
        return dropOnDeath && _health.Dead && !_hasDroppedItems;
    }

    private bool CanDropHealthPotion()
    {
        Health killerHealth = _health.Killer.GetComponent<Health>();

        return killerHealth && killerHealth.HealthPoints < killerHealth.maxHealth &&
               itemDrops[(int)ItemType.HealthPotion].dropChance >= GetRandomFloat();
    }
    
    private bool CanDropStrengthPotion()
    {
        PlayerCombat playerCombat = _health.Killer.GetComponent<PlayerCombat>();

        return (!playerCombat.HasMeleeStrengthBoost &&
                itemDrops[(int)ItemType.StrengthPotion].dropChance >= GetRandomFloat()) ||
               (playerCombat.HasMeleeStrengthBoost &&
                itemDrops[(int)ItemType.StrengthPotion].conditionalDropChance >= GetRandomFloat());
    }
    
    private bool CanDropKnife()
    {
        PlayerCombat playerCombat = _health.Killer.GetComponent<PlayerCombat>();

        return (playerCombat.MaxKnives < 5 &&
                itemDrops[(int)ItemType.ThrowingKnife].dropChance >= GetRandomFloat()) ||
               (playerCombat.MaxKnives >= 5 &&
                itemDrops[(int)ItemType.StrengthPotion].conditionalDropChance >= GetRandomFloat());
    }
    
    private bool CanDropItems()
    {
        return (dropOnDeath || dropOnActivate) && itemDrops.Length != 0 && amountToDrop != 0;
    }
    
    #endregion
}
