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
        resourcesView.UpdateActionPoints(ne);
    }

    [SyncVar(hook = nameof(OnMovementPointsSync))]
    private int movementPoints;

    private void OnMovementPointsSync(int ol, int ne)
    {
        resourcesView.UpdateMovementPoints(ne);
    }


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

        MessageBroker.Default
            .Receive<HexTile>()
            .Subscribe(OnHexClicked)
            .AddTo(this);

        turnView.onEndTurnClicked += CmdEndTurn;
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
        else if (hex.character == this)
        {
            if (actionPoints < 1) return;

            CmdMineHex(hex);
        }
        else
        {
            if (movementPoints < 1) return;

            CmdAttackOtherCharacter(hex);
        }
    }

    private void CmdMineHex(HexTile hex)
    {
        Debug.Log("Mine Hex");
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

        hex.character = null;
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
        if (this.character != null)
            NetworkServer.Destroy(this.character.gameObject);

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
