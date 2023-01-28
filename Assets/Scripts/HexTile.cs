using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    Vector2Int coordinates;
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
        Debug.Log("Hex!");
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
