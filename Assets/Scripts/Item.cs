using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

abstract public class Item
{
    public Dictionary<ResourceType, int> requiredResources;

    [SyncVar] public Character wearer;
    [SyncVar] public int tier;
    [SyncVar] public CombatUnit.NewStats stats;

    public string name;
    public Sprite sprite;

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
        
        return ConcreteCraft();
    }

    abstract protected Item ConcreteCraft();

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

    abstract protected void OnTierUp();
}

public class ItemHelmOfCommand : Item
{
    public ItemHelmOfCommand()
    {
        sprite = Resources.Load<Sprite>("Sprites/HelmOfCommand");
    }

    protected override Item ConcreteCraft()
    {
        return new ItemHelmOfCommand();
    }

    protected override void OnTierUp()
    {
        stats.attack = 2 + 2 * tier;
        stats.defense = 3 + 3 * tier;
    }
}
