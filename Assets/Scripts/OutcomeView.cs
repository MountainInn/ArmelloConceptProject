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

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        leaveButton.onClick.AddListener(LeaveGame);
    }

    private void Start()
    {
        canvasGroup.SetVisibleAndInteractable(false);
    }


    private void LeaveGame()
    {
        NetworkClient.Disconnect();
    }


    public void ShowLoss()
    {
        outcomeText.text = "You lose!";
        canvasGroup.SetVisibleAndInteractable(true);
    }

    public void ShowVictory()
    {
        outcomeText.text = "You win!";
        canvasGroup.SetVisibleAndInteractable(true);
    }
}
