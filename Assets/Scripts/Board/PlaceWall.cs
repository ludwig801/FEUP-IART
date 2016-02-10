public class PlaceWall : Move
{
    public Tile Tile;
    public bool Horizontal;

    public PlaceWall(Tile tile, bool isHorizontal)
    {
        Type = Types.PlaceWall;
        Tile = tile;
        Horizontal = isHorizontal;
    }

    public override string ToString()
    {
        return string.Format("Place Wall at " + Tile.ToString());
    }
}
