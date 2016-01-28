using UnityEngine;
using System.Collections;

public class Move
{
    public const int MovePawn = 0;
    public const int SetWall = 1;

    public int Type;
    public int Row, Col;
    public bool IsHorizontal;

    public Move(Move move)
    {
        Type = move.Type;
        Row = move.Row;
        Col = move.Col;
        IsHorizontal = move.IsHorizontal;
    }

    public Move(int row, int col)
    {
        Type = MovePawn;
		Row = row;
		Col = col;
    }

    public Move(int row, int col, bool horizontal)
    {
        Type = SetWall;
        Row = row;
        Col = col;
        IsHorizontal = horizontal;
    }

    public override string ToString()
    {
        if (Type == MovePawn)
        {
            return "Move Pawn to: " + Row + " " + Col;
        }
        else
        {
            return "Set Wall at: " + Row + " " + Col + " (" + IsHorizontal + ")";
        }
    }
}
