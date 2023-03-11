using UnityEngine;
using Mirror;

public class CubicTransform : NetworkBehaviour
{
    [SyncVar] public Vector3Int coordinates;
}
