using UnityEngine;

public abstract class Move
{
    public enum Types
    {
        MovePawn = 0,
        PlaceWall = 1
    }

    static int NumTypes = System.Enum.GetValues(typeof(Types)).Length;

    public Types Type;

    public static Types GetNextType(Types currentType)
    {
        var nextType = ((int)currentType + 1) % NumTypes;
        return (Types)nextType;
    }
}
