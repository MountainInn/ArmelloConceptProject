using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeSO", menuName = "ScriptableObjects/RecipeSO", order = 1)]
public class RecipeSO : ScriptableObject
{
    public List<ItemScriptableObject> requiredItems;
    public List<resreq> requiredResources;
    public ItemScriptableObject resultItem;

    [System.Serializable]
    public struct resreq
    {
        [SerializeField] public ResourceType resourceType;
        [SerializeField] public int amount;
    }
}
