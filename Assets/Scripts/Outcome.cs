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
        var obsPlayerUnregistered = MessageBroker.Default.Receive<TurnSystem.msgPlayerUnregistered>();

        var obsPlayerRemoved =
            Observable.CombineLatest(obsPlayerLost,
                                     obsPlayerUnregistered,
                                     (a, b) => (a.player, b.player))
            .Where(tup => tup.Item1 == tup.Item2)
            .Select(tup => tup.Item1);

        obsPlayerRemoved
            .Subscribe(player => this.StartInvokeAfter(() => OnPlayerRemoved(player), 3))
            .AddTo(this);
    }

    [Server]
    private void OnPlayerRemoved(Player removedPlayer)
    {
        TargetLoss(removedPlayer);
        RpcLoss(removedPlayer);

        Player onlyPlayerLeft = turnSystem.players.SingleOrDefault();

        if (onlyPlayerLeft != default)
        {
            TargetVictory(onlyPlayerLeft);
        }
    }

    [TargetRpc]
    public void TargetLoss(Player player)
    {
        outcomeView.ShowLoss();
    }

    [ClientRpc]
    public void RpcLoss(Player player)
    {
        Debug.Log($"{player.name} Lost!");
    }

    [TargetRpc]
    public void TargetVictory(Player player)
    {
        outcomeView.ShowVictory();
    }
}
