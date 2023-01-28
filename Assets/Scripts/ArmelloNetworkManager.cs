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
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? playerFactory.Create(startPos.position, startPos.rotation).gameObject
            : playerFactory.Create().gameObject;

        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
