using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using MountainInn;
using System;

public class ArmelloDistanceInterestManagement : InterestManagement
{
    [Tooltip("The maximum range that objects will be visible at. Add DistanceInterestManagementCustomRange onto NetworkIdentities for custom ranges.")]
    public int visRange = 10;

    [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
    public float rebuildInterval = 1;
    double lastRebuildTime;

    CubeMap cubeMap;

    [ServerCallback]
    public override void Reset()
    {
        lastRebuildTime = 0D;
    }

    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
    {
        if (!newObserver.identity.TryGetComponent(out Player player))
            return true;

        var customRange = player.GetComponent<ArmelloDistanceInterestManagementCustomRange>();
        float range;
        bool
            isInRange = true,
            isTile = false;

        if (
            identity.TryGetComponent(out CubicTransform cubicTransform) &&
            !identity.GetComponent<Player>() &&
            cubeMap
        )
        {
            range = customRange.cubicRange;
            isInRange = cubeMap.Distance(player.cubicTransform().coordinates, cubicTransform.coordinates) <= range;

            if (isTile = identity.TryGetComponent(out HexTile hexTile))
            {
                hexTile.TargetToggleVisibility(newObserver, isInRange);
            }
        }
        else
        {
            range = customRange.floatRange;
            isInRange = Vector3.Distance(player.transform.position, identity.transform.position) < range;
        }

        return
            isTile || isInRange;
    }

    public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
    {
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.isAuthenticated && conn.identity != null)
            {
                if (OnCheckObserver(identity, conn))
                {
                    newObservers.Add(conn);
                }
            }
        }
    }




    // internal so we can update from tests
    [ServerCallback]
    internal void Update()
    {
        if ((cubeMap ??= FindObjectOfType<CubeMap>()) is null)
        {
            return;
        }

        if (NetworkTime.localTime >= lastRebuildTime + rebuildInterval)
        {
            RebuildAll();
            lastRebuildTime = NetworkTime.localTime;
        }
    }

}
