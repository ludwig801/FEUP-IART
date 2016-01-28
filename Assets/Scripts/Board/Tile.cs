using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    GameManager manager;

	public List<Tile> neighbors;
	public int row;
	public int col;

    // AStar
    public int AStarCostValue;
    public int AStarHeuristicValue;
	public int AStarFunctionValue { get { return AStarCostValue + AStarHeuristicValue; } }
    public Tile AStarPathParent;

	private Wall wall;
	private Pawn pawn;
	private bool mouseOver = false;
	private bool highlight = false;
    private bool selected = false;
	
	public Color color;
	public Color hoverColor;
	public Color highlightColor;
    public Color selectedColor;
	private Material rendererMaterial;
	private float transitionModifier = 50f;

	// Properties
	public bool Highlight
	{
		set
		{
            if (value)
            {
                highlight = true;
            }
            else
            {
                bool fade = true;
                foreach (Tile tile in neighbors)
                {
                    if (tile.selected)
                    {
                        fade = false;
                    }
                }
                if (fade)
                {
                    highlight = false;
                }
            }
		}
	}

    public bool Selected
    {
        get
        {
            return selected;
        }

        set
        {
            selected = !selected;
            foreach (Tile tile in neighbors)
            {
                tile.Highlight = selected;
            }
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
    void Start()
    {
        manager = GameObject.Find(Names.GameManager).GetComponent<GameManager>();
    }

	void Update()
	{
        if (selected)
        {
            rendererMaterial.color = Color.Lerp(rendererMaterial.color, selectedColor, transitionModifier * Time.deltaTime);
        }
		else if(mouseOver)
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
		rendererMaterial = GetComponent<Renderer>().material;
		//color = rendererMaterial.color;
        Reset();
	}

    public void Reset()
    {
        wall = null;
        pawn = null;
        AStarPathParent = null;
        AStarCostValue = 0;
        AStarHeuristicValue = 0;
    }

	void OnMouseEnter()
	{
		mouseOver = true;
        Highlight = true;
        foreach (Tile tile in neighbors)
        {
            tile.Highlight = true;
        }
	}

    void OnMouseUpAsButton()
    {
        if (!selected)
        {
            manager.OnTileSelected(this);
        }
        else
        {
            manager.OnTileDeselected(this);
        }
    }

	void OnMouseExit()
	{
		mouseOver = false;
        Highlight = false;
        if (!selected)
        {
            foreach (Tile tile in neighbors)
            {
                tile.Highlight = false;
            }
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

    public void RemoveNeighbor(Tile b)
    {
        if (neighbors.Contains(b))
        {
            b.Highlight = false;
            neighbors.Remove(b);
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

    public bool IsNeighborOf(Tile b)
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
