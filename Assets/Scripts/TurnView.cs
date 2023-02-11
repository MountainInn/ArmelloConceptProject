using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using MountainInn;
using System;

public class TurnView : MonoBehaviour
{
    [SerializeField] Button endTurn;
    [SerializeField] TextMeshProUGUI turnText;

    public event Action onEndTurnClicked;

    private void Awake()
    {
        endTurn.onClick.AddListener(EndTurn);
    }

    public void Toggle(bool turnStarted)
    {
        endTurn.interactable = turnStarted;

        if (turnStarted)
        {
            endTurn.image.color = Color.green;
            turnText.text = "Your turn";
        }
        else
        {
            endTurn.image.color = Color.red;
            turnText.text = "Waiting for other players";
        }
    }

    private void EndTurn()
    {
        onEndTurnClicked?.Invoke();
    }
}
