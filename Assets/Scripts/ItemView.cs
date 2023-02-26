using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class ItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;
    [SerializeField] Sprite blankIconSprite;
    [SerializeField] ItemPopup popup;
    private Item item;

    public void SetItem(Item item)
    {
        this.item = item;

        if (item != null)
        {
            icon.sprite = item.icon;
        }
        else
        {
            ClearSlot();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        popup.SetItem(item);
        popup.transform.position = eventData.position;
        popup.SetVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popup.SetVisible(false);
    }

    internal void ClearSlot()
    {
        icon.sprite = blankIconSprite;
    }
}
