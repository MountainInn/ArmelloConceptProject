using Mirror;
using UnityEngine;
using Zenject;

public class ArmelloNetworkManager : NetworkManager
{
    private Player.Factory playerFactory;

    [Inject]
    public void Construct(Player.Factory playerFactory)
    {
        this.playerFactory = playerFactory;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = playerFactory.Create().gameObject;

        player.name = $"Player [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
