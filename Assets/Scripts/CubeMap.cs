using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class CubeMap : MonoBehaviour
{
    [Range(1, 3)]
    public int mapRadius;
    public int hexagonSize = 1;
    public HexTile hexagonPrefab;

    public event Action<Dictionary<Vector3Int, HexTile>> onGenerated;

    Dictionary<Vector3Int, HexTile> tiles;

    void Start()
    {
        Generate(mapRadius);
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

    public Dictionary<Vector3Int, HexTile> Gen(int radius)
    {
        tiles = new Dictionary<Vector3Int, HexTile>();

        var center = new Vector3Int(0, 0, 0);

        tiles.Add(center, CreateHex(center));

        for (int r = 1; r <= radius; r++)
        {
            int
                pillar = 0,
                full = 1,
                empty = 2;

            int rev = 0;

            int[] coords = new int[] { r, -r, 0 };

            do
            {
                while (coords[full] != 0)
                {
                    int sign = Math.Sign(coords[full]);
                    coords[full] -= sign;
                    coords[empty] += sign;

                    var c = new Vector3Int(coords[0], coords[1], coords[2]);
                    tiles.Add(c, CreateHex(c));
                }

                if (--pillar < 0) pillar += coords.Length;
                if (--full < 0) full += coords.Length;
                if (--empty < 0) empty += coords.Length;
            }
            while (++rev < 6);
        }

        Debug.Log("Count: " + tiles.Count);
        return tiles;
    }

    public Dictionary<Vector3Int, HexTile> Generate(int radius)
    {
        tiles =
            TilePositions(radius)
            .Select(coord => (coord, CreateHex(coord)))
            .ToDictionary(kv => kv.coord, kv => kv.Item2);

        Debug.Log($"Count: {tiles.Count}");

        onGenerated?.Invoke(tiles);

        return tiles;

    }

    HexTile CreateHex(Vector3Int coordinates)
    {
        float x = hexagonSize * (MathF.Sqrt(3) * coordinates.x + MathF.Sqrt(3) / 2 * coordinates.y);
        float y = hexagonSize * 3f / 2 * coordinates.y;

        var position = new Vector3(x, y, 0) / 2;

        var go = Instantiate(hexagonPrefab, position, Quaternion.identity, transform)
            .GetComponent<HexTile>();

        go.Initialize(coordinates);

        Debug.Log(coordinates.ToString());

        return go;
    }

    public PathfindingNode FindPath(Vector3Int from, Vector3Int to)
    {
        SortedList<int, PathfindingNode> priorityList = new SortedList<int, PathfindingNode>(new DuplicateKeyComparer<int>());

        Dictionary<Vector3Int, PathfindingNode> nodes =
            tiles.Select(kv =>
            {
                var node = new PathfindingNode()
                {
                    coord = kv.Key,
                    distance = int.MaxValue,
                    rootDistance = int.MaxValue
                };

                if (from == node.coord)
                {
                    node.rootDistance = 0;
                    priorityList.Add(0, node);
                }

                node.InitManhattanDistance(to);

                return node;
            })
            .ToDictionary((node) => node.coord,
                          (node) => node);

        int minDistance = int.MaxValue;

        while (priorityList.Count > 0)
        {
            var kv = priorityList.ElementAt(0);
            priorityList.RemoveAt(0);

            var item = kv.Value;
            item.visited = true;

            NeighbourCoordinatesInRadius(1, item.coord)
                .Select(i3 => nodes[i3])
                .Where(n => !n.visited)
                .ToList()
                .ForEach(n =>
                {
                    n.rootDistance = Math.Min(n.rootDistance, item.rootDistance + 1);

                    minDistance = Math.Min(n.distance, n.rootDistance + n.manhattanDistance);

                    if (minDistance != n.distance)
                    {
                        n.distance = (int)minDistance;
                        n.parent = item;

                        // Change queue priority of the neighbor
                        if (priorityList.ContainsValue(n))
                        {
                            int index = priorityList.IndexOfValue(n);
                            priorityList.RemoveAt(index);
                            priorityList.Add(minDistance, n);
                        }
                    }

                    if (!priorityList.ContainsValue(n))
                        priorityList.Add(0, n);

                });
        }

        return nodes[to];
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

    public class PathfindingNode
    {
        public bool
            visited;

        public Vector3Int
            coord;

        public int
            distance,
            rootDistance,
            manhattanDistance;

        public PathfindingNode
            parent;

        public void InitManhattanDistance(Vector3Int end)
        {
            manhattanDistance =
                Math.Abs(end.x - coord.x) +
                Math.Abs(end.y - coord.y) +
                Math.Abs(end.z - coord.z);
        }
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
