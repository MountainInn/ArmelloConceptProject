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
    public void Construct(Player.Factory playerFactory, EOSLobbyUI lobbyUI)
    {
        this.playerFactory = playerFactory;

        lobbyUI.onStartGameButtonClicked += StartNextPlayerTurn;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var turnResolver = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Turn Resolver"));
        NetworkServer.Spawn(turnResolver.gameObject);

        turnSystem =
            Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Turn System"))
            .GetComponent<TurnSystem>();

        NetworkServer.Spawn(turnSystem.gameObject);
    }

    [Server]
    private void StartNextPlayerTurn()
    {
        turnSystem.CmdStartNextPlayerTurn();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = playerFactory.Create().gameObject;

        player.name = $"Player [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        turnSystem.RenewPlayers();
    }

}
