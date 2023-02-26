using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UniRx;
using MountainInn;
using System;

public class Player : NetworkBehaviour
{
    [SyncVar(hook=nameof(OnCharacterSync))] public Character character;

    private void OnCharacterSync(Character oldv, Character newv)
    {
        Debug.Log($"CharacterSync {newv}");
    }

    public Turn turn;
    public bool turnStarted;
    public Color clientCharacterColor => playerCustomizationView.playerColor;

    [SyncVar(hook = nameof(OnActionPointsSync))]
    public int actionPoints;
    private void OnActionPointsSync(int ol, int ne)
    {
        if (isOwned)
            resourcesView.UpdateActionPoints(ne);
    }

    [SyncVar(hook = nameof(OnMovementPointsSync))]
    private int movementPoints;
    private void OnMovementPointsSync(int ol, int ne)
    {
        if (isOwned)
            resourcesView.UpdateMovementPoints(ne);
    }

    private List<InfluenceTile> influenceTiles;

    readonly SyncDictionary<ResourceType, int> resources = new SyncDictionary<ResourceType, int>();

    private TurnView turnView;
    private TurnSystem turnSystem;
    private CubeMap cubeMap;
    private PlayerCustomizationView playerCustomizationView;
    private CharacterSelectionView characterSelectionView;
    private ResourcesView resourcesView;
    private FlagPool flagPool;
    private Character prefabCharacter;

    public void Awake()
    {
        turnView = FindObjectOfType<TurnView>();
        turnSystem = FindObjectOfType<TurnSystem>();
        cubeMap = FindObjectOfType<CubeMap>();
        playerCustomizationView = FindObjectOfType<PlayerCustomizationView>();
        characterSelectionView = FindObjectOfType<CharacterSelectionView>();
        resourcesView = FindObjectOfType<ResourcesView>();
        flagPool = FindObjectOfType<FlagPool>();
        prefabCharacter = Resources.Load<Character>("Prefabs/Character");
    }

    public override void OnStartLocalPlayer()
    {
        CmdCreateCharacter(clientCharacterColor);
        // CmdCreateCharacter(clientCharacterColor);

        MessageBroker.Default
            .Receive<HexTile>()
            .Subscribe(OnHexClicked)
            .AddTo(this);

        turnView.onEndTurnClicked += CmdEndTurn;

        resources.Callback += OnResourcesSync;
    }

    private void OnResourcesSync(SyncIDictionary<ResourceType, int>.Operation op, ResourceType key, int item)
    {
        switch (op)
        {
            case SyncIDictionary<ResourceType, int>.Operation.OP_ADD:
                break;
            case SyncIDictionary<ResourceType, int>.Operation.OP_SET:
                break;
            case SyncIDictionary<ResourceType, int>.Operation.OP_REMOVE:
                break;
            case SyncIDictionary<ResourceType, int>.Operation.OP_CLEAR:
                break;
        }

        if (isOwned)
        {
            resources
                .Log("Resources: ");

            resourcesView.SetResource(key, item);
        }
    }

    public override void OnStartServer()
    {
        System.Enum.GetValues(typeof(ResourceType))
            .Cast<ResourceType>()
            .ToList()
            .ForEach(r => resources.Add(r, 0));

        MessageBroker.Default
            .Receive<MiningTile.TileMinedMsg>()
            .Where(msg => msg.player == this)
            .Subscribe(CmdAddResource)
            .AddTo(this);


        influenceTiles = new List<InfluenceTile>();

        MessageBroker.Default
            .Receive<InfluenceTile.TileTakenMsg>()
            .Subscribe(OnInfluenceTileTaken)
            .AddTo(this);

        MessageBroker.Default
            .Receive<TurnSystem.OnRoundEnd>()
            .Subscribe(msg =>
            {
                influenceTiles
                    .ForEach(t => t.InfluenceEffect(this));
            })
            .AddTo(this);

        MessageBroker.Default.Receive<OnPlayerLost>()
            .Subscribe(OnPlayerLost)
            .AddTo(this);
    }

    [Server]
    private void OnPlayerLost(OnPlayerLost msg)
    {
        TargetToggleTurnStarted(false);
        turnSystem.UnregisterPlayer(this);
        NetworkServer.Destroy(character.gameObject);
    }

    [Server]
    private void OnInfluenceTileTaken(InfluenceTile.TileTakenMsg msg)
    {
        if (msg.newOwner == this)
        {
            Debug.Log("You took Influence Tile");
            influenceTiles.Add(msg.tile);
            RpcPutFlagOnInfluenceTile(this, msg.hexTile);
        }
        else if (msg.previousOwner == this)
        {
            Debug.Log("You lost Influence Tile");
            influenceTiles.Remove(msg.tile);
            RpcRemoveFlagFromInfluenceTile(this, msg.hexTile);
        }
    }

    [ClientRpc]
    private void RpcPutFlagOnInfluenceTile(Player player, HexTile tile)
    {
        var flag = flagPool.Rent(player, tile);
    }
    [ClientRpc]
    private void RpcRemoveFlagFromInfluenceTile(Player player, HexTile tile)
    {
        flagPool.Return(player, tile);
    }

    public void CmdAddResource(MiningTile.TileMinedMsg msg)
    {
        resources[msg.resourceType] += msg.amount;
    }

    public override void OnStopLocalPlayer()
    {
        turnView.onEndTurnClicked -= CmdEndTurn;
    }


    [Command(requiresAuthority = false)]
    public void CmdResetPoints()
    {
        actionPoints = character.utilityStats.stamina;
        movementPoints = character.utilityStats.speed;
    }

    [Client]
    private void OnHexClicked(HexTile hex)
    {
        if (!turnStarted) return;

        if (hex.character is null)
        {
            if (movementPoints < hex.moveCost)
                return;

            if (cubeMap.Distance(character.coordinates, hex.coordinates) != 1)
                return;

            character.CmdMove(hex);
            CmdSpendMovementPoints(hex.moveCost);
        }
        else if (hex.character.isOwned)
        {
            if (actionPoints < 1) return;
            if (!hex.CanUseTile(this)) return;

            CmdUseTile(hex);
            CmdSpendActionPoints(1);
        }
        else
        {
            if (movementPoints < 1)
                return;

            if (cubeMap.Distance(character.coordinates, hex.coordinates) != 1)
                return;

            CmdAttackOtherCharacter(hex);
            CmdSpendMovementPoints(1);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdUseTile(HexTile hex)
    {
        hex.UseTile(this);
    }

    [Command(requiresAuthority = false)]
    private void CmdAttackOtherCharacter(HexTile hex)
    {
        CombatUnit[] units =
            new[] { character, hex.character }
            .Select(ch => ch.combatUnit)
            .ToArray();

        MessageBroker.Default
            .Publish<CombatUnit[]>(units);
    }

    [Command(requiresAuthority = false)]
    private void CmdEndTurn()
    {
        if (turn == null) return;

        turn.forceTurnCompletion.Value = true;
    }

    [TargetRpc]
    public void TargetToggleTurnStarted(bool turnStarted)
    {
        this.turnStarted = turnStarted;

        if (turnStarted)
            CmdResetPoints();

        turnView.Toggle(turnStarted);
    }

    [Command(requiresAuthority = false)]
    private void CmdSpendMovementPoints(int amount)
    {
        movementPoints -= amount;
        Debug.Assert(movementPoints >= 0);
    }
    [Command(requiresAuthority = false)]
    private void CmdSpendActionPoints(int amount)
    {
        actionPoints -= amount;
        Debug.Assert(actionPoints >= 0);
    }

    [Command(requiresAuthority = false)]
    private void CmdCreateCharacter(Color characterColor)
    {
        if (this.character != null)
            NetworkServer.Destroy(this.character.gameObject);

        this.character = characterFactory.Create();
        this.character.characterColor = characterColor;
        this.character.player = this;

        NetworkServer.Spawn(this.character.gameObject, this.connectionToClient);
    }

    public class Factory : PlaceholderFactory<Player>
    {
        new public Player Create()
        {
            var player = base.Create();

            return player;
        }
        public Player Create(Vector3 startPosition, Quaternion startRotation)
        {
            var player = Create();

            player.transform.rotation = startRotation;

            return player;
        }
    }

}
