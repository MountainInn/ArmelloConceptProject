using UnityEngine;

public class InfluenceTileResource : InfluenceTile
{
    public MiningTile miningTile;

    public InfluenceTileResource(HexTile hexTile, ResourceType resourceType) : base(hexTile)
    {
        this.miningTile = new MiningTile(hexTile, resourceType);
    }

    public override void InfluenceEffect(Player player)
    {
        miningTile.UseTile(player);
        Debug.Log($"InfluenceTile: +1 Generic resource");

    }
}
