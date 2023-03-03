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
    }

    public override void OnStartLocalPlayer()
    {
        owner = GetComponent<Character>();
        view = FindObjectOfType<InventoryView>();
        resourcesView = FindObjectOfType<ResourcesView>();

        view.SetInventory(this);
        resourcesView.SetResourcesSync(Resources);
    }

    private void LogResourceOnSync(SyncIDictionary<ResourceType, int>.Operation op, ResourceType key, int item)
    {
        resources.Log("Resources: ");
    }

    [Server]
    public void AddResource(ResourceType resourceType, int income)
    {
        Resources[resourceType] += income;
    }

    public bool HasSpace()
    {
        return equipment.Count < Size;
    }

    [Command(requiresAuthority = false)]
    public void CmdDisassemble(Item item)
    {
        Unequip(item);

        Recipes.Add(item);

        item.costResources
            .ToList()
            .ForEach(cost =>
            {
                int scrapResource = Mathf.FloorToInt(cost.Value * 0.5f);
                resources[cost.Key] += scrapResource;
            });

        NetworkServer.UnSpawn(item.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void CmdCraft(Item recipe)
    {
        if (!Recipes.Contains(recipe) || !HasSpace())
            return;

        bool canAfford =
            recipe.costResources
            .All(cost => Resources[cost.Key] >= cost.Value);

        if (!canAfford)
            return;

        recipe.costResources
            .ToList()
            .ForEach(cost => Resources[cost.Key] -= cost.Value);

        Equip(recipe);
    }

    [Command(requiresAuthority = false)]
    public void CmdMerge(Item a, Item b)
    {
        if (a.tier != b.tier)
            return;
        if (a.itemSO != b.itemSO)
            return;
        if (!equipment.Contains(a) || !equipment.Contains(b))
            return;

        Unequip(b);
        NetworkServer.UnSpawn(b.gameObject);

        a.TierUp();

        UpdateEquipmentStats();
    }

    [Server]
    public void Equip(Item item)
    {
        equipment.Add(item);
        UpdateEquipmentStats();
    }

    [Server]
    public void Unequip(Item item)
    {
        equipment.Remove(item);
        UpdateEquipmentStats();
    }

    [Server]
    private void UpdateEquipmentStats()
    {
        owner.combatUnit.SetEquipmentStats(GetTotalEquipmentStats());
    }

    private CombatUnit.Stats GetTotalEquipmentStats()
    {
        return
            (equipment.Any())
            ? equipment
            .Select(item => item.stats)
            .Aggregate((a, b) => a + b)
            : default;
    }
}
