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

    public bool isFullySpawned { get; private set; }
    public event Action onFullySpawned;

    private HexTile[] hexagonPrefabs;

    public Dictionary<Vector3Int, HexTile> tiles { get; private set; }

    private HashSet<Vector3Int> pickedPositions;

    [SyncVar]
    int expectedTileCount;
    int spawnedTileCount;

    public void Awake()
    {
        tiles = new Dictionary<Vector3Int, HexTile>();

        expectedTileCount = GetTileCount();

        MessageBroker.Default
            .Receive<HexTile.msgSpawned>()
            .Subscribe(spawned =>
            {
                var hex = spawned.Value;

                tiles.Add(hex.coordinates, hex);

                if (isFullySpawned = (++spawnedTileCount == expectedTileCount))
                {
                    onFullySpawned?.Invoke();
                    spawnedTileCount = 0;

                    MessageBroker.Default.Publish(new msgFullySpawned());
                }
            })
            .AddTo(this);
    }

    public struct msgFullySpawned {}

    public override void OnStartServer()
    {
        pickedPositions = new HashSet<Vector3Int>();
        hexagonPrefabs =
            Resources.LoadAll<HexTile>("Prefabs/3D Tiles/")
            .Where(tile => tile.name != "Base Tile")
            .ToArray();

        Generate(mapRadius);
    }

    public override void OnStopClient()
    {
        isFullySpawned = false;
        spawnedTileCount = 0;
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

    [Server]
    public void Generate(int radius)
    {
        expectedTileCount = GetTileCount();

        if (tiles.Any())
        {
            tiles.Values
                .NotEqual(null)
                .Select(hex => hex.gameObject)
                .ToList()
                .ForEach(NetworkServer.Destroy);

            tiles.Clear();
        }

        TileCoordinates(radius)
            .Select(CreateSyncData)
            .Select(CreateHex)
            .ToList()
            .ForEach(tile => NetworkServer.Spawn(tile.gameObject));
    }

    private int GetTileCount()
    {
        return MathExt.Fact(mapRadius) * 6 + 1;
    }

    HexSyncData CreateSyncData(Vector3Int coord)
    {
        return new HexSyncData()
        {
            coord = coord,
                };
    }

    HexTile CreateHex(HexSyncData syncData)
    {
        var coordinates = syncData.coord;

        var position = PositionFromCoordinates(coordinates);

        position = new Vector3(position.x, 0, position.y);

        HexTile prefab = (HexTile)hexagonPrefabs.ArrayGetRandom<HexTile>();

        var hexagon = Instantiate(prefab, position, Quaternion.identity)
            .GetComponent<HexTile>();

        hexagon.Initialize(syncData);

        return hexagon;
    }

    public Vector3 PositionFromCoordinates(Vector3Int coordinates)
    {
        if (tiles.TryGetValue(coordinates, out HexTile tile))
            return tile.transform.position;

        float x = hexagonSize * (MathF.Sqrt(3) * coordinates.x + MathF.Sqrt(3) / 2 * coordinates.y);
        float y = hexagonSize * 3f / 2 * coordinates.y;

        return new Vector3(x, y, 0) / 2;
    }

    public Vector3Int GetRandomCoordinates()
    {
        var coords = TileCoordinates(mapRadius);
        Vector3Int randomCoordinates;
        do
        {
            randomCoordinates = coords.GetRandomOrDefault();
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
                res = res.Concat(TileCoordinatesRing(i + 1));
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

    public int Distance(CubicTransform from, CubicTransform to)
    {
        var vec = from.coordinates - to.coordinates;
        return (Math.Abs(vec.x) + Math.Abs(vec.y) + Math.Abs(vec.z)) / 2;
    }


    public IEnumerable<Vector3Int> TileCoordinates(int radius)
    {
        yield return new Vector3Int(0, 0, 0);

        for (int r = 1; r <= radius; r++)
        {
            foreach (var item in TileCoordinatesRing(r))
            {
                yield return item;
            }
        }
    }

    public IEnumerable<Vector3Int> TileCoordinatesRing(int radius)
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
