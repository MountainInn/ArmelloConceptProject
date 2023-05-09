using System;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using MountainInn;
using UnityEngine;

public class TileLevel : NetworkBehaviour
{
    [SyncVar] public int syncLevel = 0;

    public int effectiveLevel = 0;
    public float height => effectiveLevel + 1;

    public event Action onLevelSync;

    [Client]
    private void OnLevelSync(int oldv, int newv)
    {
        onLevelSync?.Invoke();
    }


    [Client]
    public bool ShouldSync() => effectiveLevel != syncLevel;

    [Client]
    public void SyncTileLevel(Transform[] standingOnTop)
    {
        effectiveLevel = syncLevel;

        transform.DOScaleY(height, .3f);

        standingOnTop
            .Map(tr => tr.DOMoveY(height, .3f));
    }


    [Command(requiresAuthority = false)]
    public void CmdIncreaseLevel(int inc)
    {
        syncLevel += inc;
    }

    [Command(requiresAuthority = false)]
    public void CmdDecreaseLevel(int dec)
    {
        int newLevel = syncLevel - dec;
        syncLevel = Math.Max(0, newLevel);
    }

}
