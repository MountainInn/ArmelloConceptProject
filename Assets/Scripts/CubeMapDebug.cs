using UnityEngine;
using System.Linq;
using Zenject;
using System;
using MountainInn;
using System.Collections.Generic;

public class CubeMapDebug : MonoBehaviour
{
    public bool
        highlightNeighbours,
        highlightPath;

    Vector3Int start, end;
    CubeMap cubeMap;

    List<HexTile> path = new List<HexTile>();
    List<HexTile> neighbours = new List<HexTile>();
    HexTile startTile;

    [Inject]
    public void Construct(CubeMap cubeMap)
    {
        this.cubeMap = cubeMap;
        // cubeMap.onHexClicked += SetStart;
        // cubeMap.onHexPointerEnter += SetEnd;
        // cubeMap.onHexPointerEnter += FindPath;
        // cubeMap.onHexPointerEnter += FindNeighbours;
        // cubeMap.onHexPointerEnter += (coord)=>cubeMap[coord].HighlightMouseOver();
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
        path
            .Where(tile => tile != null)
            .ToList()
            .ForEach(tile => tile.RemoveHighlight());
        path.Clear();

        var result = cubeMap.FindPath(start, endCoord);

        var coord = endCoord;

        while (result[coord].HasValue)
        {
            var item = cubeMap[result[coord].Value];
            path.Add(item);
           
            if (highlightPath)
                item.HighlightPath();

            coord = result[coord].Value;
        }

        if (highlightPath && result[endCoord].HasValue)
            cubeMap[result[endCoord].Value].HighlightPath();
    }

    private void SetEnd(Vector3Int coord)
    {
        end = coord;
    }

    private void SetStart(Vector3Int coord)
    {
        if (cubeMap.tiles == null || !cubeMap.tiles.ContainsKey(coord))
            return;       

        if (startTile != null)
            startTile.RemoveHighlight();


        start = coord;
        startTile = cubeMap[coord];
        startTile.HighlightStart();
    }
}
