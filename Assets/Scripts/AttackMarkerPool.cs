using UnityEngine;

public class AttackMarkerPool : UniRx.Toolkit.ObjectPool<LineRenderer>
{
    LineRenderer prefab;

    public AttackMarkerPool(LineRenderer prefab)
    {
        this.prefab = prefab;
    }

    protected override LineRenderer CreateInstance()
    {
        var newMarker = GameObject.Instantiate(prefab);
        return newMarker;
    }
}
