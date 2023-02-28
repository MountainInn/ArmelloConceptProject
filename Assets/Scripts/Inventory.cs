using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using MountainInn;

public class Inventory : NetworkBehaviour
{
    readonly SyncDictionary<ResourceType, int>
        resources = new SyncDictionary<ResourceType, int>();

    readonly SyncList<Item>
        recipes = new SyncList<Item>(),
        equipment = new SyncList<Item>();

    Character owner;

    public int Size => (owner) ? owner.utilityStats.thriftiness : 0;
    public SyncList<Item> Recipes => recipes;
    public SyncList<Item> Equipment => equipment;
    public SyncDictionary<ResourceType, int> Resources => resources;

    private InventoryView view;
    private ResourcesView resourcesView;


    public override void OnStartServer()
    {
        System.Enum.GetValues(typeof(ResourceType))
            .Cast<ResourceType>()
            .ToList()
            .ForEach(r => resources.Add(r, 0));
    }

    public override void OnStartClient()
    {
        resources.Callback += LogResourceOnSync;

        if (isOwned)
        {
            view = FindObjectOfType<InventoryView>();
            view.SetInventory(this);

            resourcesView = FindObjectOfType<ResourcesView>();
            resourcesView.SetResourcesSync(Resources);

            owner =
                NetworkClient.connection.GetSingleOwnedOfType<Character>();
        }
    }

    private void LogResourceOnSync(SyncIDictionary<ResourceType, int>.Operation op, ResourceType key, int item)
    {
        resources.Log("Resources: ");
    }

    public bool HasSpace()
    {
        return equipment.Count < Size;
    }

    public void PickupItem(HexTile tile)
    {
        if (!HasSpace())
            return;

        Equip(tile.item);
        tile.item = null;
    }

    public void DropItem(Item item)
    {
        HexTile tile = owner.GetHexTile();

        if (tile.item != null)
            return;
        
        Unequip(item);
        tile.item = item;
    }

    public void Craft(Item item)
    {
        Debug.Assert(recipes.Contains(item));
        Debug.Assert(HasSpace());

        Item newItem = null; // item.Craft(resources);

        Equip(newItem);
    }

    public void Merge(Item a, Item b)
    {
        Debug.Assert(equipment.Contains(a) && equipment.Contains(b));

        Unequip(b);

        a.Merge(b);

        UpdateEquipmentStats();
    }

    public void Equip(Item item)
    {
        equipment.Add(item);
        item.Equip(owner);
        UpdateEquipmentStats();
    }

    public void Unequip(Item item)
    {
        equipment.Remove(item);
        item.Unequip();
        UpdateEquipmentStats();
    }

    private void UpdateEquipmentStats()
    {
        owner.combatUnit.SetEquipmentStats(GetTotalEquipmentStats());
    }

    private CombatUnit.Stats GetTotalEquipmentStats()
    {
        return equipment
            .Select(item => item.stats)
            .Aggregate((a, b) => a + b);
    }
}
