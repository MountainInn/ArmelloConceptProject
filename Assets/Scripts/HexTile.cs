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
    public bool isVisible = false;

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
