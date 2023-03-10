using UnityEngine;
using TMPro;
using UniRx;

public class RoundCountView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roundCountText;

    private void Awake()
    {
        roundCountText = GetComponentInChildren<TextMeshProUGUI>();

        MessageBroker.Default
            .Receive<TurnSystem.msgRoundEnd>()
            .Subscribe(msg => roundCountText.text = $"Round: {msg.roundCount}")
            .AddTo(this);
    }
}
