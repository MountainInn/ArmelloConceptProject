using UnityEngine;
using TMPro;
using UniRx;

public class ItemPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemStats;
    [SerializeField] TextMeshProUGUI requiredResources;
    [SerializeField] public CanvasGroup canvasGroup;

    private Inventory localPlayerInventory;

    private void Awake()
    {
        SetVisible(false);

        System.IDisposable recieveLocalPlayerInventory = default;
       
        recieveLocalPlayerInventory =
            MessageBroker.Default
            .Receive<Player.msgOnLocalPlayerStarted>()
            .DoOnCompleted(() => recieveLocalPlayerInventory.Dispose())
            .Subscribe(msg => localPlayerInventory = msg.player.inventory);
    }

    public void SetVisible(bool toggle)
    {
        canvasGroup.alpha = (toggle) ? 1f : 0f;
    }

    public void SetItem(Item item)
    {
        itemName.text = item.name;
        itemStats.text = item.stats.ToString();

        requiredResources.text =
            (localPlayerInventory.Recipes.Contains(item))
            ? item.RequiredResourcesAsString()
            : "No Recipe";
    }
}
