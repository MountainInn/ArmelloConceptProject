abstract public class TriggerTile : UsableTile
{
    protected TriggerTile(HexTile hexTile) : base(hexTile)
    {
    }

    public override void UseTile(Player player)
    {
        Trigger(player);
    }

    abstract public void Trigger(Player player);
}
