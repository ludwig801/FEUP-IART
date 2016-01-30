public class PlaceWall : Move
{
    public Tile Tile;
    public bool Horizontal;

    public PlaceWall(Tile where, bool isHorizontal)
    {
        Tile = where;
        Horizontal = isHorizontal;
    }
}
