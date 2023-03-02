using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UniRx;

public class Player : NetworkBehaviour
{
    [SyncVar] public Character character;
    public Turn turn;
    public bool turnStarted;
    public CharacterSettings characterSettings;

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

    [SyncVar]
    public Inventory inventory;

    private ArmelloRoomPlayer roomPlayer;
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
        roomPlayer = FindObjectOfType<ArmelloRoomPlayer>();
        turnView = FindObjectOfType<TurnView>();
        turnSystem = FindObjectOfType<TurnSystem>();
        cubeMap = FindObjectOfType<CubeMap>();
        playerCustomizationView = FindObjectOfType<PlayerCustomizationView>();
        characterSelectionView = FindObjectOfType<CharacterSelectionView>();
        resourcesView = FindObjectOfType<ResourcesView>();
        flagPool = FindObjectOfType<FlagPool>();
        prefabCharacter = Resources.Load<Character>("Prefabs/Character");

        name = $"[Player] {PlayerPrefs.GetString("Nickname")}";
        character = GetComponent<Character>();
        inventory = GetComponent<Inventory>();
        characterSettings = Resources.Load<CharacterSettings>("CharacterSettings");
    }

    public override void OnStartServer()
    {
        character.SetCharacterSO(characterSettings.characterSO);
        character.characterColor = characterSettings.characterColor;

        MessageBroker.Default.Receive<OnPlayerLost>()
            .Subscribe(OnPlayerLost)
            .AddTo(this);
    }

    [Server]
    public void SetInventory(Inventory newInventory)
    {
        this.inventory = newInventory;
    }

    [Server]
    public void SetCharacter(Character newCharacter)
    {
        this.character = newCharacter;
    }

    public override void OnStartLocalPlayer()
    {
        MessageBroker.Default
            .Receive<HexTile>()
            .Subscribe(OnHexClicked)
            .AddTo(this);


        MessageBroker.Default.Publish(new msgOnLocalPlayerStarted{ player = this });
    }

    public struct msgOnLocalPlayerStarted { public Player player; }

    [Server]
    private void OnPlayerLost(OnPlayerLost msg)
    {
        TargetToggleTurnStarted(false);
        turnSystem.UnregisterPlayer(this);
    }

    public override void OnStopServer()
    {
        turnSystem.UnregisterPlayer(this);
    }

    [TargetRpc]
    public void TargetInitTurnView()
    {
        turnView.onEndTurnClicked += CmdEndTurn;
    }

    [TargetRpc]
    public void TargetCleanupTurnView()
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
        hex.influence.WorkOn(this);
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
}
