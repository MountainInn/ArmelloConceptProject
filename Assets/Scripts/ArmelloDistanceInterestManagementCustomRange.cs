// add this to NetworkIdentities for custom range if needed.
// only works with DistanceInterestManagement.
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class ArmelloDistanceInterestManagementCustomRange : NetworkBehaviour
{
    public float floatRange = 5;
    public int cubicRange = 2;
}
