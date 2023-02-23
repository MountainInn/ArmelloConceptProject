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

    public bool isVisible = false;
    public Vector3 Top => topTransform.position;
    [SyncVar] public Character character;


    private MeshRenderer meshRenderer;
    private Color baseColor, highlightColor, warScreenColor;

    [SyncVar(hook=nameof(OnTileActionTypeSync))] private TileActionType tileActionType;
    private void OnTileActionTypeSync(TileActionType oldv, TileActionType newv)
    {
        SetTileAction(newv);
        SetColors(newv);
    }
    [SyncVar]
    public UsableTile usableTile;
    internal Transform flag;
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
        usableTile = tileActionType switch
            {
                TileActionType.Mining => new MiningTile(this, ResourceType.GenericResource),
                TileActionType.Influence => new InfluenceTileResource(this, ResourceType.GenericResource),
                TileActionType.Trigger => new TriggerTileCharacterHealth(this),
                _ => throw new System.Exception("Not all TileActionTypes are handled")
            };
    }

    private void SetColors(TileActionType tileActionType)
    {
        baseColor = tileActionType switch
            {
                (TileActionType.Mining) => Color.green,
                (TileActionType.Influence) => Color.cyan,
                (TileActionType.Trigger) => new Color(.8f, .6f, .3f),
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


public abstract class UsableTile
{
    public HexTile hexTile;

    protected UsableTile(HexTile hexTile)
    {
        this.hexTile = hexTile;
    }

    abstract public void UseTile(Player player);
}

public enum TileActionType
{
    Mining, Influence, Trigger
}

public enum ResourceType
{
    GenericResource
}

public class MiningTile : UsableTile
{
    [SyncVar]
    public ResourceType resourceType;

    public MiningTile(HexTile hexTile, ResourceType resourceType) : base(hexTile)
    {
        this.resourceType = resourceType;
    }

    public override void UseTile(Player player)
    {
        MessageBroker.Default
            .Publish(new TileMinedMsg(){ player = player, resourceType = resourceType, amount = 1 });
        Debug.Log($"MiningTile: +1 Generic Resource");

    }

    public struct TileMinedMsg
    {
        public Player player;
        public ResourceType resourceType;
        public int amount;
    }
}

abstract public class InfluenceTile : UsableTile
{
    [SyncVar(hook=nameof(OnOwnerSync))]
    public Player owner;

    static private FlagPool flagPool;

    protected InfluenceTile(HexTile hexTile) : base(hexTile)
    {
        flagPool = MonoBehaviour.FindObjectOfType<FlagPool>();
    }

    [Server]
    public override void UseTile(Player player)
    {
        if (owner == player)
            return;
       
        MessageBroker.Default
            .Publish(new TileTakenMsg(){ previousOwner = owner, newOwner = player, tile = this, hexTile = hexTile});

        owner = player;
    }

    private void OnOwnerSync(Player previousOwner, Player newOwner)
    {
        flagPool.Return(previousOwner, hexTile);
        flagPool.Rent(newOwner, hexTile);
    }

    abstract public void InfluenceEffect(Player player);

    public struct TileTakenMsg
    {
        public Player previousOwner, newOwner;
        public InfluenceTile tile;
        public HexTile hexTile;
    }
}

public class InfluenceTileResource : InfluenceTile
{
    [SyncVar]
    public MiningTile miningTile;

    public InfluenceTileResource(HexTile hexTile, ResourceType resourceType) : base(hexTile)
    {
        this.miningTile = new MiningTile(hexTile, resourceType);
    }

    public override void InfluenceEffect(Player player)
    {
        miningTile.UseTile(player);
        Debug.Log($"InfluenceTile: +1 Generic resource");

    }
}


abstract public class TriggerTile : UsableTile
{
    protected TriggerTile(HexTile hexTile) : base(hexTile)
    {
    }

    public override void UseTile(Player player)
    {
        Trigger(player);
    }

    abstract public void Trigger(Player player);
}

public class TriggerTileCharacterHealth : TriggerTile
{
    public TriggerTileCharacterHealth(HexTile hexTile) : base(hexTile)
    {
    }

    public override void Trigger(Player player)
    {
        player.character.combatUnit.health += 10;
        Debug.Log($"TriggerTIle: +10 Health");
    }
}

static public class UsableTileSerializer
{
    const byte
        NO_SUBTYPE=0;
    const byte
        MINING=1;

    const byte
        INFLUENCE=2;
    const byte
        INF_RESOURCE=1;

    const byte
        TRIGGER=3;
    const byte
        TRIGGER_CHARACTER_HEALTH=1;

    public static void WriteUsableTile(this NetworkWriter writer, UsableTile usableTile)
    {
        writer.WriteNetworkBehaviour(usableTile.hexTile);
       
        if (usableTile is MiningTile miningTile)
        {
            writer.WriteByte(MINING);
            writer.WriteByte(NO_SUBTYPE);
            writer.WriteInt((int)miningTile.resourceType);
        }
        else if (usableTile is InfluenceTile influenceTile)
        {
            writer.WriteByte(INFLUENCE);

            if (influenceTile is InfluenceTileResource resTile)
            {
                writer.WriteByte(INF_RESOURCE);
                writer.WriteInt((int)resTile.miningTile.resourceType);
                writer.WriteUsableTile(resTile.miningTile);
            }

            if (influenceTile.owner)
                writer.WriteGameObject(influenceTile.owner.gameObject);
            else
                writer.WriteGameObject(null);
        }
        else if (usableTile is TriggerTile triggerTile)
        {
            writer.WriteByte(TRIGGER);

            if (triggerTile is TriggerTileCharacterHealth charHealth)
            {
                writer.WriteByte(TRIGGER_CHARACTER_HEALTH);
            }
        }
    }

    public static UsableTile ReadUsableTile(this NetworkReader reader)
    {
        HexTile hexTile = reader.ReadNetworkBehaviour<HexTile>();
        byte type = reader.ReadByte();
        byte subtype = reader.ReadByte();

        switch (type)
        {
            case MINING:
                return new MiningTile(hexTile, (ResourceType)reader.ReadInt());

            case INFLUENCE:
                Player owner = reader.ReadGameObject().GetComponent<Player>();
                switch (subtype)
                {
                    case INF_RESOURCE:
                        return new InfluenceTileResource(hexTile, (ResourceType)reader.ReadInt())
                        {
                            miningTile = (MiningTile)reader.ReadUsableTile(),
                            owner = owner
                        };
                    default:
                        throw new System.Exception($"Invalid InfluenceTile subtype");
                }

            case TRIGGER:
                switch (subtype)
                {
                    case TRIGGER_CHARACTER_HEALTH:
                        return new TriggerTileCharacterHealth(hexTile);
                    default:
                        throw new System.Exception($"Invalid TriggerTile subtype");
                }

            default:
                throw new System.Exception($"Invalid UsableTile supertype");
        };
    }
}

