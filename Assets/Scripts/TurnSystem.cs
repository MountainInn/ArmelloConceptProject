using System.Linq;
using Mirror;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using MountainInn;
using UnityEngine;

public class TurnSystem : NetworkBehaviour
{
    readonly SyncDictionary<uint, Player> players = new SyncDictionary<uint, Player>();
    [SyncVar] int currentPlayerIndex = -1;

    [SyncVar(hook = nameof(SetPlayerNetIdStreamValue))]
    uint currentPlayerNetId = uint.MaxValue;

    [SyncVar] Player currentPlayer;

    ReactiveProperty<uint> playerNetIdStream = new ReactiveProperty<uint>(uint.MaxValue);

    IDisposable turnDisposable;

    private void Awake()
    {
        var lobbyUI = FindObjectOfType<EOSLobbyUI>();

        lobbyUI.onStartGameButtonClicked += CmdStartNextPlayerTurn;
    }

    [Command(requiresAuthority =false)]
    public void CmdRegisterPlayer(Player player)
    {
        RegisterPlayer(player);
    }

    [Command(requiresAuthority =false)]
    public void CmdUnregisterPlayer(Player player)
    {
        UnregisterPlayer(player);
    }

    [Server]
    public void UnregisterPlayer(Player player)
    {
        Debug.Log($"UnRegisterPlayer");
        players.Remove(player.netId);

        player.turn = null;

        if (currentPlayer == player)
            CmdStartNextPlayerTurn();
    }

    [Server]
    public void RegisterPlayer(Player player)
    {
        player.turn = new Turn(player.netId, playerNetIdStream);
        player.turn.started
            .Subscribe(player.TargetToggleTurnView);

        players.Add(player.netId, player);
    }

    [Server]
    public void CmdStartNextPlayerTurn()
    {
        if (players.Count == 0) return;
       
        currentPlayerIndex = (currentPlayerIndex+1) % players.Count;

        (uint nextNetId, Player nextPlayer) = players.ElementAt(currentPlayerIndex);

        turnDisposable?.Dispose();
        turnDisposable =
            nextPlayer.turn.completed
            .IsTrue()
            .Subscribe((b) => CmdStartNextPlayerTurn());

        currentPlayer = nextPlayer;

        playerNetIdStream.SetValueAndForceNotify(nextNetId);
        currentPlayerNetId = nextNetId;
    }

    private void SetPlayerNetIdStreamValue(uint oldNetId, uint newNetId)
    {
        playerNetIdStream.Value = newNetId;
    }
}
