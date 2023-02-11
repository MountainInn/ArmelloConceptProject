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

    public override void OnStartClient()
    {
        CmdRenewPlayers();
    }

    [Command(requiresAuthority =false)]
    public void CmdRenewPlayers()
    {
        RenewPlayers();
    }

    public void RenewPlayers()
    {
        HashSet<Player>
            oldPlayers = players.Values.ToHashSet(),
            newPlayers = FindObjectsOfType<Player>().ToHashSet();

        IEnumerable<Player>
            addedPlayers = newPlayers.Except(oldPlayers),
            removedPlayers = oldPlayers.Except(newPlayers);


        removedPlayers.Log("Removed Players");
        removedPlayers.ToList()
            .ForEach(player => players.Remove(player.netId));

        addedPlayers.Log("Added Players");
        addedPlayers.ToList()
            .ForEach(player =>
            {
                player.turn = new Turn(player.netId, playerNetIdStream);
                player.turn.started
                    .Subscribe(player.TargetToggleTurnView);

                players.Add(player.netId, player);
            });


        var removedCurrentPlayer =
            removedPlayers
            .FirstOrDefault(player => (player.netId == currentPlayerNetId));

        if (removedCurrentPlayer != default)
            CmdStartNextPlayerTurn();

        Debug.Log($"Player count: {players.Count()}");
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
