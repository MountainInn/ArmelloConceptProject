using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using MountainInn;


public class Player : NetworkBehaviour
{
    private Tilemap tilemap;
    private Character character;

    [Inject]
    public void Construct(Map map, Tilemap tilemap, Character.Factory characterFactory)
    {
        this.tilemap = tilemap;
        this.character = characterFactory.Create();
        this.character.transform.SetParent(transform, true);

        foreach (var item in map.hexTiles)
        {
            item.onHexClicked += MoveCharacter;
        }
    }


    private void MoveCharacter(Vector2Int coordinates)
    {
        if (coordinates == character.coordinates ||
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
            var player = base.Create();

            player.transform.position = startPosition;
            player.transform.rotation = startRotation;

            return player;
        }
    }

}
