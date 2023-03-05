using Mirror;
using MountainInn;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] Transform equipmentContainer;
    [SerializeField] ItemView groundView;

    private List<ItemView> equipmentViews;

    private ItemView prefabItemView;
    private Sprite blankIconSprite;
    private Inventory inventory;
    private HexTile groundTile;

    private void Awake()
    {
        prefabItemView = Resources.Load<ItemView>("Prefabs/Item Slot");
        blankIconSprite = Resources.Load<Sprite>("Sprites/Blank Icon");
    }
    private void Start()
    {
        MessageBroker.Default
            .Receive<OnStandOnTile>()
            .Subscribe(SwitchGroundTile);
    }

    private void SwitchGroundTile(OnStandOnTile msg)
    {
        msg.hex.itemPlacement.TryGetItem(out Item item);

        groundView.SetItem(item);
        groundTile = msg.hex;
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        inventory.Equipment.Callback += OnEquipSync;

        equipmentViews =
            inventory.Size.ToRange()
            .Select(i =>
                    InstantiateItemView(null))
            .Map(view =>
            {
                view.onLeftClick += () => DropItem(inventory, view);
                view.onRightClick += () => DisassembleFromEquipment(inventory, view);
            })
            .ToList();

        groundView.onLeftClick += () => Pickup(inventory);
        groundView.onRightClick += () => DisassembleFromGround();
    }

    private void DisassembleFromEquipment(Inventory inventory, ItemView view)
    {
        if (view.Item == null)
            return;

        inventory.CmdDisassemble(view.Item);

        view.SetItem(null);
    }
    private void DisassembleFromGround()
    {
        if (!groundTile.itemPlacement.IsPlaced)
            return;

        groundTile.itemPlacement.CmdDisassemble(inventory);

        groundView.SetItem(null);
    }

    private void Pickup(Inventory inventory)
    {
        if (groundView.Item == null)
            return;
        if (!groundTile.itemPlacement.IsPlaced)
            return;
        if (!inventory.HasSpace())
            return;

        groundTile.itemPlacement.CmdPickupItem(inventory);

        groundView.SetItem(null);
    }

    private void DropItem(Inventory inventory, ItemView view)
    {
        if (view.Item == null)
            return;
        if (groundTile.itemPlacement.IsPlaced)
            return;

        Item dropItem = view.Item;

        groundTile.itemPlacement.CmdDropItem(inventory, dropItem);

        groundView.SetItem(dropItem);
    }

    private ItemView InstantiateItemView(Item item)
    {
        var newItemSlot = Instantiate(prefabItemView, Vector3.zero, Quaternion.identity, equipmentContainer);

        newItemSlot.transform.localPosition = Vector3.zero;
        newItemSlot.transform.localEulerAngles = Vector3.zero;

        return newItemSlot;
    }

    private void OnEquipSync(SyncList<Item>.Operation op, int itemIndex, Item oldItem, Item newItem)
    {
        switch (op)
        {
            case SyncList<Item>.Operation.OP_ADD:
                break;
            case SyncList<Item>.Operation.OP_SET:
                break;
            case SyncList<Item>.Operation.OP_INSERT:
                break;
            case SyncList<Item>.Operation.OP_REMOVEAT:
                break;
            case SyncList<Item>.Operation.OP_CLEAR:
                break;
        }

        equipmentViews[itemIndex].SetItem(newItem);
    }
}
