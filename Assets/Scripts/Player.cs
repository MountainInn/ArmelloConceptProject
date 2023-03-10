using System.Linq;
using Mirror;
using UnityEngine;
using UniRx;
using TMPro;
using System;

public class Player : NetworkBehaviour
{
    [SyncVar] public Character character;
    public Turn turn;
    public bool turnStarted;

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

    [SyncVar] public ArmelloRoomPlayer roomPlayer;
    private TurnView turnView;
    private TurnSystem turnSystem;
    private CubeMap cubeMap;
    private PlayerCustomizationView playerCustomizationView;
    private CharacterSelectionView characterSelectionView;
    private ResourcesView resourcesView;

    private FlagPool flagPool;
    private Character prefabCharacter;
    private TextMeshPro nicknameText;

    internal void SetRoomPlayer(ArmelloRoomPlayer armelloRoomPlayer)
    {
        roomPlayer = armelloRoomPlayer;
    }

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

    public override void OnStartClient()
    {
        InitNicknameText(roomPlayer);

        InitCharacterSO(roomPlayer);

        inventory = GetComponent<Inventory>();
    }

    public void InitCharacterSO(ArmelloRoomPlayer roomPlayer)
    {
        character = GetComponent<Character>();

        character.SetCharacterSO(roomPlayer.characterSO);
        character.characterColor = roomPlayer.playerColor;
    }

    private void InitNicknameText(ArmelloRoomPlayer roomPlayer)
    {
        string nickname = roomPlayer.nickname;
        name = $"[Player] {nickname}";

        nicknameText = GetComponentInChildren<TextMeshPro>();
        nicknameText.text = nickname;
        nicknameText.color = roomPlayer.playerColor;
    }

    public override void OnStartLocalPlayer()
    {
        MessageBroker.Default
            .Receive<HexTile>()
            .Subscribe(OnHexClicked)
            .AddTo(this);

        MessageBroker.Default.Publish(new msgOnLocalPlayerStarted { player = this });
    }

    public struct msgOnLocalPlayerStarted { public Player player; }


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
