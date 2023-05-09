using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;
using UniRx;
using DG.Tweening;
using MountainInn;
using System.Collections.Generic;

public partial class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<Vector3Int> onClicked;
    public event Action<Vector3Int> onPointerEnter;
    public event Action<Vector3Int> onPointerExit;

    public Vector3Int coordinates
    {
        get => this.cubicTransform().coordinates;
        set => this.cubicTransform().coordinates = value;
    }

    [HideInInspector] [SyncVar] public HexType baseType, currentType;

    [SerializeField] private Transform topTransform;

    [HideInInspector] [SyncVar] public Character character;
    [HideInInspector] [SyncVar] public Transform flag;

    [HideInInspector] public bool isVisible = false;
    [HideInInspector] public Vector3 Top => transform.position + new Vector3(0, tileLevel.height, 0);

    private MeshRenderer meshRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    public Influence influence;
    public ResourceType resourceType;
    public int resourceAmount;
    public Aura aura;
    public ItemPlacement itemPlacement;
    public TileLevel tileLevel;
    private bool isMouseOver;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        influence = GetComponent<Influence>();
        aura = GetComponent<Aura>();
        itemPlacement = GetComponent<ItemPlacement>();

        tileLevel = GetComponent<TileLevel>();
        tileLevel.onLevelSync += SyncTileHeightIfVisible;

        SetColors();

        gameObject.name = $"{baseType} {coordinates}";
    }

    public override void OnStartServer()
    {
        NetworkServer.connections.Values.Map(conn => TargetToggleVisibility( conn, false));
    }

    public override void OnStartClient()
    {
        gameObject.name = $"{coordinates} {gameObject.name}";

        MessageBroker.Default
            .Publish(new msgSpawned(){ Value = this });
    }

    public struct msgSpawned { public HexTile Value; }

    private void OnDestroy()
    {
        onClicked = null;
        onPointerEnter = null;
        onPointerExit = null;
    }

    public void Initialize(HexSyncData syncData)
    {
        coordinates = syncData.coord;

        gameObject.name = $"{coordinates} {gameObject.name}";

        SetDirty();
    }

    private void SetColors()
    {
        baseColor = meshRenderer.material.color;
        warScreenColor = baseColor * .5f;
        meshRenderer.material.color = (isVisible) ? baseColor : warScreenColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(coordinates);

        if (isClient)
            MessageBroker.Default.Publish(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        HighlightMouseOver();
        onPointerEnter?.Invoke(coordinates);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        RemoveHighlight();
        onPointerExit?.Invoke(coordinates);
    }


    public void RemoveHighlight()
    {
        meshRenderer.material.color = (isVisible) ? baseColor : warScreenColor;
    }

    [TargetRpc]
    public void TargetToggleVisibility(NetworkConnectionToClient conn, bool toggle)
    {
        isVisible = toggle;

        if (isMouseOver)
        {
            HighlightMouseOver();
        }
        else
        {
            RemoveHighlight();
        }

        SyncTileHeightIfVisible();
    }

    [Client]
    private void SyncTileHeightIfVisible()
    {
        if (isVisible && tileLevel.ShouldSync())
        {
            var standingOnTop = new List<Transform>();

            if (character) standingOnTop.Add(character.transform);
            if (flag) standingOnTop.Add(flag.transform);

            tileLevel.SyncTileLevel(standingOnTop.ToArray());
        }
    }

    public static HexType GetRandomType()
    {
        return System.Enum.GetValues(typeof(HexType)).ArrayGetRandom<HexType>();
    }

    public void HighlightMouseOver()
    {
        meshRenderer.material.color = ((isVisible) ? baseColor : warScreenColor) * 1.1f;
    }

    public void HighlightPath()
    {
        meshRenderer.material.color = Color.yellow * 0.15f;
    }
    public void HighlightNeighbour()
    {
        meshRenderer.material.color = Color.blue * 0.15f;
    }

    public void HighlightStart()
    {
        meshRenderer.material.color = Color.blue * 0.15f;
    }
}

public enum HexType
{
    Forest, Mountain, Lake, Sand
}

public struct HexSyncData
{
    public Vector3Int coord;
    public HexType hexSubtype;
}
