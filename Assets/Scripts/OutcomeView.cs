using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class OutcomeView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI outcomeText;
    [SerializeField] Button leaveButton;

    CanvasGroup canvasGroup;
    private ViewQueue viewQueue;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        viewQueue = FindObjectOfType<ViewQueue>();

        leaveButton.onClick.AddListener(LeaveGame);
    }

    private void Start()
    {
        canvasGroup.SetVisibleAndInteractable(false);
    }

    private void LeaveGame()
    {
        NetworkClient.Disconnect();
        NetworkServer.Shutdown();
    }

    public void ShowLoss()
    {
        outcomeText.text = "You lose!";
        viewQueue.Add(canvasGroup);
    }

    public void ShowVictory()
    {
        outcomeText.text = "You win!";
        viewQueue.Add(canvasGroup);
    }
}
