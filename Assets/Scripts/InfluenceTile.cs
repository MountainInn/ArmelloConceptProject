using Mirror;
using UniRx;

abstract public class InfluenceTile : UsableTile
{
    [SyncVar]
    public Player owner;

    protected InfluenceTile(HexTile hexTile) : base(hexTile)
    {
    }

    public override void UseTile(Player player)
    {
        if (owner == player)
            return;

        MessageBroker.Default
            .Publish(new TileTakenMsg(){ previousOwner = owner, newOwner = player, tile = this, hexTile = hexTile});

        owner = player;
    }

    abstract public void InfluenceEffect(Player player);

    public struct TileTakenMsg
    {
        public Player previousOwner, newOwner;
        public InfluenceTile tile;
        public HexTile hexTile;
    }
}
