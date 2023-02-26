using UnityEngine;
using TMPro;

public class ItemPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemStats;
    [SerializeField] public CanvasGroup canvasGroup;

    private void Awake()
    {
        SetVisible(false);
    }

    public void SetVisible(bool toggle)
    {
        canvasGroup.alpha = (toggle) ? 1f : 0f;
    }

    public void SetItem(Item item)
    {
        itemName.text = item.name;
        itemStats.text = item.stats.ToString();
    }
}
