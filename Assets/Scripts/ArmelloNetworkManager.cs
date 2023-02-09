using Mirror;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private Player.Factory playerFactory;
    private TurnSystem turnSystem;

    [Inject]
    public void Construct(Player.Factory playerFactory, TurnSystem turnSystem, EOSLobbyUI lobbyUI)
    {
        this.playerFactory = playerFactory;
        this.turnSystem = turnSystem;

        lobbyUI.onStartGameButtonClicked += RegisterPlayersWithTurnSystem;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var  turnResolver = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Turn Resolver"));
        NetworkServer.Spawn(turnResolver.gameObject);
    }


    [Server]
    private void RegisterPlayersWithTurnSystem()
    {
        var players =
            GameObject.FindObjectsOfType<Player>()
            .ToList();

        turnSystem.RegisterPlayers(players);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = playerFactory.Create().gameObject;

        player.name = $"Player [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
