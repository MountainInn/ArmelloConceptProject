using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class OutcomeUI : MonoBehaviour
{
    VisualElement root;
    Image outcomeBackround;
    Label outcomeLabel;
    Button leaveButton;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        outcomeBackround = root.Q<Image>("OutcomeBackground");
        outcomeLabel = root.Q<Label>("OutcomeLabel");
        leaveButton = root.Q<Button>("LeaveButton");


        leaveButton.clicked += LeaveGame;
    }

    private void Start()
    {
        outcomeBackround.ToggleInClassList("fade-in");
    }

    private void LeaveGame()
    {
        NetworkClient.Disconnect();
    }

    public void ShowLoss()
    {
        outcomeLabel.text = "You lose!";
    }

    public void ShowVictory()
    {
        outcomeLabel.text = "You win!";
    }
}
