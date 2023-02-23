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
    new public SpriteRenderer renderer;
    public event Action<Character> onCharacterMoved;

    [SyncVar]
    public UtilityStats utilityStats;

    public Player player;

    public void SetCharacterSO(CharacterScriptableObject characterSO)
    {
        this.gameObject.name = player.name +" "+ characterSO.characterName;
        this.renderer.sprite = characterSO.characterSprite;
        this.combatUnit.characterStats = characterSO.combatStats;
        this.utilityStats = characterSO.utilityStats;
    }

    private void Awake()
    {
        cubeMap = FindObjectOfType<CubeMap>();
        var characterSelectionView = FindObjectOfType<CharacterSelectionView>();
        SetCharacterSO(characterSelectionView.GetSelectedCharacter());
    }

    public override void OnStartClient()
    {
        if (isOwned)
        {
            if (cubeMap.isFullySpawned)
                CmdInitializeCoordinates();
            else
                cubeMap.onFullySpawned += CmdInitializeCoordinates;
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdInitializeCoordinates()
    {
        var coord = cubeMap.GetRandomCoordinates();

        var position = cubeMap.PositionFromCoordinates(coord);

        var hex = cubeMap[coord];

        RpcMove(hex, useTween: false);
    }   

    [Command(requiresAuthority = false)]
    public void CmdMove(HexTile hex)
    {
        Vector3Int coordinates = hex.coordinates;
        Vector3 position = cubeMap.PositionFromCoordinates(coordinates);

        RpcMove(hex, true);
    }

    [ClientRpc]
    public void RpcMove(HexTile hex, bool useTween)
    {
        Vector3 position = hex.Top;
        Vector3Int coordinates = hex.coordinates;

        if (useTween)
        {
            transform
                .DOMove(position, .5f)
                .OnKill(EndTransition);
        }
        else
        {
            transform.position = position;
            EndTransition();
        }

        void EndTransition()
        {
            CmdSetCoordinates(coordinates);
            hex.character = this;

            if (hex.usableTile is TriggerTile bonusTile)
            {
                bonusTile.UseTile(player);
            }
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
        if (isOwned)
            ClearWarscreen();

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

        cubeMap.NeighbourTilesInRadius(utilityStats.perception, coordinates)
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

    public struct UtilityStats
    {
        [SyncVar]
        public int
            health,
            speed,
            stamina,
            perception,
            thriftiness;

        public override string ToString()
        {
            return
                ("Health: " + health + "\n").PadLeft(18) +
                ("Speed: " + speed + "\n").PadLeft(18) +
                ("Stamina: " + stamina + "\n").PadLeft(18) +
                ("Perception: " + perception + "\n").PadLeft(18) +
                ("Thriftiness: " + thriftiness + "\n").PadLeft(18);
        }
    }
}
