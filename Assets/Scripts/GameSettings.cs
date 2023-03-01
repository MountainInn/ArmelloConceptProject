using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GameSettings", fileName="GameSettings")]
public class GameSettings : ScriptableObject
{
    public int influenceThreshold = 3;
    public int plunderMultiplier = 2;
}
