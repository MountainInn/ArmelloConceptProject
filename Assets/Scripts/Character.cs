using System;
using Mirror;
using UnityEngine;
using DG.Tweening;
using Zenject;
using MountainInn;

public class Character : NetworkBehaviour
{
    [SyncVar] public Vector2Int coordinates;
    public ushort moveRadius = 1;
    public ushort moveRadiusSquared = 1;
    public Map map;

    [Inject]
    public void Construct(Map map)
    {
        this.map = map;
        coordinates = map.GetRandomCoordinates();

        transform.position = map.tilemap.GetCellCenterWorld(coordinates.xy_());
    }


    public void Move(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
       
        Vector3 position = map.tilemap.GetCellCenterWorld(coordinates.xy_());

        transform.DOMove(position, .5f);
    }

    public bool OutOfReach(Vector2Int target)
    {        
        Vector3
            position = map.tilemap.GetCellCenterWorld(coordinates.xy_()),
            targetPos = map.tilemap.GetCellCenterWorld(target.xy_());

        float distance = (targetPos - position).magnitude;

        return distance > moveRadius * 1.8f;
    }

    public class Factory : PlaceholderFactory<Character>
    {
    }

}
