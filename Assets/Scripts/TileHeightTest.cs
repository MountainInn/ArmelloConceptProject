using System.Linq;
using UnityEngine;

public class TileHeightTest : MonoBehaviour
{
    private HexTile currentHex;

    void Awake()
    {
        var cubemap = FindObjectOfType<CubeMap>();
        cubemap
            .onFullySpawned += () =>
            {
                cubemap.tiles.Values
                    .ToList()
                    .ForEach(hex =>
                             hex.onPointerEnter += (_) => { currentHex = hex; });
            };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            currentHex?.DecreaseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            currentHex?.IncreaseLevel();
        }
    }
}
