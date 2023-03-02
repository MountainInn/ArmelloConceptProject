using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "ScriptableObjects/CharacterScriptableObject", order = 1)]
public class CharacterScriptableObject : ScriptableObject
{
    public Sprite characterSprite;
    public Character.UtilityStats utilityStats;
    public CombatUnit.Stats combatStats;
}
