using UnityEngine;

public class TriggerTileCharacterHealth : TriggerTile
{
    public TriggerTileCharacterHealth(HexTile hexTile) : base(hexTile)
    {
    }

    public override void Trigger(Player player)
    {
        player.character.combatUnit.health += 10;
        Debug.Log($"TriggerTIle: +10 Health");
    }
}
