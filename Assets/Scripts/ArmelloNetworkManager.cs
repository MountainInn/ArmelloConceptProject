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

        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();

        newPlayer.SetRoomPlayer(roomPlayer.GetComponent<ArmelloRoomPlayer>());

        return newPlayer.gameObject;
    }

    private T InstantiatePrefab<T>(string name)
    {
        return Instantiate(GetPrefab(name)).GetComponent<T>();
    }

    private GameObject GetPrefab(string name)
    {
        return spawnPrefabs.Find(prefab => prefab.name == name);
    }

}
