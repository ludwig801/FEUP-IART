public class MovePawn : Move
{
    public Pawn Pawn;
    public Tile Source, Destination;

    public MovePawn(Pawn pawn, Tile src, Tile dest)
    {
        Type = Types.MovePawn;
        Pawn = pawn;
        Source = src;
        Destination = dest;
    }
}
