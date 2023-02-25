using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Item
{
    [SyncVar] public Character wearer;
    [SyncVar] public int tier;
    [SyncVar] public CombatUnit.Stats stats;

    public string name;
    public Sprite icon;

    readonly SyncDictionary<ResourceType, int> requiredResources = new SyncDictionary<ResourceType, int>();

    private ItemScriptableObject itemSO;

    public Item(){}

    public Item(ItemScriptableObject itemSO)
    {
        itemSO.RequiredResourcesAsDictionary()
            .ToList()
            .ForEach(kv => requiredResources.Add(kv));
       
        this.name = itemSO.name;
        this.icon = itemSO.icon;
        this.stats = itemSO.combatStats;
        this.itemSO = itemSO;
    }

    public Item Craft(Dictionary<ResourceType, int> resources)
    {
        bool canAfford =
            requiredResources
            .All(kv => resources[kv.Key] >= kv.Value);

        if (!canAfford)
            return null;

        requiredResources
            .ToList()
            .ForEach(kv => resources[kv.Key] -= kv.Value);
        
        return this;
    }

    public void Disassemble(List<Item> equippedItems, List<Item> recipes, Dictionary<ResourceType, int> resources)
    {
        Unequip();

        recipes.Add(this);

        requiredResources
            .ToList()
            .ForEach(kv =>
            {
                int scrapResource = Mathf.FloorToInt(kv.Value * 0.5f);
                resources[kv.Key] += scrapResource;
            });
    }

    public void Merge(Item otherItem)
    {
        if (this.GetType() != otherItem.GetType())
            return;

        if (this.tier != otherItem.tier)
            return;

        Character cacheWearer
            = this.wearer;

        this.Unequip();
        otherItem.Unequip();
        otherItem = null;

        this.TierUp();

        this.Equip(cacheWearer);
    }

    public void Equip(Character wearer)
    {
        Debug.Assert(wearer == null);

        this.wearer = wearer;
    }

    public void Unequip()
    {
        if (wearer == null)
            return;

        this.wearer = null;
    }

    public void TierUp()
    {
        tier++;
        OnTierUp();
    }

    void OnTierUp()
    {
        itemSO.combatStats += itemSO.perTier;
    }
}
