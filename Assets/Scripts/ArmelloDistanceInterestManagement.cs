using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using MountainInn;

public class ArmelloDistanceInterestManagement : InterestManagement
{
    [Tooltip("The maximum range that objects will be visible at. Add DistanceInterestManagementCustomRange onto NetworkIdentities for custom ranges.")]
    public int visRange = 10;

    [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
    public float rebuildInterval = 1;
    double lastRebuildTime;

    CubeMap cubeMap;

    protected override void Awake()
    {
        base.Awake();

        MessageBroker.Default
            .Receive<CubeMap.msgFullySpawned>()
            .Subscribe(msg => cubeMap = FindObjectOfType<CubeMap>())
            .AddTo(this);
    }

    // helper function to get vis range for a given object, or default.
    int GetVisRange(NetworkIdentity identity)
    {
        return identity.TryGetComponent(out DistanceInterestManagementCustomRange custom) ? custom.visRange : visRange;
    }

    [ServerCallback]
    public override void Reset()
    {
        lastRebuildTime = 0D;
    }

    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
    {
        identity.TryGetComponent(out HexTile hexTile);

        int range = GetVisRange(newObserver.identity);

        return
            hexTile ||
            Vector3.Distance(identity.transform.position, newObserver.identity.transform.position) < range;
    }

    public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
    {
        Vector3 position = identity.transform.position;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.isAuthenticated && conn.identity != null)
            {
                int range = GetVisRange(conn.identity);

                bool isInRange = Vector3.Distance(conn.identity.transform.position, position) < range;

                if (identity.TryGetComponent(out HexTile hexTile))
                {
                    hexTile.TargetToggleVisibility(conn, isInRange);
                }

                if (hexTile || isInRange)
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
        // if (cubeMap is null) return;

        if (NetworkTime.localTime >= lastRebuildTime + rebuildInterval)
        {
            RebuildAll();
            lastRebuildTime = NetworkTime.localTime;
        }
    }

}
