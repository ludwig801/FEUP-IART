public class PlaceWall : Move
{
    public Tile Tile;
    public bool Horizontal;

    public PlaceWall(Tile where, bool isHorizontal)
    {
        Type = Types.PlaceWall;
        Tile = where;
        Horizontal = isHorizontal;
    }
}
