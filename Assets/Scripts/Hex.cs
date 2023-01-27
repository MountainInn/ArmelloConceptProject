using UnityEngine;
using MountainInn;

abstract public class Hex
{
    public Vector2Int coordinates {get; protected set;}
    public SpriteRenderer viewPrefab {get; protected set;}

    public Hex(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
    }

    abstract public void OnStepped();

    public enum Type
    {
        Forest, Mountain, Lake, Sand
    }
    public static Hex RealizeSyncData(HexSyncData syncData)
    {
        var coord = syncData.coord;
        var subtype = syncData.hexSubtype;
       
        return subtype switch
        {
            (Hex.Type.Forest) => new Forest(coord),
                (Hex.Type.Mountain) => new Mountain(coord),
                (Hex.Type.Lake) => new Lake(coord),
                (Hex.Type.Sand) => new Sand(coord),
                (_) => throw new System.Exception($"Hex subtype {subtype} not handled by RealizeSubtype function")
                };
    }

    public static Hex.Type GetRandomType()
    {
        return (Hex.Type)System.Enum.GetValues(typeof(Hex.Type)).GetRandom();
    }
}

public struct HexSyncData
{
    public Hex.Type hexSubtype;
    public Vector2Int coord;

    public HexSyncData(Hex.Type hexSubtype, Vector2Int coord)
    {
        this.hexSubtype = hexSubtype;
        this.coord = coord;
    }
}

public class Forest : Hex
{
    public Forest(Vector2Int coordinates) : base(coordinates)
    {
    }

    public override void OnStepped()
    {

    }
}

public class Mountain : Hex
{
    public Mountain(Vector2Int coordinates) : base(coordinates)
    {
    }

    public override void OnStepped()
    {

    }
}

public class Lake : Hex
{
    public Lake(Vector2Int coordinates) : base(coordinates)
    {
    }

    public override void OnStepped()
    {

    }
}

public class Sand : Hex
{
    public Sand(Vector2Int coordinates) : base(coordinates)
    {
    }

    public override void OnStepped()
    {

    }
}
