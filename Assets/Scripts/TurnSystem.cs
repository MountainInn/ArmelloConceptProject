using UnityEngine;
using System.Linq;
using Mirror;
using System;
using System.Collections.Generic;
using UniRx;
using MountainInn;

public class TurnSystem : NetworkBehaviour
{
    readonly SyncDictionary<uint, Player> players = new SyncDictionary<uint, Player>();
    [SyncVar] int currentPlayerIndex = -1;

    [SyncVar(hook = nameof(SetPlayerNetIdStreamValue))]
    uint currentPlayerNetId = uint.MaxValue;

    [SyncVar] Player currentPlayer;

    ReactiveProperty<uint> playerNetIdStream = new ReactiveProperty<uint>(uint.MaxValue);

    IDisposable turnDisposable;

    public void RegisterPlayers(List<Player> newPlayers)
    {
        players.Clear();

        newPlayers
            .Shuffle()
            .ToList()
            .ForEach((p) =>
            {
                p.turn = new Turn(p.netId, playerNetIdStream);
                players.Add(p.netId, p);
            });


        StartNextPlayerTurn();
    }


    public void StartNextPlayerTurn()
    {
        currentPlayerIndex = (currentPlayerIndex+1) % players.Count;

        (uint nextNetId, Player nextPlayer) = players.ElementAt(currentPlayerIndex);


        turnDisposable?.Dispose();
        turnDisposable =
            nextPlayer.turn.completed
            .Subscribe((b) =>
            {
                if (b)
                {
                    StartNextPlayerTurn();
                }
            });

        currentPlayer = nextPlayer;

        if (isServer)
        {
            playerNetIdStream.SetValueAndForceNotify(nextNetId);
            currentPlayerNetId = nextNetId;
        }
    }

    private void SetPlayerNetIdStreamValue(uint oldNetId, uint newNetId)
    {
        playerNetIdStream.Value = newNetId;
    }
}
