using Mirror;
using System.Linq;

public class ItemPlacement : NetworkBehaviour
{
    [SyncVar] Item item;
    HexTile hexTile;

    private void Awake()
    {
        hexTile = GetComponent<HexTile>();
    }

    public bool IsPlaced => item != null;

    public bool TryGetItem(out Item item)
    {
        item = this.item;
        return item;
    }

    [Command(requiresAuthority = false)]
    public void CmdDropItem(Inventory inventory, Item item)
    {
        inventory.Unequip(item);

        this.item = item;

        item.transform.position = ItemSpawner.GetItemPosition(hexTile);

        item.RpcToggleParticle(true);
    }

    [Server]
    public void PutItem(Item item)
    {
        this.item = item;

        item.transform.position = ItemSpawner.GetItemPosition(hexTile);

        item.RpcToggleParticle(true);
    }

    [Command(requiresAuthority = false)]
    public void CmdPickupItem(Inventory inventory)
    {
        inventory.Equip(item);

        item.RpcToggleParticle(false);

        item = null;
    }

    [Command(requiresAuthority = false)]
    public void CmdDisassemble(Inventory inventory)
    {
        inventory.CmdDisassemble(item);

        item = null;
    }
}
