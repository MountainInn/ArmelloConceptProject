using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CharacterSettings", fileName="CharacterSettings", order = 5)]
public class CharacterSettings : ScriptableObject
{
    public CharacterScriptableObject characterSO;
    public Color characterColor;
}
