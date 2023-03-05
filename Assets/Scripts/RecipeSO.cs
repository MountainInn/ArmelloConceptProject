using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeSO", menuName = "ScriptableObjects/RecipeSO", order = 1)]
public class RecipeSO : ScriptableObject
{
    public List<ItemScriptableObject> requiredItems;
    public ItemScriptableObject resultItem;
}
