using UnityEngine;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using System;

public class FlagPool : MonoBehaviour
{
    private ResourcePool<Transform> pool;

    Dictionary<Tuple<Player, HexTile>, Transform> flagDict = new Dictionary<Tuple<Player, HexTile>, Transform>();

    private void Awake()
    {
        pool = new ResourcePool<Transform>("Prefabs/Flag");
    }

    public Transform Rent(Player player, HexTile tile)
    {
        if (flagDict.ContainsKey(Tuple.Create(player, tile)))
            throw new System.Exception("(Player, HexTile) key is already present in flagDict");

        var newFlag = pool.Rent();

        flagDict.Add(Tuple.Create(player, tile), newFlag);

        SetColor(newFlag.gameObject, player.clientCharacterColor);

        newFlag.position = tile.Top;
        tile.flag = newFlag;

        return newFlag;
    }

    public void Return(Player player, HexTile tile)
    {
        if (flagDict.TryGetValue(Tuple.Create(player, tile), out Transform flag))
            throw new System.Exception("(Player, HexTile) key is NOT present in flagDict");

        pool.Return(flag);
        tile.flag = null;
    }

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
