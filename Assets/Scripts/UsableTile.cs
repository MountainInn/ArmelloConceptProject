using Mirror;

public abstract class UsableTile
{
    public HexTile hexTile;

    protected UsableTile(HexTile hexTile)
    {
        this.hexTile = hexTile;
    }

    abstract public void UseTile(Player player);
}

public enum UsableTileType
{
    Mining, AutoMining, HealthBonus
}
