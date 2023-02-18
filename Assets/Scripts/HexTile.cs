using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using System;
using UniRx;
using DG.Tweening;
using MountainInn;

public partial class HexTile : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<Vector3Int> onClicked;
    public event Action<Vector3Int> onPointerEnter;
    public event Action<Vector3Int> onPointerExit;

    [SyncVar] public Vector3Int coordinates;
    [SyncVar] public HexType baseType, currentType;

    [SyncVar(hook = nameof(OnLevelSync))]
    public int level = 0;
    public int moveCost => level + 1;

    private void OnLevelSync(int oldv, int newv)
    {
        transform.DOScaleY(1 + level, .3f);
    }

    public bool isVisible = false;

    public Transform Top;

    private MeshRenderer meshRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        baseColor = UnityEngine.Random.ColorHSV() * .4f + new Color(.4f, .4f, .4f);
        warScreenColor = baseColor * .5f;

        meshRenderer.material.color = baseColor;

        ToggleVisibility(false);

        gameObject.name = $"{baseType} {coordinates}";
    }

    public override void OnStartClient()
    {
        MessageBroker.Default
            .Publish(new HexTileSpawned(){ Value = this });
    }

    public struct HexTileSpawned { public HexTile Value; }

    private void OnDestroy()
    {
        onClicked = null;
        onPointerEnter = null;
        onPointerExit = null;
    }

    public void Initialize(HexSyncData syncData)
    {
        this.coordinates = syncData.coord;
        baseType = (HexType)syncData.hexSubtype;

        SetDirty();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(coordinates);

        if (isClient)
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

    public void IncreaseLevel(int inc = 1)
    {
        level = level + inc;
    }

    public void DecreaseLevel(int dec = 1)
    {
        level = Math.Max(0, level - dec);
    }


    public void RemoveHighlight()
    {
        meshRenderer.material.color = (isVisible) ? baseColor : warScreenColor;
    }

    public void ToggleVisibility(bool toggle)
    {
        isVisible = toggle;
        meshRenderer.material.color = (isVisible) ? baseColor : warScreenColor;
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
