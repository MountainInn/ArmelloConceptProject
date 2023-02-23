using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "ScriptableObjects/CharacterScriptableObject", order = 1)]
public class CharacterScriptableObject : ScriptableObject
{
    public Sprite characterSprite;
    public string characterName;
    public Character.UtilityStats utilityStats;
    public CombatUnit.NewStats combatStats;
}
