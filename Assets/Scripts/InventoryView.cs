using Mirror;
using MountainInn;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] Transform equipmentContainer;
    [SerializeField] ItemView groundItemView;

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
            .Subscribe(SetTileItem);
    }

    private void SetTileItem(OnStandOnTile msg)
    {
        var item = msg.hex.GetItem();

        if (item == null)
            return;

        groundItemView.SetItem(msg.hex.GetItem());
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
                view.onRightClick += () => Disassemble(inventory, view);
            })
            .ToList();

        groundItemView.onLeftClick += () => Pickup(inventory);
        groundItemView.onLeftClick += () => Disassemble(inventory, groundItemView);
    }

    private void Disassemble(Inventory inventory, ItemView view)
    {
        if (view.Item == null)
            return;

        inventory.CmdDisassemble(view.Item);
        view.SetItem(null);
    }

    private void Pickup(Inventory inventory)
    {
        if (groundItemView.Item == null)
            return;

        ItemPlacement placement =
            groundItemView.Item.GetComponent<ItemPlacement>();

        if (!placement.IsPlaced)
            return;
        if (inventory.HasSpace() == false)
            return;

        placement.CmdPickupItem(inventory);
        
        groundItemView.SetItem(null);
    }

    private void DropItem(Inventory inventory, ItemView view)
    {
        if (view.Item == null)
            return;
        if (groundItemView.Item != null)
            return;

        Item dropItem = view.Item;
        ItemPlacement placement = dropItem.GetComponent<ItemPlacement>();

        placement.CmdDropItem(inventory, groundTile);

        groundItemView.SetItem(dropItem);
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
