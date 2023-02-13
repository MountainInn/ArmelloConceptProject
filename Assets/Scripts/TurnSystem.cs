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

    private Player localPlayer;

    IDisposable turnDisposable;

    private void Awake()
    {
        var lobbyUI = FindObjectOfType<EOSLobbyUI>();
        lobbyUI.onJoinLobbySuccess += CmdRegisterLocalPlayer;
        lobbyUI.onPreLeaveLobbySuccess += CmdUnregisterLocalPlayer;
        lobbyUI.onStartGameButtonClicked += CmdStartNextPlayerTurn;
    }

    private void Start()
    {
        localPlayer =
            FindObjectsOfType<Player>()
            .Single(p => p.isLocalPlayer);
    }

    [Command(requiresAuthority =false)]
    public void CmdRegisterLocalPlayer()
    {
        CmdRegisterPlayer(localPlayer);
    }

    [Command(requiresAuthority =false)]
    public void CmdRegisterPlayer(Player player)
    {
        player.turn = new Turn(player.netId, playerNetIdStream);
        player.turn.started
            .Subscribe(player.TargetToggleTurnView);

        players.Add(player.netId, player);
    }

    [Command(requiresAuthority =false)]
    public void CmdUnregisterLocalPlayer()
    {
        CmdUnregisterPlayer(localPlayer);
    }

    [Command(requiresAuthority =false)]
    public void CmdUnregisterPlayer(Player player)
    {
        players.Remove(player.netId);

        if (currentPlayer == player)
            CmdStartNextPlayerTurn();
    }

    [Command(requiresAuthority=false)]
    public void CmdStartNextPlayerTurn()
    {
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
