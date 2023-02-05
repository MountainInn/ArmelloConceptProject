using System.Collections.Generic;
using System.Linq;
using Mirror;
using MountainInn;
using UnityEngine;
using Zenject;

public class Player : NetworkBehaviour
{
    public Character character;
    public Turn turn;

    private CubeMap cubeMap;
    private Character.Factory characterFactory;

    [Inject]
    public void Construct(CubeMap cubeMap, Character.Factory characterFactory)
    {
        this.characterFactory = characterFactory;
        this.cubeMap = cubeMap;

        Debug.Log("_Inject_ player");
    }

    private void Awake()
    {
        if (!isServer)
        {
            var installer = GameObject.FindObjectOfType<MainInstaller>();
            installer.GetContainer().Inject(this);
        }

        NetworkClient.RegisterSpawnHandler((uint)3611098826, SpawnCharacter, (o)=> Destroy(o));
    }

    public override void OnStartLocalPlayer()
    {
        cubeMap.onHexClicked += CmdMoveCharacter;
    }

    private void Start()
    {
        CmdCreateCharacter();
    }

    private void OnDisable()
    {
        if (!isLocalPlayer)
            return;

        cubeMap.onHexClicked -= CmdMoveCharacter;
    }

    [Command]
    private void CmdCreateCharacter()
    {
        this.character = characterFactory.Create();
        NetworkServer.Spawn(this.character.gameObject, this.connectionToClient);
    }

    GameObject SpawnCharacter(Vector3 position, uint assetId)
    {
        character = characterFactory.Create();
        var netId = character.GetComponent<NetworkIdentity>().netId;
        character.name = $"Character [netId={netId}]";
        if (isLocalPlayer)
            character.name = $"My Character [netId={netId}]";
        return character.gameObject;
    }


    [Command]
    private void CmdMoveCharacter(Vector3Int coordinates)
    {
        if (character is null ||
            coordinates == character.coordinates ||
            character.OutOfReach(coordinates)
        )
            return;

        character.coordinates = coordinates;

        turn.CompleteExplorationPhase();
        turn.CompleteMovementPhase();
        turn.CompleteCombatPhase();

        character.RpcMove(coordinates);
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

            player.transform.position = startPosition;
            player.transform.rotation = startRotation;

            return player;
        }
    }

}
