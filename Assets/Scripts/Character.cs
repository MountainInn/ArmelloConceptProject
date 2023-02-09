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
    public CombatUnit combatUnit => GetComponent<CombatUnit>();
    public event Action<Character> onCharacterMoved;

    [Inject]
    public void Construct(CubeMap cubeMap)
    {
        this.cubeMap = cubeMap;
    }

    private void Start()
    {
        if (isOwned)
        {
            CmdInitializeCoordinates();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdInitializeCoordinates()
    {
        var coord = cubeMap.GetRandomCoordinates();

        var position = cubeMap.PositionFromCoordinates(coord);

        RpcMove(coord, position, useTween: false);
    }

    [Command(requiresAuthority = false)]
    public void CmdMove(Vector3Int coordinates)
    {
        if (this.coordinates == coordinates ||
            OutOfReach(coordinates)
        )
        {
            Debug.Log($"Character: {this.coordinates} Target: {coordinates}");
            return;
        }

        Vector3 position = cubeMap.PositionFromCoordinates(coordinates);

        RpcMove(coordinates, position, true);
    }

    [ClientRpc]
    public void RpcMove(Vector3Int coordinates, Vector3 position, bool useTween)
    {
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

    [Command(requiresAuthority = false)]
    private void CmdSetCoordinates(Vector3Int coordinates)
    {
        this.coordinates = coordinates;
    }

    [Client]
    private void CoordinatesSyncedHook(Vector3Int oldCoord, Vector3Int newCoord)
    {
        ///Даёт null-reference
        // Временно закоментировал
        //
        // if (isOwned)
        //     ClearWarscreen();

        // Проблемы с авторитетом клиента при командовании
        // InvokeOnCharacterMoved();
    }

    [Command(requiresAuthority = false)]
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
