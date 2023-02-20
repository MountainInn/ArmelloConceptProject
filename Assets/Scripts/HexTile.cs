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

        if (character)
            character.transform.DOMoveY(1 + level + 0.5f, .3f);
    }

    [SerializeField]
    private Transform topTransform;

    public bool isVisible = false;
    public Vector3 Top => topTransform.position + new Vector3(0, 0.5f, 0);
    [SyncVar] public Character character;


    private MeshRenderer meshRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    [SyncVar(hook=nameof(OnTileActionTypeSync))] private TileActionType tileActionType;
    private void OnTileActionTypeSync(TileActionType oldv, TileActionType newv)
    {
        SetTileAction(newv);
        SetColors(newv);
    }
    private ITileAction tileAction;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        ToggleVisibility(false);

        gameObject.name = $"{baseType} {coordinates}";
    }

    public override void OnStartClient()
    {
        SetTileAction(tileActionType);

        SetColors(tileActionType);

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
        this.tileActionType = syncData.tileActionType;

        SetTileAction(syncData.tileActionType);

        SetColors(syncData.tileActionType);

        SetDirty();
    }

    private void SetTileAction(TileActionType tileActionType)
    {
        tileAction = tileActionType switch
        {
            TileActionType.Mining => new MiningTile(ResourceType.GenericResource),
            _ => throw new System.Exception("Not all TileActionTypes are handled")
        };
    }

    private void SetColors(TileActionType tileActionType)
    {
        baseColor = tileActionType switch
            {
                (TileActionType.Mining) => Color.green,
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
        Debug.Assert(tileAction != null);
        tileAction.TileAction(player);
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
    public TileActionType tileActionType;
}


public enum TileActionType
{
    Mining
}

public interface ITileAction
{
    void TileAction(Player player);
}

public enum ResourceType
{
    GenericResource
}

public class MiningTile : ITileAction
{
    ResourceType resourceType;

    public MiningTile(ResourceType resourceType)
    {
        this.resourceType = resourceType;
    }

    public void TileAction(Player player)
    {
        MessageBroker.Default
            .Publish(new TileMinedMsg(){ player = player, resourceType = resourceType, amount = 1 });
    }

    public struct TileMinedMsg
    {
        public Player player;
        public ResourceType resourceType;
        public int amount;
    }
}
