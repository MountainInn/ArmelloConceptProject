using Mirror;
using UnityEngine;
using DG.Tweening;
using Zenject;
using System.Linq;
using System;
using UniRx;

public class Character : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCoordinatesSync))]
    public Vector3Int coordinates;

    public Color characterColor;

    [Range(1, 10)]
    public int moveRadius = 1;
    public CombatUnit combatUnit => GetComponent<CombatUnit>();
    new public MeshRenderer renderer;
    public event Action<Character> onCharacterMoved;

    [SyncVar]
    public UtilityStats utilityStats;

    [SyncVar] public Player player;
    public Inventory inventory;
    private IDisposable onLostFightSubscription;

    public CubeMap cubeMap;

    public void SetCharacterSO(CharacterScriptableObject characterSO)
    {
        // this.gameObject.name = player.name +" "+ characterSO.characterName;
        this.renderer.material.SetTexture("_MainTex", characterSO.characterSprite.texture);
        this.combatUnit.SetCharacterStats(characterSO.combatStats);
        this.utilityStats = characterSO.utilityStats;
    }

    private void Awake()
    {
        cubeMap = FindObjectOfType<CubeMap>();
    }

    [Server]
    public override void OnStartServer()
    {
        onLostFightSubscription =
            MessageBroker.Default
            .Receive<OnLostFight>()
            .Where(msg => msg.loser == combatUnit)
            .Subscribe(OnLostFight);

        this.player.character = this;
    }

    public override void OnStartClient()
    {
        if (isOwned)
        {
            if (cubeMap.isFullySpawned)
                CmdInitializeCoordinates();
            else
                cubeMap.onFullySpawned += CmdInitializeCoordinates;
        }
    }


    [Server]
    private void OnLostFight(OnLostFight msg)
    {
        if (--utilityStats.health == 0)
        {
            Debug.Log($"Hearts: {utilityStats.health}");
            onLostFightSubscription.Dispose();

            MessageBroker.Default
                .Publish<OnPlayerLost>(new OnPlayerLost(){ player = player });
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdInitializeCoordinates()
    {
        var coord = cubeMap.GetRandomCoordinates();

        var position = cubeMap.PositionFromCoordinates(coord);

        var hex = cubeMap[coord];

        RpcMove(hex, useTween: false);
    }   

    [Command(requiresAuthority = false)]
    public void CmdMove(HexTile hex)
    {
        Vector3Int coordinates = hex.coordinates;
        Vector3 position = cubeMap.PositionFromCoordinates(coordinates);

        RpcMove(hex, true);
    }

    [ClientRpc]
    public void RpcMove(HexTile hex, bool useTween)
    {
        Vector3 position = hex.Top;
        Vector3Int coordinates = hex.coordinates;

        if (useTween)
        {
            transform
                .DOMove(position, .5f)
                .OnKill(() => CmdSetCoordinates(coordinates));
        }
        else
        {
            transform.position = position;
            CmdSetCoordinates(coordinates);
        }

    }

    [Command(requiresAuthority = false)]
    private void CmdSetCoordinates(Vector3Int coordinates)
    {
        GetHexTile().character = null;

        this.coordinates = coordinates;

        HexTile hex = cubeMap[coordinates];

        hex.character = this;

        if (hex.usableTile is TriggerTile bonusTile)
        {
            bonusTile.UseTile(player);
        }
    }

    [Client]
    private void OnCoordinatesSync(Vector3Int oldCoord, Vector3Int newCoord)
    {
        if (isOwned)
            ClearWarscreen();

        MessageBroker.Default
            .Publish<OnStandOnTile>(new OnStandOnTile(){ hex = cubeMap[newCoord] });
    }

    [Command(requiresAuthority = false)]
    private void InvokeOnCharacterMoved()
    {
        onCharacterMoved?.Invoke(this);
    }

    private void ClearWarscreen()
    {
        cubeMap.tiles[coordinates].ToggleVisibility(true);

        cubeMap.NeighbourTilesInRadius(utilityStats.perception, coordinates)
            .ToList()
            .ForEach(tile => tile.ToggleVisibility(true));
    }

    public bool OutOfReach(Vector3Int target)
    {
        return moveRadius < cubeMap.Distance(coordinates, target);
    }

    public HexTile GetHexTile()
    {
        return cubeMap[coordinates];
    }

    public class Factory : PlaceholderFactory<Character>
    {
    }

    [System.Serializable]
    public struct UtilityStats
    {
        [SyncVar]
        public int
            health,
            speed,
            stamina,
            perception,
            thriftiness;

        public override string ToString()
        {
            return
                ("Health: " + health + "\n").PadLeft(20, ' ') +
                ("Speed: " + speed + "\n").PadLeft(20, ' ') +
                ("Stamina: " + stamina + "\n").PadLeft(20, ' ') +
                ("Perception: " + perception + "\n").PadLeft(20, ' ') +
                ("Thriftiness: " + thriftiness + "\n").PadLeft(20, ' ');
        }
    }
}

public struct OnStandOnTile
{
    public HexTile hex;
}

public struct OnPlayerLost
{
    public Player player;
}
