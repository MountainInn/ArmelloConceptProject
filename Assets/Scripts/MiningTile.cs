using UnityEngine;
using Mirror;
using UniRx;

public class MiningTile : UsableTile
{
    [SyncVar]
    public ResourceType resourceType;

    public MiningTile(HexTile hexTile, ResourceType resourceType) : base(hexTile)
    {
        this.resourceType = resourceType;
    }

    public override void UseTile(Player player)
    {
        MessageBroker.Default
            .Publish(new TileMinedMsg(){ player = player, resourceType = resourceType, amount = 1 });
        Debug.Log($"MiningTile: +1 Generic Resource");

    }

    public struct TileMinedMsg
    {
        public Player player;
        public ResourceType resourceType;
        public int amount;
    }
}
