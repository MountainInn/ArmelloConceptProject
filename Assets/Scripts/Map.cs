using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using MountainInn;
using Mirror;

public class Map : NetworkBehaviour
{
    [RangeAttribute(1, 3)]
    [SerializeField] int radius;
    public TextMeshPro textPrefab;

    public event Action<List<HexTile>> onGenerated;

    public List<HexTile> hexTiles {get; private set;}
    [HideInInspector]
    public Tilemap tilemap;
    private HexTile[] tilePrefabs;
    private HashSet<Vector2Int> pickedPositions = new HashSet<Vector2Int>();


    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        tilePrefabs = Resources.LoadAll<HexTile>("Prefabs/Tiles/");
    }

    public override void OnStartServer()
    {
        RandomizeMap();

        onGenerated?.Invoke(hexTiles);
    }

    private void RandomizeMap()
    {
        hexTiles = new List<HexTile>();

        foreach ( var coord in TilePositions(radius) )
        {
            HexSyncData hexSyncData = new HexSyncData(Hex.GetRandomType(), coord.xy());

            var tilePrefab = (HexTile) tilePrefabs.First(sr => sr.name == hexSyncData.hexSubtype.ToString());

            var position = tilemap.GetCellCenterWorld(hexSyncData.coord.xy_());

            HexTile newTile = Instantiate(tilePrefab, position, Quaternion.identity, tilemap.transform);
            newTile.Initialize(hexSyncData.coord);

            hexTiles.Add(newTile);

            NetworkServer.Spawn(newTile.gameObject);
        }
    }

    public Vector2Int GetRandomCoordinates()
    {
        Vector2Int randomCoordinates;
        do
        {
            int y = UnityEngine.Random.Range(-radius, radius+1);

            MountainInn.GridExt.HexXBorders(radius, y, out int left, out int right);

            int x = UnityEngine.Random.Range(left, right+1);

            randomCoordinates = new Vector2Int(x, y);
        }
        while (pickedPositions.Contains(randomCoordinates));

        pickedPositions.Add(randomCoordinates);

        return randomCoordinates;
    }


    private IEnumerable<Vector3Int> TilePositions(int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            MountainInn.GridExt.HexXBorders(radius, y, out int left, out int right);

            for (int x = left; x <= right; x++)
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
