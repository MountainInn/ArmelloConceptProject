using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using MountainInn;

public class GridGenerator : MonoBehaviour
{
    [RangeAttribute(1, 3)]
    [SerializeField] int radius;
    public TextMeshPro textPrefab;

    public event Action onMapGenerated;

    private Tilemap tilemap;
    private Canvas canvas;
    private List<SpriteRenderer> tiles;

    private void Awake()
    {
        canvas = GameObject.FindObjectOfType<Canvas>();
        tilemap = GetComponentInChildren<Tilemap>();

        tiles = Resources.LoadAll<SpriteRenderer>("Prefabs/Tiles").ToList();
    }

    private void Start()
    {

        Generate(radius);
    }

    private void Generate(int radius)
    {
        foreach ( var coord in TilePositions(radius) )
        {
            InstantiateCoordText(coord);

            InstantiateRandomTile(coord);
        }

        onMapGenerated?.Invoke();
    }

    private void InstantiateRandomTile(Vector3Int coord)
    {
        var randomPrefab = tiles.GetRandom();

        var position = tilemap.GetCellCenterWorld(coord);

        var tile = Instantiate(randomPrefab, position, Quaternion.identity, tilemap.transform);

        // tilemap.SetTile(new Vector3Int(x, y, 0) , );
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
