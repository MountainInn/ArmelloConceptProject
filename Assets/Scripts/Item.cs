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

    new public string name;
    public Sprite icon;
    private ParticleSystem particle;

    public Dictionary<ResourceType, int> costResources {get; private set;}

    public ItemScriptableObject itemSO {get; private set;}

    public void Initialize(ItemScriptableObject itemSO)
    {
        particle = GetComponentInChildren<ParticleSystem>();
        costResources = itemSO.RequiredResourcesAsDictionary();

        gameObject.name = itemSO.name;
        this.name = itemSO.name;
        this.icon = itemSO.icon;
        this.stats = itemSO.combatStats;
        this.itemSO = itemSO;
    }

    public void ToggleParticle(bool toggle)
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
        itemSO.combatStats += itemSO.perTier;
    }
}
