using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Item : NetworkBehaviour
{
    [SyncVar] public Character wearer;
    [SyncVar] public int tier;
    [SyncVar] public CombatUnit.Stats stats;

    public Sprite icon;
    private ParticleSystem particle;

    public Dictionary<ResourceType, int> costResources;

    public ItemScriptableObject itemSO;

    [SyncVar(hook = nameof(OnItemSONameSync))] string itemSOName;
    private void OnItemSONameSync(string oldv, string newv)
    {
        itemSO = Resources.Load<ItemScriptableObject>($"Items/{newv}");

        Initialize(itemSO);
    }

    public void SetItemSOName(string name)
    {
        this.itemSOName = name;
    }

    public void Initialize(ItemScriptableObject itemSO)
    {
        this.itemSO = itemSO;

        particle = GetComponentInChildren<ParticleSystem>();
        costResources = itemSO.RequiredResourcesAsDictionary();

        gameObject.name = itemSO.name;
        this.icon = itemSO.icon;
        this.stats = itemSO.combatStats;
    }

    [ClientRpc]
    public void RpcToggleParticle(bool toggle)
    {
        if (toggle)
            particle.Play();
        else
            particle.Stop();
    }

    public string RequiredResourcesAsString()
    {
        return
            costResources
            .Select(kv => $"{kv.Key}: {kv.Value}")
            .Aggregate((a, b) => a + " | " + b);
    }

    public void TierUp()
    {
        tier++;
        OnTierUp();
    }

    void OnTierUp()
    {
    }
}
