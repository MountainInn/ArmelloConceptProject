using Mirror;
using System.Linq;

public class ItemPlacement : NetworkBehaviour
{
    readonly SyncDictionary<HexTile, Item>
        itemPlacements = new SyncDictionary<HexTile, Item>();

    public bool IsPlaced(Item item)
    {
        return itemPlacements.Values.Contains(item);
    }

    public Item GetItem(HexTile hexTile)
    {
        return itemPlacements[hexTile];
    }

    [Command(requiresAuthority = false)]
    public void CmdDropItem(Inventory inventory, HexTile hexTile, Item item)
    {
        if (IsPlaced(item))
            return;

        inventory.Unequip(item);

        itemPlacements.Add(hexTile, item);

        item.transform.position = ItemSpawner.GetItemPosition(hexTile);

        item.ToggleParticle(true);
    }

    [Server]
    public void PutItem(HexTile hexTile, Item item)
    {
        if (IsPlaced(item))
            return;

        itemPlacements.Add(hexTile, item);

        item.transform.position = ItemSpawner.GetItemPosition(hexTile);

        item.ToggleParticle(true);
    }

    [Command(requiresAuthority = false)]
    public void CmdPickupItem(Inventory inventory, HexTile hexTile, Item item)
    {
        if (!inventory.HasSpace())
            return;

        inventory.Equip(item);

        itemPlacements.Remove(hexTile);

        item.ToggleParticle(false);
    }
}
