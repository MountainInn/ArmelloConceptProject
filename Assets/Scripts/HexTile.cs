using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<Vector2Int> onHexClicked;

    public Vector2Int coordinates {get; private set;}
    private SpriteRenderer spriteRenderer;
    private Color baseColor, highlightColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        highlightColor = baseColor * 1.1f;
    }

    public void Initialize(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onHexClicked?.Invoke(coordinates);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        spriteRenderer.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.color = baseColor;
    }
}
