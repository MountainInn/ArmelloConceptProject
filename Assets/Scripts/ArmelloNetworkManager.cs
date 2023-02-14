using Mirror;
using MountainInn;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private Player.Factory playerFactory;
    private TurnSystem srv_turnSystem;

    [Inject]
    public void Construct(Player.Factory playerFactory, EOSLobbyUI lobbyUI)
    {
        this.playerFactory = playerFactory;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var turnResolver = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Turn Resolver"));
        NetworkServer.Spawn(turnResolver.gameObject);

        srv_turnSystem =
            Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Turn System"))
            .GetComponent<TurnSystem>();

        NetworkServer.Spawn(srv_turnSystem.gameObject);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerGo = playerFactory.Create().gameObject;

        playerGo.name = $"Player [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, playerGo);

        srv_turnSystem.RegisterPlayer(playerGo.GetComponent<Player>());
    }


    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Player disconnectedPlayer =
            conn.owned
            .Single(netid => netid.GetComponent<Player>())
            .GetComponent<Player>();

        srv_turnSystem.UnregisterPlayer(disconnectedPlayer);

        base.OnServerDisconnect(conn);
    }

}
