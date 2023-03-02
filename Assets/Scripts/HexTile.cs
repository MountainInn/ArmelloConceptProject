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
    [HideInInspector] [SyncVar] public HexType baseType, currentType;

    [SyncVar(hook = nameof(OnLevelSync))]
    public int level = 0;

    public int moveCost => level + 1;

    private void OnLevelSync(int oldv, int newv)
    {
        transform.DOScaleY(tileScale, .3f);

        if (character)
            character.transform.DOMoveY(Top.y, .3f);

        if (flag)
            flag.transform.DOMoveY(Top.y, .3f);
    }
    private float tileScale => 1 + level;
    private float tileHeight => tileScale * 0.5f;

    [SerializeField] private Transform topTransform;

    [HideInInspector] [SyncVar] public Character character;
    [HideInInspector] [SyncVar] public Item item;

    [HideInInspector] public bool isVisible = false;
    [HideInInspector] public Vector3 Top => topTransform.position;

    private MeshRenderer meshRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    public Transform flag;

    public Influence influence;
    public ResourceType resourceType;
    public int resourceAmount;
    public Aura aura;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        influence = GetComponent<Influence>();
        aura = GetComponent<Aura>();

        SetColors();

        gameObject.name = $"{baseType} {coordinates}";
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
        this.coordinates = syncData.coord;

        gameObject.name = $"{coordinates} {gameObject.name}";

        SetDirty();
    }

    private void SetColors()
    {
        baseColor = meshRenderer.material.color;
        warScreenColor = baseColor * .5f;

        ToggleVisibility(false);
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
