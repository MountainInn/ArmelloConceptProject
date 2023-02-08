using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Mirror;
using Zenject;
using MountainInn;
using UniRx;

public class CubeMap : NetworkBehaviour
{
    [SyncVar]
    [Range(1, 3)]
    public int mapRadius;
    public int hexagonSize = 1;
    private HexTile[] hexagonPrefabs;

    public event Action onGenerated;

    public Dictionary<Vector3Int, HexTile> tiles { get; private set; }

    private HashSet<Vector3Int> pickedPositions = new HashSet<Vector3Int>();
    public IObservable<bool> onSynced;
    readonly public SyncList<HexSyncData> syncData = new SyncList<HexSyncData>();

    [Inject]
    public void Construct(EOSLobbyUI lobbyUI)
    {
        lobbyUI.onStartGameButtonClicked += () => Generate(mapRadius);
    }

    public void Awake()
    {
        tiles = new Dictionary<Vector3Int, HexTile>();
    }

    [ClientRpc]
    private void RpcOnGeneratedInvoke()
    {
        onGenerated?.Invoke();
    }

    public void OnEnable()
    {
        hexagonPrefabs = (HexTile[])Resources.LoadAll<HexTile>("Prefabs/Tiles/");
    }

    public void OnDisable()
    {
        if (isClient)
        {
            tiles.Clear();
        }
    }

    public override void OnStartClient()
    {
        syncData.Callback += OnHexSyncDataUpdated;
    }

    public override void OnStopClient()
    {
        syncData.Callback -= OnHexSyncDataUpdated;
        tiles.Clear();
    }


    public HexTile this[Vector3Int v3]
    {
        get => tiles[v3];
        set => tiles[v3] = value;
    }
    public HexTile this[int q, int r, int s]
    {
        get => tiles[new Vector3Int(q, r, s)];
        set => tiles[new Vector3Int(q, r, s)] = value;
    }

    [Client]
    void OnHexSyncDataUpdated(SyncList<HexSyncData>.Operation op, int index, HexSyncData oldItem, HexSyncData newItem)
    {
        switch (op)
        {
            case SyncList<HexSyncData>.Operation.OP_ADD:
                if (tiles.ContainsKey(newItem.coord))
                    break;

                var hex = CreateHex(newItem);
                hex.gameObject.name = $"{newItem.hexSubtype.ToString()} {newItem.coord.ToString()}";
                tiles.Add(hex.coordinates, hex);
                break;

            case SyncList<HexSyncData>.Operation.OP_CLEAR:
                tiles.Values.ToList().ForEach(hex => GameObject.Destroy(hex.gameObject));
                tiles.Clear();
                break;

            default:
                break;
        }

        if ( tiles.Count == ( MathExt.Fact(mapRadius) * 6 + 1 ))
        {
            onGenerated?.Invoke();
        }
    }

    [Server]
    public Dictionary<Vector3Int, HexTile> Generate(int radius)
    {
        if (syncData.Count > 0)
            syncData.Clear();

        if (tiles != null || tiles.Count > 0)
        {
            tiles.Values
                .Where(hex => hex != null)
                .ToList()
                .ForEach(hex => NetworkServer.Destroy(hex.gameObject));

            tiles.Clear();
            tiles = new Dictionary<Vector3Int, HexTile>();
        }

        var newSyncData =
            TilePositions(radius)
            .Select(CreateSyncData)
            .ToList();

        syncData.AddRange(newSyncData);

        return tiles;
    }

    HexSyncData CreateSyncData(Vector3Int coord)
    {
        return new HexSyncData()
        {
            coord = coord,
                hexSubtype = Hex.GetRandomType()
                };
    }

    HexTile CreateHex(HexSyncData syncData)
    {
        var coordinates = syncData.coord;

        var position = PositionFromCoordinates(coordinates);

        HexTile prefab = (HexTile)hexagonPrefabs
            .First(sr => sr.name == syncData.hexSubtype.ToString());

        var hexagon = Instantiate(prefab, position, Quaternion.identity, transform)
            .GetComponent<HexTile>();

        hexagon.Initialize(coordinates);

        return hexagon;
    }

    public Vector3 PositionFromCoordinates(Vector3Int coordinates)
    {
        if (positions.TryGetValue(coordinates, out Vector3 position))
            return position;

        float x = hexagonSize * (MathF.Sqrt(3) * coordinates.x + MathF.Sqrt(3) / 2 * coordinates.y);
        float y = hexagonSize * 3f / 2 * coordinates.y;

        return positions[coordinates] = new Vector3(x, y, 0) / 2;
    }

    public Vector3Int GetRandomCoordinates()
    {
        var coords = TilePositions(mapRadius);
        Vector3Int randomCoordinates;
        do
        {
            randomCoordinates = coords.GetRandom();
        }
        while (pickedPositions.Contains(randomCoordinates));

        pickedPositions.Add(randomCoordinates);

        return randomCoordinates;
    }


    public Dictionary<Vector3Int, Vector3Int?> FindPath(Vector3Int from, Vector3Int to)
    {
        SortedList<int, Vector3Int> priorityList = new SortedList<int, Vector3Int>(new DuplicateKeyComparer<int>());

        var cameFrom = new Dictionary<Vector3Int, Vector3Int?>();
        var costSoFar = new Dictionary<Vector3Int, int>();

        cameFrom.Add(from, null);
        costSoFar.Add(from, 0);
        priorityList.Add(0, from);

        while (priorityList.Count > 0)
        {
            var kv = priorityList.ElementAt(0);
            priorityList.RemoveAt(0);

            Vector3Int current = kv.Value;

            if (current == to)
            {
                break;
            }

            NeighbourCoordinatesInRadius(1, current)
                .ToList()
                .ForEach(next =>
                {
// graph.cost - Возвращает цену движения от текущего тайла
// к следующему, может пригодиться если разные типы тайлов
// будут иметь разную проходимость.
// Пока что просто поставлю 1.
// new_cost = cost_so_far[current] + graph.cost(current, next)
                    var newCost = costSoFar[current] + 1;

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + heuiristic(to, next);
                        priorityList.Add(priority, next);
                        cameFrom[next] = current;
                    }
                });
        }

        return cameFrom;
    }

    private int heuiristic(Vector3Int end, Vector3Int next)
    {
        return
            Math.Abs(end.x - next.x) +
            Math.Abs(end.y - next.y) +
            Math.Abs(end.z - next.z);
    }

    public IEnumerable<HexTile> NeighbourTilesInRadius(int radius, Vector3Int startCoordinates)
    {
        return
            NeighbourCoordinatesInRadius(radius, startCoordinates)
            .Select(i3 => this[i3]);
    }

    public IEnumerable<Vector3Int> NeighbourCoordinatesInRadius(int radius, Vector3Int startCoordinates)
    {
        var neighbours = new Vector3Int[][]
        {
            ImmediateNeighbourCoordinates(),
        };

        IEnumerable<Vector3Int> res = Enumerable.Empty<Vector3Int>();

        for (int i = 0; i < radius; i++)
        {
            if (i < neighbours.Length)
                res = res.Concat(neighbours[i]);
            else
            {
                res = res.Concat(TilePositionsRing(i));
            }
        }

        res = res
            .Select(v3 => v3 + startCoordinates)
            .Where(v3 => DistanceFromCenter(v3) <= mapRadius);

        return res;
    }

    private HexTile[] ImmediateNeighbourTiles()
    {
        return
            ImmediateNeighbourCoordinates()
            .Select(i3 => this[i3])
            .ToArray();
    }

    private static Vector3Int[] ImmediateNeighbourCoordinates()
    {
        return new Vector3Int[]
        {
            new Vector3Int(+1, 0, -1), new Vector3Int(+1, -1, 0), new Vector3Int(0, -1, +1),
            new Vector3Int(-1, 0, +1), new Vector3Int(-1, +1, 0), new Vector3Int(0, +1, -1),
        };
    }

    private int DistanceFromCenter(Vector3Int to)
    {
        return Distance(Vector3Int.zero, to);
    }

    public int Distance(Vector3Int from, Vector3Int to)
    {
        var vec = from - to;
        return (Math.Abs(vec.x) + Math.Abs(vec.y) + Math.Abs(vec.z)) / 2;
    }


    public IEnumerable<Vector3Int> TilePositions(int radius)
    {
        yield return new Vector3Int(0, 0, 0);

        for (int r = 1; r <= radius; r++)
        {
            foreach (var item in TilePositionsRing(r))
            {
                yield return item;
            }
        }
    }

    public IEnumerable<Vector3Int> TilePositionsRing(int radius)
    {
        int
            pillar = 0,
            full = 1,
            empty = 2;

        int rev = 0;

        int[] coords = new int[] { radius, -radius, 0 };

        do
        {
            while (coords[full] != 0)
            {
                int sign = Math.Sign(coords[full]);
                coords[full] -= sign;
                coords[empty] += sign;

                yield return new Vector3Int(coords[0], coords[1], coords[2]);
            }

            if (--pillar < 0) pillar += coords.Length;
            if (--full < 0) full += coords.Length;
            if (--empty < 0) empty += coords.Length;
        }
        while (++rev < 6);
    }

    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }
    }
}
