using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using MountainInn;
using Mirror;
using Zenject;

public class Map : NetworkBehaviour
{
    [RangeAttribute(1, 3)]
    [SerializeField] int radius;
    public TextMeshPro textPrefab;

    public event Action<List<HexTile>> onGenerated;

    public List<HexTile> hexTiles {get; private set;}
    public event Action<HexTile> onTileCreated;
    [HideInInspector]
    public Tilemap tilemap;
    private HexTile[] tilePrefabs;
    private HashSet<Vector3Int> pickedPositions = new HashSet<Vector3Int>();

    readonly public SyncList<HexSyncData> hexSyncs = new SyncList<HexSyncData>();

    [Inject]
    public void Construct(EOSLobbyUI lobbyUI)
    {
        lobbyUI.onStartGameButtonClicked += RandomizeMap;
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        tilePrefabs = Resources.LoadAll<HexTile>("Prefabs/Tiles/");
        hexTiles = new List<HexTile>();
    }

    public override void OnStartClient()
    {
        Debug.Log("_Map OnStartCLient");
        hexSyncs.Callback += OnHexSyncDataUpdated;
    }

    void OnHexSyncDataUpdated(SyncList<HexSyncData>.Operation op, int index, HexSyncData oldItem, HexSyncData newItem)
    {
        Debug.Log(op);
        switch (op)
        {
            case SyncList<HexSyncData>.Operation.OP_ADD:
                var newTile = CreateTile(newItem);
                hexTiles.Add(newTile);
                Debug.Log("--+Sync Tile") ;
                break;

            case SyncList<HexSyncData>.Operation.OP_CLEAR:
                hexTiles.ForEach(h => GameObject.Destroy(h.gameObject));
                hexTiles.Clear();
                Debug.Log("--+Clear Tiles") ;

                break;
            default:
                break;
        }
    }

    public void RandomizeMap()
    {
        hexTiles = new List<HexTile>();
        var hexData = new List<HexSyncData>();

        foreach (var coord in TilePositions(radius))
        {
            HexSyncData hexSync = new HexSyncData
            {
                coord = coord.xy(),
                hexSubtype = Hex.GetRandomType()
            };
            hexData.Add(hexSync);
        }

        hexSyncs.AddRange(hexData);

        onGenerated?.Invoke(hexTiles);
    }

    private HexTile CreateTile(HexSyncData hexSyncData)
    {
        var tilePrefab = (HexTile)tilePrefabs.First(sr => sr.name == hexSyncData.hexSubtype.ToString());

        var position = tilemap.GetCellCenterWorld(hexSyncData.coord.xy_());

        HexTile newTile = Instantiate(tilePrefab, position, Quaternion.identity, tilemap.transform);
        newTile.Initialize(hexSyncData.coord.xy_());

        hexTiles.Add(newTile);

        onTileCreated?.Invoke(newTile);
       
        return newTile;
    }

    public Vector3Int GetRandomCoordinates()
    {
        Vector3Int randomCoordinates;
        do
        {
            int y = UnityEngine.Random.Range(-radius, radius + 1);

            MountainInn.GridExt.HexXBorders(radius, y, out int left, out int right);

            int x = UnityEngine.Random.Range(left, right + 1);

            randomCoordinates = new Vector3Int(x, y);
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
                yield return new Vector3Int(x, y, 0);
            }
        }
    }

    private void InstantiateCoordText(Vector3Int coord)
    {
        var coordText = Instantiate(textPrefab, tilemap.GetCellCenterWorld(coord), Quaternion.identity);
        coordText.text = $"({coord.x}, {coord.y})";
    }


}
