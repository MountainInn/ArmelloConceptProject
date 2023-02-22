using UnityEngine;

[AddComponentMenu("ScriptableObjects/CharacterScriptableObject", 0)]
public class CharacterScriptableObject : ScriptableObject
{
    public Sprite characterSprite;
    public string characterName;
    public Character.UtilityStats utilityStats;
    public CombatUnit.NewStats combatStats;
}
