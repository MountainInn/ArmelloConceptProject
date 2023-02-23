using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class ItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;
    [SerializeField] ItemPopup popup;
    private Item item;

    public void SetItem(Item item)
    {
        this.item = item;

        icon.sprite = item.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        popup.SetItem(item);
        popup.transform.position = eventData.position;
        popup.canvasGroup.alpha = 1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popup.canvasGroup.alpha = 0;
    }
}
