using Mirror;
using System.Linq;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private Player.Factory playerFactory;
    private TurnSystem turnSystem;

    [Inject]
    public void Construct(Player.Factory playerFactory, TurnSystem turnSystem)
    {
        this.playerFactory = playerFactory;
        this.turnSystem = turnSystem;
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        Player player = conn.owned
            .Select(netid => netid.gameObject.GetComponent<Player>())
            .Where(p => p != null)
            .First();

        turnSystem.AddPlayer(player);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = playerFactory.Create().gameObject;

        player.name = $"Player [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
