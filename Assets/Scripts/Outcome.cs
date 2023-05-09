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
            .Subscribe(msg => OnPlayerLost(msg.player))
            .AddTo(this);
    }

    [Server]
    private void OnPlayerLost(Player removedPlayer)
    {
        TargetLoss(removedPlayer.connectionToClient, removedPlayer);
        RpcLoss(removedPlayer);

        if (turnSystem.players.Count == 1)
        {
            Player onlyPlayerLeft = turnSystem.players.Single();
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
