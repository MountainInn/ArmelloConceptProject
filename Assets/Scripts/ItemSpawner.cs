using System;
using Mirror;
using UniRx;
using UnityEngine;
using MountainInn;

public class ItemSpawner : NetworkBehaviour
{
    ItemScriptableObject[] itemSOs;

    Item prefabItem;

    public override void OnStartServer()
    {
        MessageBroker.Default
            .Receive<HexTile.msgSpawned>()
            .Subscribe(msg => SpawnItem(msg.Value))
            .AddTo(this);

        prefabItem = Resources.Load<Item>("Prefabs/Item");
        itemSOs = Resources.LoadAll<ItemScriptableObject>("Items");
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

        ItemPlacement placement = newItem.GetComponent<ItemPlacement>();
        placement.PutItem(tile);
    }

    static public Vector3 GetItemPosition(HexTile tile)
    {
        return tile.Top - Vector3.fwd * .3f + Vector3.up * .5f;
    }
}
