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
            .Subscribe(OnHexClicked);

        turnView.onEndTurnClicked += CmdEndTurn;
    }

    public override void OnStopLocalPlayer()
    {
        turnView.onEndTurnClicked -= CmdEndTurn;
    }

    [Server]
    public void ResetActionPoints()
    {
        actionPoints = 5;
    }

    private void OnHexClicked(HexTile hex)
    {
        if (turn.started.Value == false) return;
        if (actionPoints < hex.moveCost) return;
        if (actionPoints == 0) return;

        CmdMoveCharacter(hex);
    }

    [Command]
    private void CmdEndTurn()
    {
        if (turn == null) return;

        turn.forceTurnCompletion.Value = true;
    }

    [TargetRpc]
    public void TargetToggleTurnView(bool turnStarted)
    {
        turnView.Toggle(turnStarted);
    }

    [Command]
    private void CmdMoveCharacter(HexTile hex)
    {
        character.CmdMove(hex.coordinates);
        CmdSpendActionPoints(hex.moveCost);
    }

    [Command]
    private void CmdSpendActionPoints(int amount)
    {
        actionPoints -= amount;
        Debug.Assert(actionPoints >= 0);
    }

    [Command]
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
