using Mirror;
using UnityEngine;
using DG.Tweening;
using Zenject;
using System.Linq;
using System;
using TMPro;

public class Character : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCoordinatesSync))]
    public Vector3Int coordinates;

    [SyncVar(hook = nameof(OnColorSync))]
    public Color characterColor;

    [Range(1, 10)]
    public int moveRadius = 1;
    public CubeMap cubeMap;
    public CombatUnit combatUnit => GetComponent<CombatUnit>();
    public TextMeshPro textMeshPro;
    public event Action<Character> onCharacterMoved;

    private void Awake()
    {
        cubeMap = FindObjectOfType<CubeMap>();
    }

    private void Start()
    {
        if (isOwned)
            cubeMap.onFullySpawned += CmdInitializeCoordinates;
    }

    [Command(requiresAuthority = false)]
    private void CmdInitializeCoordinates()
    {
        var coord = cubeMap.GetRandomCoordinates();

        var position = cubeMap.PositionFromCoordinates(coord);

        RpcMove(coord, useTween: false);
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

        RpcMove(coordinates, true);
    }

    [ClientRpc]
    public void RpcMove(Vector3Int coordinates, bool useTween)
    {
        Transform top = cubeMap[coordinates].Top;
        Vector3 position = top.position;

        if (useTween)
        {
            transform
                .DOMove(position, .5f)
                .OnComplete(() => transform.SetParent(top, true))
                .OnKill(() => CmdSetCoordinates(coordinates));
        }
        else
        {
            transform.position = position;
            transform.SetParent(top, true);
            CmdSetCoordinates(coordinates);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSetCoordinates(Vector3Int coordinates)
    {
        this.coordinates = coordinates;
    }

    [Client]
    private void OnCoordinatesSync(Vector3Int oldCoord, Vector3Int newCoord)
    {
        /// Даёт null-reference
        /// Временно закоментировал
        //
        // if (isOwned)
        //     ClearWarscreen();

        // Проблемы с авторитетом клиента при командовании
        // InvokeOnCharacterMoved();
    }

    private void OnColorSync(Color oldc, Color newc)
    {
        textMeshPro.color = newc;
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
