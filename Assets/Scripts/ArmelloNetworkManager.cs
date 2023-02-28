using Mirror;
using MountainInn;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private const string GAME_SCENE_NAME = "Game Scene";
    private TurnSystem turnSystem;
    private GameStartNotifier gameStartNotifier;

    public event System.Action onStopClient;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var turnResolver = InstantiatePrefab<TurnSystem>("Turn Resolver");
        turnSystem = InstantiatePrefab<TurnSystem>("Turn System");
        gameStartNotifier = InstantiatePrefab<GameStartNotifier>("Game Start Notifier");

        NetworkServer.Spawn(turnResolver.gameObject);
        NetworkServer.Spawn(turnSystem.gameObject);
        NetworkServer.Spawn(gameStartNotifier.gameObject);

        FindObjectOfType<EOSLobbyUI>()
            .onStartGameButtonClicked += () => ServerChangeScene(GAME_SCENE_NAME);
    }


    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Player player = Instantiate(playerPrefab).GetComponent<Player>();

        player.name = $"Player [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player.gameObject);

        turnSystem.RegisterPlayer(player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Player disconnectedPlayer =
            conn.owned
            .Single(netid => netid.GetComponent<Player>())
            .GetComponent<Player>();

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
