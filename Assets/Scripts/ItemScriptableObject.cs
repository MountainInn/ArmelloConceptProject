using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "ScriptableObjects/ItemScriptableObject", order = 1)]
public class ItemScriptableObject : ScriptableObject
{
    [SerializeField] public Sprite icon;
    [SerializeField] public CombatUnit.Stats combatStats;
    [SerializeField] public CombatUnit.Stats perTier;
    [SerializeField] List<ResourceTuple> requiredResources;


    public Dictionary<ResourceType, int> RequiredResourcesAsDictionary()
    {
        return
            requiredResources
            .ToDictionary(t => t.resourceType,
                          t => t.amount);
    }

    [System.Serializable]
    private struct ResourceTuple
    {
        public ResourceType resourceType;
        public int amount;
    }
}
