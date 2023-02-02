using UnityEngine;
using System.Linq;
using Zenject;
using System;
using System.Collections.Generic;

public class CubeMapDebug : MonoBehaviour
{
    public bool
        highlightNeighbours,
        highlightPath;

    CubeMap.PathfindingNode start, end;
    CubeMap cubeMap;

    List<HexTile> path = new List<HexTile>();
    List<HexTile> neighbours = new List<HexTile>();
    HexTile startTile;

    [Inject]
    public void Construct(CubeMap cubeMap)
    {
        this.cubeMap = cubeMap;
        cubeMap.onGenerated += (tiles) =>
        {
            tiles.Values
                .ToList()
                .ForEach(hex =>
                {
                    hex.onClicked += SetStart;
                    hex.onPointerEnter += SetEnd;
                    hex.onPointerEnter += FindPath;
                    hex.onPointerEnter += FindNeighbours;
                    hex.onPointerEnter += (coord)=>cubeMap[coord].HighlightMouseOver();
                });
        };
    }

    private void FindNeighbours(Vector3Int coord)
    {
        if (highlightNeighbours)
            neighbours.ForEach(n => n.RemoveHighlight());

        neighbours =
            cubeMap.NeighbourTilesInRadius(1, coord)
            .ToList();

        if (highlightNeighbours)
            neighbours.ForEach(n => n.HighlightNeighbour());
    }

    private void FindPath(Vector3Int endCoord)
    {
        if (start is null)
            return;

        path.ForEach(tile => tile.RemoveHighlight());
        path.Clear();

        var result = cubeMap.FindPath(start.coord, endCoord);

        while (result.parent != null)
        {
            var item = cubeMap[result.coord];
            path.Add(item);
            result = result.parent;
            if (highlightPath)
                item.HighlightPath();
        }
        if (highlightPath && result[endCoord].HasValue)
            cubeMap[result[endCoord].Value].HighlightPath();
    }

    private void SetEnd(Vector3Int coord)
    {
        end = new CubeMap.PathfindingNode() { coord = coord };
    }

    private void SetStart(Vector3Int coord)
    {
        if (startTile is not null)
            startTile.RemoveHighlight();

        start = new CubeMap.PathfindingNode() { coord = coord };
        startTile = cubeMap[coord];
        startTile.HighlightStart();
    }
}
