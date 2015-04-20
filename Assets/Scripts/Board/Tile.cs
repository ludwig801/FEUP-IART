using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
	public List<Tile> neighbors;

	public int row;
	public int col;
	private int[] value;
	private Tile[] child;
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
		value = new int[2];
		value[0] = value[1] = int.MaxValue;
		child = new Tile[2];
		child[0] = child[1] = null;
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

	public int GetValue(int player)
	{
		return value[player];
	}

	public void SetValue(int player, int value)
	{
		this.value[player] = value;
	}

	public Tile GetChild(int player)
	{
		return child[player];
	}

	public void SetChild(int player, Tile tile)
	{
		this.child[player] = tile;
	}

	public override string ToString()	
	{
		return "[" + row + " " + col + "]";
	}

	// Class Methods
	public static bool Contiguous(Tile a, Tile b)
	{
		return (Distance(a,b) < 2);
	}

	public static bool SameRow(Tile a, Tile b)
	{
		return (a.row == b.row);
	}

	public static bool SameCol(Tile a, Tile b)
	{
		return (a.col == b.col);
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

	public static float Distance(Tile a, Tile b)
	{
		return Mathf.Sqrt(Mathf.Pow(a.row - b.row,2) + Mathf.Pow(a.col - b.col,2));
	}
}
