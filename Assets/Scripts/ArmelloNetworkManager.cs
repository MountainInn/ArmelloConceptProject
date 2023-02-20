using Mirror;
using MountainInn;
using System.Linq;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private Player.Factory playerFactory;
    private TurnSystem turnSystem;

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

        turnSystem =
            Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Turn System"))
            .GetComponent<TurnSystem>();

        NetworkServer.Spawn(turnSystem.gameObject);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Player player = playerFactory.Create();

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

}
