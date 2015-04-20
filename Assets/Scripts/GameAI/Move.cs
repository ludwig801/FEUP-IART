using UnityEngine;
using System.Collections;

public class Move
{
	public const int MovePawn = 0;
	public const int SetWall = 1;

	public int type;
	public int row, col;
	public bool isHorizontal;

	public Move(Move move)
	{
		this.type = move.type;
		this.row = move.row;
		this.col = move.col;
		this.isHorizontal = move.isHorizontal;
	}

	public Move(int row, int col)
	{
		this.type = MovePawn;
		this.row = row;
		this.col = col;
	}

	public Move(int row, int col, bool horizontal)
	{
		this.type = SetWall;
		this.row = row;
		this.col = col;
		this.isHorizontal = horizontal;
	}

	public override string ToString ()
	{
		if(type == MovePawn)
		{
			return "Move Pawn to: " + row + " " + col;
		}
		else
		{
			return "Set Wall at: " + row + " " + col + " (" + isHorizontal + ")";
		}
	}
}
