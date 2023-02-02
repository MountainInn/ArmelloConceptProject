using Mirror;
using UnityEngine;
using DG.Tweening;
using Zenject;
using MountainInn;

public class Character : NetworkBehaviour
{
    [SyncVar] public Vector3Int coordinates;
    [Range(1, 10)]
    public int moveRadius = 1;
    public Map map;

    [Inject]
    public void Construct(Map map)
    {
        this.map = map;
        coordinates = map.GetRandomCoordinates();

        transform.position = map.tilemap.GetCellCenterWorld(coordinates);
    }


    public void Move(Vector3Int coordinates)
    {
        this.coordinates = coordinates;
       
        Vector3 position = map.tilemap.GetCellCenterWorld(coordinates);

        transform.DOMove(position, .5f);
    }

    public bool OutOfReach(Vector3Int target)
    {        
        Vector3
            position = map.tilemap.GetCellCenterWorld(coordinates),
            targetPos = map.tilemap.GetCellCenterWorld(target);

        float distance = (targetPos - position).magnitude;

        return distance > moveRadius * 1.8f;
    }

    public class Factory : PlaceholderFactory<Character>
    {
    }

}
