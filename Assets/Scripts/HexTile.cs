using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using MountainInn;
using UniRx;

public partial class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public event Action<Vector3Int> onClicked;
    public event Action<Vector3Int> onPointerEnter;
    public event Action<Vector3Int> onPointerExit;

    [SyncVar]
    public Vector3Int coordinates;
    [SyncVar]
    public HexType baseType, currentType;
    public bool isVisible = false;

    private SpriteRenderer spriteRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        warScreenColor = baseColor * .5f;

        ToggleVisibility(false);

        gameObject.name = $"{baseType} {coordinates}";
    }

    public void Start()
    {
        MessageBroker.Default
            .Publish(new HexTileSpawned());
    }

    public struct HexTileSpawned {}

    private void OnDestroy()
    {
        onClicked = null;
        onPointerEnter = null;
        onPointerExit = null;
    }

    public void Initialize(HexSyncData syncData)
    {
        this.coordinates = syncData.coord;
        baseType = (HexType) syncData.hexSubtype;

        SetDirty();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(coordinates);

        MessageBroker.Default.Publish(this);
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
        spriteRenderer.color = (isVisible) ? baseColor : warScreenColor;
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

public struct SpawnHexTileMessage : NetworkMessage
{
    public SpawnMessage spawnMessage;
    public Vector3Int coordinates;
}
