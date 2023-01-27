using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using MountainInn;
using Mirror;

public class GridGenerator : NetworkBehaviour
{
    [RangeAttribute(1, 3)]
    [SerializeField] int radius;
    public TextMeshPro textPrefab;

    public event Action onMapGenerated;

    private Tilemap tilemap;
    private Canvas canvas;

    private List<Hex> hexes;
    private readonly SyncList<HexSyncData> hexesSyncList = new SyncList<HexSyncData>();

    private HexTile[] tiles;

    private void Awake()
    {
        canvas = GameObject.FindObjectOfType<Canvas>();
        tilemap = GetComponentInChildren<Tilemap>();

        hexes = new List<Hex>();
        hexesSyncList.Callback += OnHexesChanged;

        tiles = Resources.LoadAll<HexTile>("Prefabs/Tiles/");
    }

    private void OnHexesChanged(SyncList<HexSyncData>.Operation op, int itemIndex, HexSyncData oldItem, HexSyncData newItem)
    {
        switch (op)
        {
            case SyncList<HexSyncData>.Operation.OP_ADD:
                RealizeHexesSyncData(newItem);
                break;

            case SyncList<HexSyncData>.Operation.OP_INSERT:
                break;

            case SyncList<HexSyncData>.Operation.OP_REMOVEAT:
                break;

            case SyncList<HexSyncData>.Operation.OP_SET:
                break;

            case SyncList<HexSyncData>.Operation.OP_CLEAR:
                break;
        }
    }

    public override void OnStartServer()
    {
        RandomizeHexesSyncData();
    }

    private void RandomizeHexesSyncData()
    {
        foreach ( var coord in TilePositions(radius) )
        {
            HexSyncData hexSyncData = new HexSyncData(Hex.GetRandomType(), coord.xy());

            hexesSyncList.Add(hexSyncData);
        }
    }

    private void RealizeHexesSyncData(HexSyncData hexSyncData)
    {
        Hex hex = Hex.RealizeSyncData(hexSyncData);
        var tilePrefab = (HexTile) tiles.First(sr => sr.name == hexSyncData.hexSubtype.ToString());

        hexes.Add(hex);

        var position = tilemap.GetCellCenterWorld(hexSyncData.coord.xy_());

        HexTile tile = Instantiate(tilePrefab, position, Quaternion.identity, tilemap.transform);
        tile.Initialize(hexSyncData.coord);

        NetworkServer.Spawn(tile.gameObject);
    }

    private IEnumerable<Vector3Int> TilePositions(int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            int absY = Math.Abs(y);

            int leftBorder = -radius + (absY)  /2;
            int rightBorder = radius - (absY+1)/2;

            for (int x = leftBorder; x <= rightBorder; x++)
            {
                yield return new Vector3Int(x,y, 0);
            }
        }
    }

    private void InstantiateCoordText(Vector3Int coord)
    {
        var coordText = Instantiate(textPrefab, tilemap.GetCellCenterWorld(coord), Quaternion.identity);
        coordText.text = $"({coord.x}, {coord.y})";
    }


}
