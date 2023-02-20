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

    [SyncVar(hook = nameof(OnCurrentPlayerNetIdSync))]
    uint currentPlayerNetId = uint.MaxValue;

    public List<Player> players = new List<Player>();
    [SyncVar] Player currentPlayer;

    ReactiveProperty<uint> playerNetIdStream = new ReactiveProperty<uint>(uint.MaxValue);
    IDisposable turnDisposable;
    public event Action onRoundEnd;

    private void Awake()
    {
        var lobbyUI = FindObjectOfType<EOSLobbyUI>();
        lobbyUI.onStartGameButtonClicked += StartNextPlayerTurn;
    }

    [Server]
    public void UnregisterPlayer(Player player)
    {
        players.Remove(player);

        player.turn = null;

        if (currentPlayer == player)
            StartNextPlayerTurn();
    }

    [Server]
    public void RegisterPlayer(Player player)
    {
        player.turn = new Turn(player.netId, playerNetIdStream);
        player.turn.started
            .Subscribe(b =>
            {
                player.TargetToggleTurnStarted(b);
            })
            .AddTo(player);

        players.Add(player);
        players = players.Shuffle().ToList();
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

            onRoundEnd?.Invoke();
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

    private void OnCurrentPlayerNetIdSync(uint oldNetId, uint newNetId)
    {
        playerNetIdStream.Value = newNetId;
    }
}
