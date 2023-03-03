using Mirror;

public class ItemPlacement : NetworkBehaviour
{
    [SyncVar] HexTile hexTile;
    Item item;

    private void Awake()
    {
        item = GetComponent<Item>();
    }

    public bool IsPlaced => hexTile != null;

    [Command(requiresAuthority = false)]
    public void CmdDropItem(Inventory inventory, HexTile hexTile)
    {
        if (IsPlaced)
            return;

        inventory.Unequip(item);

        this.hexTile = hexTile;

        item.ToggleParticle(true);
    }

    [Server]
    public void PutItem(HexTile hexTile)
    {
        if (IsPlaced)
            return;

        this.hexTile = hexTile;

        item.ToggleParticle(true);
    }

    [Command(requiresAuthority = false)]
    public void CmdPickupItem(Inventory inventory)
    {
        if (hexTile == null)
            return;
        if (!inventory.HasSpace())
            return;

        inventory.Equip(item);

        hexTile = null;

        item.ToggleParticle(false);
    }
}
