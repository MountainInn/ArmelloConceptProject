using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class ItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image icon;
    private Sprite blankIconSprite;
    private ItemPopup popup;

    private Item item;


    private void Start()
    {
        icon = GetComponent<Image>();
        popup = FindObjectOfType<ItemPopup>();

        blankIconSprite = Resources.Load<Sprite>("Sprites/Blank Icon");
    }

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
