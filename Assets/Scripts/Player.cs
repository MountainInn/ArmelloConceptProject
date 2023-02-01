using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using MountainInn;
using System;

public class Player : NetworkBehaviour
{
    public Character character;
    private Map map;
    private Character.Factory characterFactory;

    [Inject]
    public void Construct(Map map, Character.Factory characterFactory)
    {
        this.characterFactory = characterFactory;
        this.map = map;

        Debug.Log("_Inject_ player");
    }

    [Command]
    private void CmdCreateCharacter()
    {
        this.character = characterFactory.Create();
        NetworkServer.Spawn(this.character.gameObject, this.connectionToClient);
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
        if (map.hexTiles.Count == 0)
        {
            map.onTileCreated += SubToTileClick;
        }
        else
        {
            map.hexTiles.ForEach(SubToTileClick);
        }
    }

    private void SubToTileClick(HexTile tile)
    {
        tile.onHexClicked += MoveCharacter;
    }

    private void OnDisable()
    {
        if (!isLocalPlayer)
            return;

        map.onTileCreated -= SubToTileClick;
        map.hexTiles.ForEach(t => t.onHexClicked -= MoveCharacter);
    }

    private void Start()
    {
        CmdCreateCharacter();
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


    private void MoveCharacter(Vector2Int coordinates)
    {
        if (character is null ||
            coordinates == character.coordinates ||
            character.OutOfReach(coordinates)
        )
            return;

        character.Move(coordinates);
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
