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

    [SerializeField]
    private Transform topTransform;

    [HideInInspector] [SyncVar] public Character character;
    [HideInInspector] [SyncVar] public Item item;
    [HideInInspector] [SyncVar] public UsableTile usableTile;

    [HideInInspector] public bool isVisible = false;
    [HideInInspector] public Vector3 Top => topTransform.position;

    private MeshRenderer meshRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    [SyncVar(hook=nameof(OnUsableTileTypeSync))]
    [SerializeField]
    private UsableTileType usableTileType;

    private void OnUsableTileTypeSync(UsableTileType oldv, UsableTileType newv)
    {
        InitUsableTile(newv);
        SetColors(newv);
    }
    internal Transform flag;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        ToggleVisibility(false);

        gameObject.name = $"{baseType} {coordinates}";
    }

    public override void OnStartClient()
    {
        InitUsableTile(usableTileType);

        SetColors(usableTileType);

        gameObject.name = $"{coordinates} {usableTileType}";

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

        InitUsableTile(usableTileType);

        SetColors(usableTileType);

        gameObject.name = $"{coordinates} {usableTileType}";

        SetDirty();
    }

    private void InitUsableTile(UsableTileType usableTileType)
    {
        usableTile = usableTileType switch
            {
                UsableTileType.Mining => new MiningTile(this, ResourceType.GenericResource),
                UsableTileType.AutoMining => new InfluenceTileResource(this, ResourceType.GenericResource),
                UsableTileType.HealthBonus => new TriggerTileCharacterHealth(this),
                _ => throw new System.Exception("Not all UsableTileTypes are handled")
            };
    }

    private void SetColors(UsableTileType usableTileType)
    {
        baseColor = usableTileType switch
            {
                (UsableTileType.Mining) => Color.green,
                (UsableTileType.AutoMining) => Color.cyan,
                (UsableTileType.HealthBonus) => new Color(.8f, .6f, .3f),
                (_) => Color.magenta
            };
        warScreenColor = baseColor * .5f;

        ToggleVisibility(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(coordinates);

        if (isClient)
            MessageBroker.Default.Publish(this);
    }

    [Server]
    public void UseTile(Player player)
    {
        Debug.Assert(usableTile != null);
        usableTile.UseTile(player);
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

    [Client]
    public bool CanUseTile(Player player)
    {
        if (usableTile == null)
        {
            return false;
        }
        if (usableTile is TriggerTile)
        {
            return false;
        }
        else if (usableTile is InfluenceTile influenceTile)
        {
            return influenceTile.owner != player;
        }

        return true;
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
    public TileActionType tileActionType;
}
