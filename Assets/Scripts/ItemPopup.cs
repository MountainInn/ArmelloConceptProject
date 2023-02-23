using UnityEngine;
using TMPro;

public class ItemPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemStats;
    [SerializeField] public CanvasGroup canvasGroup;

    public void SetItem(Item item)
    {
        itemName.text = item.name;
        itemStats.text = item.stats.ToString();
    }
}
