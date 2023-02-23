using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    Dictionary<ResourceType, int> resources;
    List<Item> recipes, equippedItems;
    Character owner;

    public void Craft(Item item)
    {
        Debug.Assert(recipes.Contains(item));
        Debug.Assert(equippedItems.Count < owner.utilityStats.thriftiness);

        var newItem = item.Craft(resources);

        Equip(newItem);
    }

    public void Merge(Item a, Item b)
    {
        Debug.Assert(equippedItems.Contains(a) && equippedItems.Contains(b));

        Unequip(b);

        a.Merge(b);

        UpdateEquipmentStats();
    }

    public void Equip(Item item)
    {
        equippedItems.Add(item);
        item.Equip(owner);
        UpdateEquipmentStats();
    }

    public void Unequip(Item item)
    {
        equippedItems.Remove(item);
        item.Unequip();
        UpdateEquipmentStats();
    }

    private void UpdateEquipmentStats()
    {
        owner.combatUnit.UpdateEquipmentStats(GetTotalEquipmentStats());
    }

    private CombatUnit.NewStats GetTotalEquipmentStats()
    {
        return equippedItems
            .Select(item => item.stats)
            .Aggregate((a, b) => a + b);
    }
}
