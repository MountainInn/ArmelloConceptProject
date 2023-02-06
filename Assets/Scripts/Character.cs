using Mirror;
using UnityEngine;
using DG.Tweening;
using Zenject;
using System.Collections.Generic;
using MountainInn;
using System.Linq;

public class Character : NetworkBehaviour
{
    [SyncVar] public Vector3Int coordinates;
    [Range(1, 10)]
    public int moveRadius = 1;
    public CubeMap cubeMap;

    public Combat.Stats combatStats;

    [Inject]
    public void Construct(CubeMap cubeMap)
    {
        this.cubeMap = cubeMap;
    }

    private void OnEnable()
    {
        cubeMap.onGenerated += CmdPlaceCharacter;
    }

    private void OnDisable()
    {
        cubeMap.onGenerated -= CmdPlaceCharacter;
    }

    private void Start()
    {
        if ( cubeMap.IsReady )
        {
            Debug.Log("=-=-=-Map is ready");
            CmdPlaceCharacter();
        }
        else
        {
            Debug.Log("=-=-=-Map is NOT ready");
        }
    }

    [Command]
    private void CmdPlaceCharacter()
    {
        Debug.Log($"{gameObject.name} CmdPlaceCharacter");
        coordinates = cubeMap.GetRandomCoordinates();

        RpcPlaceCharacter();
    }

    [ClientRpc]
    private void RpcPlaceCharacter()
    {
        transform.position = cubeMap[coordinates].transform.position;

        if (isOwned)
            ClearWarscreen();
    }

    [ClientRpc]
    public void RpcMove(Vector3Int coordinates)
    {
        Vector3 position = cubeMap[coordinates].transform.position;

        transform.DOMove(position, .5f);

        if (isOwned)
            ClearWarscreen();
    }

    private void ClearWarscreen()
    {
        cubeMap.tiles[coordinates].ToggleVisibility(true);

        cubeMap.NeighbourTilesInRadius(moveRadius, coordinates)
            .ToList()
            .ForEach(tile => tile.ToggleVisibility(true));
    }

    public bool OutOfReach(Vector3Int target)
    {        
        return moveRadius < cubeMap.Distance(coordinates, target);
    }

    public class Factory : PlaceholderFactory<Character>
    {
    }

}
