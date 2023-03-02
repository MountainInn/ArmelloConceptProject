using UnityEngine;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using Mirror;

public class FlagPool : NetworkBehaviour
{
    private ResourcePool<Transform> pool;

    Dictionary<HexTile, Transform> flagDict = new Dictionary<HexTile, Transform>();

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
    }

    [Server]
    private void Rent(Player player, HexTile tile)
    {
        if (flagDict.ContainsKey(tile))
            throw new System.Exception("(HexTile) key is already present in flagDict");

        var newFlag = pool.Rent();

        flagDict.Add(tile, newFlag);

        SetColor(newFlag.gameObject, player.roomPlayer.playerColor);

        newFlag.position = tile.Top;
        tile.flag = newFlag;

        NetworkServer.Spawn(newFlag.gameObject, player.gameObject);
    }

    [Server]
    private void Return(HexTile tile)
    {
        if (flagDict.TryGetValue(tile, out Transform flag))
            throw new System.Exception("(Player, HexTile) key is NOT present in flagDict");

        tile.flag = null;

        NetworkServer.UnSpawn(flag.gameObject);

        pool.Return(flag);
    }

    [Server]
    private void SetColor(GameObject flagGO, Color color)
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
