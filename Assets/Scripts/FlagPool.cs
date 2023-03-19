using UnityEngine;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using Mirror;
using MountainInn;

public class FlagPool : NetworkBehaviour
{
    private ResourcePool<Transform> pool;

    private void Awake()
    {
        pool = new ResourcePool<Transform>("Prefabs/Flag");
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

        MessageBroker.Default.Receive<OnPlayerLost>()
            .Subscribe(msg =>
                       FindObjectsOfType<Influence>()
                       .Where(inf => inf.owner == msg.player)
                       .Select(inf => inf.GetComponent<HexTile>())
                       .Map(tile => Return(tile))
            )
            .AddTo(this);
    }

    [Server]
    private void Rent(Player player, HexTile tile)
    {
        var newFlag = pool.Rent();

        newFlag.position = tile.Top;
        tile.flag = newFlag;

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
    protected T prefab;

    public ResourcePool(string resourcePath)
    {
        prefab = Resources.Load<T>(resourcePath);
    }

    protected override T CreateInstance()
    {
        var obj = GameObject.Instantiate(prefab);

       

        return obj;
    }
}
