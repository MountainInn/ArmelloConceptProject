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

    public bool TryGetItem(HexTile hexTile, out Item item)
    {
        return itemPlacements.TryGetValue(hexTile, out item);
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

    [Command(requiresAuthority = false)]
    public void CmdDisassemble(Inventory inventory, Item item)
    {
        if (IsPlaced(item))
        {
            var hexTile =
                itemPlacements
                .First(kv => kv.Value == item);

            itemPlacements.Remove(hexTile);
        }

        inventory.CmdDisassemble(item);
    }
}
