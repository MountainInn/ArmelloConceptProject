using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;

public class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public class MorphingRule
    {
        public HexType resultType;
        public Dictionary<HexType, int> requiredNeighbours;

        public MorphingRule(HexType resultType, params (HexType, int)[] requiredNeighbours)
        {
            this.resultType = resultType;
            this.requiredNeighbours =
                requiredNeighbours.ToDictionary(tup => tup.Item1, tup => tup.Item2);
        }

        public HexType? TryToMorph(CubeMap cubeMap, Vector3Int coord)
        {
            if (CheckNeighbours(cubeMap, coord))
                return resultType;

            return null;
        }

        public bool CheckNeighbours(CubeMap cubeMap, Vector3Int coord)
        {
            Dictionary<HexType, int> count = new Dictionary<HexType, int>(requiredNeighbours);

            var neighbours = cubeMap.NeighbourTilesInRadius(1, coord);

            foreach (var item in neighbours)
            {
                if (!count.ContainsKey(item.currentType))
                    return false;

                count[item.currentType]--;
            }

            return count.Values.Count(v => v > 0) == 0;
        }
    }

    public event Action<Vector3Int> onClicked;
    public event Action<Vector3Int> onPointerEnter;
    public event Action<Vector3Int> onPointerExit;

    public Vector3Int coordinates {get; private set;}
    public bool isVisible = false;
    public HexType baseType, currentType;

    private SpriteRenderer spriteRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        warScreenColor = baseColor *.5f;

        ToggleVisibility(false);
    }

    private void OnDestroy()
    {
        onClicked = null;
        onPointerEnter = null;
        onPointerExit = null;
    }

    public void Initialize(Vector3Int coordinates)
    {
        this.coordinates = coordinates;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(coordinates);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HighlightMouseOver();
        onPointerEnter?.Invoke(coordinates);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RemoveHighlight();
        onPointerExit?.Invoke(coordinates);
    }

    public void RemoveHighlight()
    {
        spriteRenderer.color = (isVisible)? baseColor : warScreenColor;
    }

    public void ToggleVisibility(bool toggle)
    {
        isVisible = toggle;
        spriteRenderer.color = (isVisible) ? baseColor : warScreenColor;
    }

    public void HighlightMouseOver()
    {
        spriteRenderer.color = ((isVisible) ? baseColor : warScreenColor) * 1.1f;
    }

    public void HighlightPath()
    {
        spriteRenderer.color = Color.yellow * 0.15f;
    }
    public void HighlightNeighbour()
    {
        spriteRenderer.color = Color.blue * 0.15f;
    }
    public void HighlightStart()
    {
        spriteRenderer.color = Color.blue * 0.15f;
    }
}

public enum HexType
{
    Forest, Mountain, Lake, Sand
}
