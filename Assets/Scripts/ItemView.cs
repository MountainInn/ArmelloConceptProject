using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class ItemView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image icon;
    private Sprite blankIconSprite;
    private ItemPopup popup;

    private Item item;

    public Item Item => item;

    public event Action onLeftClick, onRightClick;

    private void Awake()
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
        if (item == null)
            return;

        popup.SetItem(item);
        popup.transform.position = eventData.pointerCurrentRaycast.worldPosition;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClick?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClick?.Invoke();
        }
    }
}
