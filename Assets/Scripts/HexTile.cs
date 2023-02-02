using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<Vector3Int> onClicked;
    public event Action<Vector3Int> onPointerEnter;
    public event Action<Vector3Int> onPointerExit;

    public Vector3Int coordinates {get; private set;}
    private SpriteRenderer spriteRenderer;
    private Color baseColor, highlightColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        highlightColor = baseColor * 1.1f;
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
        spriteRenderer.color = baseColor;
    }

    public void HighlightMouseOver()
    {
        spriteRenderer.color = highlightColor;
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
