using UnityEngine;
using MountainInn;

abstract public class Hex
{
    public Vector2Int coordinates {get; protected set;}

    public Hex(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
    }

    abstract public void OnStepped();

    public enum Type
    {
        Forest, Mountain, Lake, Sand
    }

    public static Hex.Type GetRandomType()
    {
        return System.Enum.GetValues(typeof(Hex.Type)).ArrayGetRandom<Hex.Type>();
    }
}

public struct HexSyncData
{
    public Hex.Type hexSubtype;
    public Vector3Int coord;

    public HexSyncData(Hex.Type hexSubtype, Vector3Int coord)
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
