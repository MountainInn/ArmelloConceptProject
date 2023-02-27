using Mirror;
using MountainInn;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] Transform equipmentContainer;
    [SerializeField] ItemView tileItem;

    private List<ItemView> equipmentViews;
    private ItemView prefabItemView;

    private void Start()
    {
        MessageBroker.Default
            .Receive<OnStandOnTile>()
            .Subscribe(SetTileItem);
    }

    private void SetTileItem(OnStandOnTile msg)
    {
        if (!msg.hex.isOwned)
        {
            Debug.LogError($"SetTileItem: hex is not owned");
            return;
        }

        tileItem.SetItem(msg.hex.item);
    }

    public void SetInventory(Inventory inventory)
    {
        inventory.Equipment.Callback += OnEquipSync;

        equipmentViews =
            inventory.Size.ToRange()
            .Select(i => InstantiateItemView(null))
            .ToList();
    }

    private ItemView InstantiateItemView(Item item)
    {
        var newItemSlot = Instantiate(prefabItemView, Vector3.zero, Quaternion.identity, transform);

        newItemSlot.transform.localPosition = Vector3.zero;
        newItemSlot.transform.localEulerAngles = Vector3.zero;
        newItemSlot.SetItem(item);

        return newItemSlot;
    }

    private void OnEquipSync(SyncList<Item>.Operation op, int itemIndex, Item oldItem, Item newItem)
    {
        switch (op)
        {
            case SyncList<Item>.Operation.OP_ADD:
                equipmentViews[itemIndex].SetItem(newItem);
                break;
            case SyncList<Item>.Operation.OP_SET:
                equipmentViews[itemIndex].SetItem(newItem);
                break;
            case SyncList<Item>.Operation.OP_INSERT:
                break;
            case SyncList<Item>.Operation.OP_REMOVEAT:
                equipmentViews[itemIndex].ClearSlot();
                break;
            case SyncList<Item>.Operation.OP_CLEAR:
                equipmentViews.ForEach(view => view.ClearSlot());
                break;
        }
    }
}
