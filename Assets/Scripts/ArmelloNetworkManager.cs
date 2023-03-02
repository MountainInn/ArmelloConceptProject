using Mirror;
using MountainInn;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkRoomManager
{
    private const string GAME_SCENE_NAME = "SampleScene";
    private TurnSystem turnSystem;

    private int loadingPlayers;

    public override void OnStartServer()
    {
        base.OnStartServer();

        MessageBroker.Default
            .Receive<TurnSystem.msgOnPlayerRegistered>()
            .Subscribe(_ => CheckIfAllPlayersLoaded())
            .AddTo(this);
    }

    private void CheckIfAllPlayersLoaded()
    {
       if (--loadingPlayers == 0)
       {
           FindObjectOfType<TurnSystem>()
               .StartNextPlayerTurn();
       }
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        ++loadingPlayers;
        // loadingPlayers = NetworkServer.connections.Select(kv => kv.Key).ToHashSet();
        // loadingPlayers.Log("LoadingPlayers");

        return null;
    }

    // [Server]
    // public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    // {
    //     turnSystem = FindObjectOfType<TurnSystem>();
    //     turnSystem.RegisterPlayer(gamePlayer.GetComponent<Player>());

    //     loadingPlayers.Remove(conn.connectionId);

    //     if (loadingPlayers.Count == 0)
    //     {
    //         turnSystem.StartNextPlayerTurn();
    //     }

    //     return true;
    // }


    // [Server]
    // public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    // {
    //     Player disconnectedPlayer =
    //         conn.GetSingleOwnedOfType<Player>();

    //     turnSystem.UnregisterPlayer(disconnectedPlayer);

    //     base.OnRoomServerDisconnect(conn);
    // }

    private T InstantiatePrefab<T>(string name)
    {
        return Instantiate(GetPrefab(name)).GetComponent<T>();
    }

    private GameObject GetPrefab(string name)
    {
        return spawnPrefabs.Find(prefab => prefab.name == name);
    }

}
