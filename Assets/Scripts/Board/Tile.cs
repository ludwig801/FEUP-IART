using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
	public List<Tile> neighbors;
	public int row;
	public int col;

    // AStar
    public int gValue;
    public int hValue;
    public int fValue;
    public Tile parent;

	private Wall wall;
	private Pawn pawn;
	private bool mouseOver = false;
	private bool highlight = false;
	
	public Color color;
	public Color hoverColor;
	public Color highlightColor;
	private Material rendererMaterial;
	private float transitionModifier = 5f;
	
	// Properties
	public bool Highlight
	{
		set
		{
			highlight = value;
		}
	}

	public Wall Wall
	{
		get
		{
			return wall;
		}
	}

	public Pawn Pawn
	{
		get
		{
			return pawn;
		}
	}

	public bool HasPawn
	{
		get
		{
			return (pawn != null);
		}
	}

	public bool HasWall
	{
		get
		{
			return (wall != null);
		}
	}

	public bool HasMouseOver
	{
		get
		{
			return mouseOver;
		}
	}

	// Methods
	void Update()
	{
		if(mouseOver)
		{
			rendererMaterial.color = Color.Lerp(rendererMaterial.color, hoverColor, transitionModifier * Time.deltaTime);
		}
		else if(highlight)
		{
			rendererMaterial.color = Color.Lerp(rendererMaterial.color, highlightColor, transitionModifier * Time.deltaTime);
		}
		else
		{
			rendererMaterial.color = Color.Lerp(rendererMaterial.color, color, transitionModifier * Time.deltaTime);
		}
	}

	public void Init()
	{
		neighbors = new List<Tile> ();
		pawn = null;
		rendererMaterial = GetComponent<Renderer>().material;
		color = rendererMaterial.color;
	}

	void OnMouseEnter()
	{
		mouseOver = true;
		foreach(Tile tile in neighbors)
		{
			tile.Highlight = true;
		}
	}

	void OnMouseExit()
	{
		mouseOver = false;
		foreach(Tile tile in neighbors)
		{
			tile.Highlight = false;
		}
	}

	public void SetPawn(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void SetWall(Wall wall)
	{
		this.wall = wall;
	}

	public void RemovePawn()
	{
		if(pawn != null)
		{
			pawn.Tile = null;
			pawn = null;
		}
	}

	public void RemoveWall()
	{
		if(wall != null)
		{
			wall.Tile = null;
			wall = null;
		}
	}

	public override string ToString()	
	{
		return "[" + row + " " + col + "]";
	}

	public bool SamePos(Tile b)
	{
		return SamePos(b.row, b.col);
	}

	public bool SamePos(int row, int col)
	{
		return (this.row == row && this.col == col);
	}

	public bool Above(Tile b)
	{
		return this.row > b.row;
	}

	public bool Below(Tile b)
	{
		return this.row < b.row;
	}

	public bool Rightside(Tile b)
	{
		return this.col > b.col;
	}

	public bool Leftside(Tile b)
	{
		return this.col < b.col;
	}

	public bool Neighbor(Tile b)
	{
		return neighbors.Contains(b);
	}

    // Class Methods
    public static bool Contiguous(Tile a, Tile b)
    {
        return (Distance(a, b) < 2);
    }

    public static bool SameRow(Tile a, Tile b)
    {
        return (a.row == b.row);
    }

    public static bool SameCol(Tile a, Tile b)
    {
        return (a.col == b.col);
    }

	public static float Distance(Tile a, Tile b)
	{
		return Mathf.Sqrt(Mathf.Pow(a.row - b.row,2) + Mathf.Pow(a.col - b.col,2));
	}
}
