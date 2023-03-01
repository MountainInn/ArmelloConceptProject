using Mirror;
using UniRx;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using MountainInn;

public class Influence : NetworkBehaviour
{
    static private GameSettings gameSettings => _gameSettings ??= Resources.Load<GameSettings>("GameSettings");
    static private GameSettings _gameSettings;

    readonly SyncDictionary<uint, int> influencePoints = new SyncDictionary<uint, int>();
    [SyncVar] public Player owner;

    private HexTile hexTile;

    IDisposable incomeDisposable;

    private void Awake()
    {
        hexTile = GetComponent<HexTile>();
    }

    public bool HasOwner() => owner != null;
    public bool IsNotOwnedBy(Player player) => owner != player;

    [Server]
    public void WorkOn(Player player)
    {
        if (HasOwner() && IsNotOwnedBy(player))
        {
            Plunder(player);
            return;
        }

        if (IsNotOwnedBy(player))
        {
            if (++influencePoints[player.netId] == gameSettings.influenceThreshold)
            {
                Capture(player);
            }
        }

        Income(player);
    }

    [Server]
    private void Capture(Player player)
    {
        owner = player;

        incomeDisposable =
            MessageBroker.Default
            .Receive<TurnSystem.OnRoundEnd>()
            .Where(_ => HasOwner())
            .Subscribe(msg => Income(owner));

        MessageBroker.Default.Publish(new msgTileCaptured(player, hexTile));
    }

    [Server]
    private void Income(Player player)
    {
        player.inventory.AddResource(hexTile.resourceType, hexTile.resourceAmount);
    }

    [Server]
    private void Plunder(Player player)
    {
        owner = null;
        incomeDisposable.Dispose();

        influencePoints
            .Keys
            .ToList()
            .ForEach(key => influencePoints[key] = 0);

        player.inventory.AddResource(hexTile.resourceType, hexTile.resourceAmount * gameSettings.plunderMultiplier);

        MessageBroker.Default.Publish(new msgTilePlundered(player, hexTile));
    }

    public struct msgTileCaptured
    {
        public Player owner;
        public HexTile hexTile;

        public msgTileCaptured(Player owner, HexTile hexTile)
        {
            this.owner = owner;
            this.hexTile = hexTile;
        }
    }

    public struct msgTilePlundered
    {
        public Player plunderer;
        public HexTile hexTile;

        public msgTilePlundered(Player plunderer, HexTile hexTile)
        {
            this.plunderer = plunderer;
            this.hexTile = hexTile;
        }
    }
}
