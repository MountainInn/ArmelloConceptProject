using Mirror;
using MountainInn;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private const string GAME_SCENE_NAME = "Game Scene";
    private TurnSystem turnSystem;
    private Dictionary<int, NetworkConnectionToClient> stillLoadingClients;

    public event System.Action onStopClient;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        FindObjectOfType<EOSLobbyUI>()
            .onStartGameButtonClicked += () =>
            {
                ServerChangeScene(GAME_SCENE_NAME);
                stillLoadingClients = NetworkServer.connections;
            };

        NetworkServer.RegisterHandler<OnClientLoadedScene>(CheckAllClientsLoadedScene);
    }

    [Server]
    private void CheckAllClientsLoadedScene(NetworkConnectionToClient conn, OnClientLoadedScene msg)
    {
        stillLoadingClients.Remove(msg.connectionId);

        if (stillLoadingClients.Count == 0)
        {
            turnSystem.StartNextPlayerTurn();
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Player player = Instantiate(playerPrefab).GetComponent<Player>();

        DontDestroyOnLoad(player);

        string playerName = PlayerPrefs.GetString("Nickname");

        player.name = $"Player {playerName} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player.gameObject);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == GAME_SCENE_NAME)
        {
            var turnResolver = InstantiatePrefab<TurnResolver>("Turn Resolver");
            turnSystem = InstantiatePrefab<TurnSystem>("Turn System");

            NetworkServer.Spawn(turnResolver.gameObject);
            NetworkServer.Spawn(turnSystem.gameObject);

            NetworkServer.connections
                .Values
                .ToList()
                .ForEach(conn =>
                {
                    Player player = conn.GetSingleOwnedOfType<Player>();

                    Character newCharacter = InstantiatePrefab<Character>("Character");
                    newCharacter.player = player;
                    newCharacter.SetCharacterSO(player.characterSettings.characterSO);
                    newCharacter.characterColor = player.characterSettings.characterColor;
                    newCharacter.name = $"{player.name} Character";

                    Inventory newInventory = InstantiatePrefab<Inventory>("Inventory");
                    newInventory.name = $"{player.name} Inventory";
                  
                    player.SetCharacter(newCharacter);
                    player.SetInventory(newInventory);

                    NetworkServer.Spawn(newCharacter.gameObject, conn);
                    NetworkServer.Spawn(newInventory.gameObject, conn);

                    turnSystem.RegisterPlayer(player);
                });
        }
    }

    public override void OnClientSceneChanged()
    {
        if (networkSceneName == GAME_SCENE_NAME)
        {
            int connId = NetworkClient.connection.connectionId;
            NetworkClient.Send(new OnClientLoadedScene(){ connectionId = connId });
        }
    }

    public struct OnClientLoadedScene : NetworkMessage
    {
        public int connectionId;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Player disconnectedPlayer =
            conn.GetSingleOwnedOfType<Player>();

        turnSystem.UnregisterPlayer(disconnectedPlayer);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopClient()
    {
        onStopClient?.Invoke();
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
