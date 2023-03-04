using System;
using Mirror;
using UniRx;
using UnityEngine;
using MountainInn;
using System.Collections.Generic;

public class ItemSpawner : NetworkBehaviour
{
    ItemScriptableObject[] itemSOs;
    private ItemPlacement itemPlacement;
    Item prefabItem;

    List<(HexTile, Item)>
        itemsBuffer = new List<(HexTile, Item)>();

    public override void OnStartServer()
    {
        MessageBroker.Default
            .Receive<HexTile.msgSpawned>()
            .Subscribe(msg => SpawnItem(msg.Value))
            .AddTo(this);

        FindObjectOfType<CubeMap>()
            .onFullySpawned += ApplyItemBuffer;

        prefabItem = Resources.Load<Item>("Prefabs/Item");
        itemSOs = Resources.LoadAll<ItemScriptableObject>("Items");
        itemPlacement = FindObjectOfType<ItemPlacement>();
    }

    public override void OnStopServer()
    {
        FindObjectOfType<CubeMap>()
            .onFullySpawned -= ApplyItemBuffer;
    }

    [Server]
    private void SpawnItem(HexTile tile)
    {
        if (UnityEngine.Random.value > .5f)
            return;

        Vector3 position = GetItemPosition(tile);

        var newItem = Instantiate(prefabItem, position, Quaternion.identity, null);
        newItem.Initialize(itemSOs.GetRandomOrThrow());

        NetworkServer.Spawn(newItem.gameObject);

        itemsBuffer.Add((tile, newItem));
    }

    [Server]
    private void ApplyItemBuffer()
    {
        itemsBuffer
            .ForEach(tup => itemPlacement.PutItem(tup.Item1, tup.Item2));

        itemsBuffer.Clear();
    }

    static public Vector3 GetItemPosition(HexTile tile)
    {
        return tile.Top - Vector3.fwd * .3f + Vector3.up * .5f;
    }
}
