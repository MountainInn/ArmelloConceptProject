using UnityEngine;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using Mirror;

public class FlagPool : NetworkBehaviour
{
    private ResourcePool<Transform> pool;

    private void Awake()
    {
        pool = new ResourcePool<Transform>(FindObjectOfType<ArmelloNetworkManager>());
    }

    public override void OnStartServer()
    {
        MessageBroker.Default
            .Receive<Influence.msgTileCaptured>()
            .Subscribe(msg => Rent(msg.owner, msg.hexTile))
            .AddTo(this);

        MessageBroker.Default
            .Receive<Influence.msgTilePlundered>()
            .Subscribe(msg => Return(msg.hexTile))
            .AddTo(this);
    }

    [Server]
    private void Rent(Player player, HexTile tile)
    {
        var newFlag = pool.Rent();

        newFlag.cubicTransform().coordinates = tile.coordinates;
        newFlag.position = tile.Top;
        tile.flag = newFlag;

        newFlag
            .GetComponentsInChildren<MeshRenderer>()
            .ToList()
            .ForEach(mr => mr.material.color = player.roomPlayer.playerColor);

        NetworkServer.Spawn(newFlag.gameObject, player.gameObject);

        RpcSetColor(newFlag.gameObject, player.roomPlayer.playerColor);
    }

    [Server]
    private void Return(HexTile tile)
    {
        Transform flag = tile.flag;

        tile.flag = null;

        NetworkServer.UnSpawn(flag.gameObject);

        pool.Return(flag);
    }

    [ClientRpc]
    private void RpcSetColor(GameObject flagGO, Color color)
    {
        flagGO
            .GetComponentsInChildren<MeshRenderer>()
            .ToList()
            .ForEach(mr => mr.material.color = color);
    }
}

public class ResourcePool<T> : UniRx.Toolkit.ObjectPool<T>
    where T : UnityEngine.Component
{
    protected ArmelloNetworkManager netman;

    public ResourcePool(ArmelloNetworkManager netman)
    {
        this.netman = netman;
    }

    protected override T CreateInstance()
    {
        var obj = netman.InstantiatePrefab<T>("Flag");

        return obj;
    }
}
