using System.Linq;
using Mirror;
using System;
using UniRx;
using MountainInn;
using UnityEngine;
using System.Collections.Generic;

public class TurnSystem : NetworkBehaviour
{
    [SyncVar] int currentPlayerIndex = -1;
    [SyncVar(hook=nameof(OnRoundCountSync))] int roundCount = 0;

    private void OnRoundCountSync(int oldv, int newv)
    {
        MessageBroker.Default.Publish<msgRoundEnd>(new msgRoundEnd(){ roundCount = newv });
    }

    [SyncVar(hook = nameof(OnCurrentPlayerNetIdSync))]
    uint currentPlayerNetId = uint.MaxValue;

    public List<Player> players = new List<Player>();
    [SyncVar] Player currentPlayer;

    ReactiveProperty<uint> playerNetIdStream = new ReactiveProperty<uint>(uint.MaxValue);
    IDisposable turnDisposable;

    public override void OnStartServer()
    {
        roundCount = 1;

        MessageBroker.Default
            .Receive<OnPlayerLost>()
            .Subscribe(msg => UnregisterPlayer(msg.player))
            .AddTo(this);

        MessageBroker.Default
            .Receive<Player.msgOnLocalPlayerStarted>()
            .Subscribe(msg => RegisterPlayer(msg.player))
            .AddTo(this);
    }

    [Command(requiresAuthority = false)]
    public void CmdRegisterPlayer(Player player)
    {
        RegisterPlayer(player);
    }

    public struct msgOnPlayerRegistered{}
    public struct msgPlayerUnregistered{ public Player player; }

    [Server]
    public void UnregisterPlayer(Player player)
    {
        if (!players.Contains(player))
            return;

        players.Remove(player);

        player.turn = null;

        MessageBroker.Default.Publish(new msgPlayerUnregistered{ player = player });

        if (currentPlayerNetId == player.netId
            && players.Count > 1
        )
            StartNextPlayerTurn();
    }

    [Server]
    public void RegisterPlayer(Player player)
    {
        if (players.Contains(player))
            return;

        player.turn = new Turn(player.netId, playerNetIdStream);
        player.turn.started
            .Subscribe(b =>
            {
                player.TargetToggleTurnStarted(b);
            })
            .AddTo(player);

        players.Add(player);
        players = players.Shuffle().ToList();

        MessageBroker.Default.Publish(new msgOnPlayerRegistered());
    }

    [Server]
    public void StartNextPlayerTurn()
    {
        if (players.Count == 0) return;

        currentPlayerNetId = uint.MaxValue;

        currentPlayerIndex = (currentPlayerIndex + 1);

        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex %= players.Count;

            EndRound();
        }

        Player nextPlayer = players.ElementAt(currentPlayerIndex);

        turnDisposable?.Dispose();
        turnDisposable =
            nextPlayer.turn.completed
            .IsTrue()
            .Subscribe((b) => StartNextPlayerTurn());

        currentPlayer = nextPlayer;
        currentPlayerNetId = nextPlayer.netId;
    }

    [Server]
    private void EndRound()
    {
        roundCount++;
        MessageBroker.Default.Publish<msgRoundEnd>(new msgRoundEnd() { roundCount = roundCount });
    }

    private void OnCurrentPlayerNetIdSync(uint oldNetId, uint newNetId)
    {
        playerNetIdStream.Value = newNetId;
    }

    public struct msgRoundEnd { public int roundCount; }
}
