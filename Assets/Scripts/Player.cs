using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Zenject;
using UniRx;
using MountainInn;
using System;

public class Player : NetworkBehaviour
{
    public Character character;
    public Turn turn;
    public bool turnStarted;
    public TurnView turnView;
    public TurnSystem turnSystem;

    private CubeMap cubeMap;
    private Character.Factory characterFactory;

    private PlayerCustomizationView playerCustomizationView;
    public Color clientCharacterColor => playerCustomizationView.playerColor;

    private ResourcesView resourcesView;

    [SyncVar(hook = nameof(OnActionPointsSync))]
    private int actionPoints;
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

    readonly SyncDictionary<ResourceType, int> resources = new SyncDictionary<ResourceType, int>();

    [Inject]
    public void Construct(CubeMap cubeMap, Character.Factory characterFactory)
    {
        this.characterFactory = characterFactory;
        this.cubeMap = cubeMap;
    }

    private void Awake()
    {
        if (!isServer)
        {
            var installer = GameObject.FindObjectOfType<MainInstaller>();
            installer.GetContainer().Inject(this);
        }

        turnView = FindObjectOfType<TurnView>();
        playerCustomizationView = FindObjectOfType<PlayerCustomizationView>();
        resourcesView = FindObjectOfType<ResourcesView>();
    }

    public override void OnStartLocalPlayer()
    {
        CmdCreateCharacter(clientCharacterColor);
        CmdCreateCharacter(clientCharacterColor);

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

        resourcesView.SetResource(key, item);
    }

    public override void OnStartServer()
    {
        System.Enum.GetValues(typeof(ResourceType))
            .Cast<ResourceType>()
            .ToList()
            .ForEach(r => resources.Add(r, 0));

        MessageBroker.Default
            .Receive<MiningTile.TileMinedMsg>()
            .Subscribe(msg =>
            {
                if (!resources.ContainsKey(msg.resourceType))
                    resources.Add(msg.resourceType, msg.amount);
                else
                    resources[msg.resourceType] += msg.amount;
            })
            .AddTo(this);
    }

    public override void OnStopLocalPlayer()
    {
        turnView.onEndTurnClicked -= CmdEndTurn;
    }


    [Command(requiresAuthority=false)]
    public void CmdResetPoints()
    {
        Debug.Log("Reset Points");
        actionPoints = 5;
        movementPoints = 5;
    }

    private void OnHexClicked(HexTile hex)
    {
        if (!turnStarted) return;

        if (hex.character is null)
        {
            if (movementPoints < hex.moveCost) return;

            CmdMoveCharacter(hex);
        }
        else if (hex.character.isOwned)
        {
            if (actionPoints < 1) return;

            CmdUseTile(hex);
        }
        else
        {
            if (movementPoints < 1) return;

            CmdAttackOtherCharacter(hex);
        }
    }

    [Command(requiresAuthority=false)]
    private void CmdUseTile(HexTile hex)
    {
        hex.UseTile(this);
    }

    [Command(requiresAuthority=false)]
    private void CmdAttackOtherCharacter(HexTile hex)
    {
        CmdSpendMovementPoints(1);

        CombatUnit[] units =
            new [] { character, hex.character }
            .Select(ch => ch.combatUnit)
            .ToArray();

        MessageBroker.Default
            .Publish<CombatUnit[]>(units);
    }

    [Command(requiresAuthority=false)]
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

    [Command(requiresAuthority=false)]
    private void CmdMoveCharacter(HexTile hex)
    {
        if (character.coordinates == hex.coordinates ||
            character.OutOfReach(hex.coordinates)
        )
        {
            return;
        }

        cubeMap[character.coordinates].character = null;
        character.CmdMove(hex);
        CmdSpendMovementPoints(hex.moveCost);
    }

    [Command(requiresAuthority=false)]
    private void CmdSpendMovementPoints(int amount)
    {
        movementPoints -= amount;
        Debug.Assert(movementPoints >= 0);
    }
    [Command(requiresAuthority=false)]
    private void CmdSpendActiongPoints(int amount)
    {
        actionPoints -= amount;
        Debug.Assert(actionPoints >= 0);
    }

    [Command(requiresAuthority=false)]
    private void CmdCreateCharacter(Color characterColor)
    {
        // if (this.character != null)
        //     NetworkServer.Destroy(this.character.gameObject);

        this.character = characterFactory.Create();
        this.character.characterColor = characterColor;

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
