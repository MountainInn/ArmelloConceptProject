using System.Collections.Generic;
using System.Linq;
using Mirror;
using MountainInn;
using UnityEngine;
using Zenject;
using UniRx;

public class Player : NetworkBehaviour
{
    public Character character;
    public Turn turn;
    // public TurnView turnView;

    private CubeMap cubeMap;
    private Character.Factory characterFactory;

    private PlayerCustomizationView playerCustomizationView;
    public Color clientCharacterColor => playerCustomizationView.playerColor;

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

        NetworkClient.RegisterSpawnHandler((uint)3611098826, SpawnCharacter, (o)=> Destroy(o));
        playerCustomizationView = FindObjectOfType<PlayerCustomizationView>();
    }

    public override void OnStartServer()
    {
        // turn.started
        //     .Subscribe(b =>
        //     {

        //         });
    }

    public override void OnStartLocalPlayer()
    {
        CmdCreateCharacter();
        CmdCreateCharacter();

        MessageBroker.Default
            .Receive<HexTile>()
            .Subscribe(hex =>
            {
                CmdMoveCharacter(hex.coordinates);
            });
    }

    [Command]
    private void CmdMoveCharacter(Vector3Int coord)
    {
        if (turn.started.Value)
            character.CmdMove(coord);
    }

    [Command]
    private void CmdCreateCharacter(Color characterColor)
    {
        // if (this.character != null)
        //     NetworkServer.Destroy(this.character.gameObject);

        this.character = characterFactory.Create();
        InitializeCharacter(this.character);
        this.character.characterColor = characterColor;

        NetworkServer.Spawn(this.character.gameObject, this.connectionToClient);
    }

    [Client]
    GameObject SpawnCharacter(Vector3 position, uint assetId)
    {
        this.character = characterFactory.Create();
        InitializeCharacter(this.character);

        return character.gameObject;
    }

    private void InitializeCharacter(Character character)
    {
        var netId = character.GetComponent<NetworkIdentity>().netId;
        character.name = $"Character [netId={netId}]";
        if (isLocalPlayer)
        {
            character.name = $"My Character [netId={netId}]";

            // character.onCharacterMoved +=
            //     (chara) =>
            //     {
            //         turn.CompleteExplorationPhase();
            //         turn.CompleteMovementPhase();
            //         turn.CompleteCombatPhase();
            //     };


        }
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
