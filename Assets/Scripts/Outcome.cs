using System.Collections.Generic;
using Mirror;
using UniRx;
using System.Linq;
using UnityEngine;

public class Outcome : NetworkBehaviour
{
    TurnSystem turnSystem;
    OutcomeView outcomeView;

    private void Awake()
    {
        turnSystem = FindObjectOfType<TurnSystem>();
        outcomeView = GetComponent<OutcomeView>();
    }

    public override void OnStartServer()
    {
        var obsPlayerLost = MessageBroker.Default.Receive<OnPlayerLost>();

        obsPlayerLost
            .Subscribe(msg =>
                       this.StartInvokeAfter(() => OnPlayerLost(msg.player), 3))
            .AddTo(this);
    }

    [Server]
    private void OnPlayerLost(Player removedPlayer)
    {
        TargetLoss(removedPlayer.connectionToClient, removedPlayer);
        RpcLoss(removedPlayer);

        Player onlyPlayerLeft = turnSystem.players.SingleOrDefault();

        if (onlyPlayerLeft != default)
        {
            TargetVictory(onlyPlayerLeft.connectionToClient, onlyPlayerLeft);
        }
    }

    [TargetRpc]
    public void TargetLoss(NetworkConnectionToClient conn, Player player)
    {
        outcomeView.ShowLoss();
    }

    [ClientRpc]
    public void RpcLoss(Player player)
    {
        Debug.Log($"{player.name} Lost!");
    }

    [TargetRpc]
    public void TargetVictory(NetworkConnectionToClient conn, Player player)
    {
        outcomeView.ShowVictory();
    }
}
