using Mirror;
using UnityEngine;
using DG.Tweening;
using Zenject;
using System.Linq;
using System;

public class Character : NetworkBehaviour
{
    [SyncVar(hook = nameof(CoordinatesSyncedHook))]
    public Vector3Int coordinates;

    [Range(1, 10)]
    public int moveRadius = 1;
    public CubeMap cubeMap;
    public Combat.CombatUnit combatUnit;
    public event Action<Character> onCharacterMoved;

    [Inject]
    public void Construct(CubeMap cubeMap)
    {
        this.cubeMap = cubeMap;
    }

    private void Start()
    {
        if (isServer)
        {
            CmdInitializeCoordinates();
        }
    }

    [Command]
    private void CmdInitializeCoordinates()
    {
        var coord = cubeMap.GetRandomCoordinates();

        RpcMove(coord, useTween: false);
    }

    [Command]
    public void CmdMove(Vector3Int coordinates)
    {
        if (this.coordinates == coordinates ||
            OutOfReach(coordinates)
        )
        {
            Debug.Log($"Character: {this.coordinates} Target: {coordinates}");
            return;
        }

        RpcMove(coordinates, true);
    }

    [ClientRpc]
    public void RpcMove(Vector3Int coordinates, bool useTween)
    {
        Vector3 position = cubeMap[coordinates].transform.position;

        if (useTween)
        {
            transform
                .DOMove(position, .5f)
                .OnKill(() => CmdSetCoordinates(coordinates));
        }
        else
        {
            transform.position = position;
            CmdSetCoordinates(coordinates);
        }
    }

    [Command]
    private void CmdSetCoordinates(Vector3Int coordinates)
    {
        this.coordinates = coordinates;
    }

    [Client]
    private void CoordinatesSyncedHook(Vector3Int oldCoord, Vector3Int newCoord)
    {
        if (isOwned)
            ClearWarscreen();

        InvokeOnCharacterMoved();
    }

    [Command]
    private void InvokeOnCharacterMoved()
    {
        onCharacterMoved?.Invoke(this);
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
