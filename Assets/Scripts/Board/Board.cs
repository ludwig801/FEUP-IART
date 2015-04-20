using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
	public GameObject tilePrefab;
	public GameObject linkPrefab;

	public int tileSize;
	public float tileSpacing;

	public int rows;
	public int columns;

	private Tile[,] tiles;
	private Transform tileTransform;
	private List<Link> links;
	
	// Properties
	public int Size
	{
		get
		{
			return rows;
		}

		set
		{
			rows = value;
			columns = value;
		}
	}

	public Tile[,] Tiles
	{
		get
		{
			return tiles;
		}
	}

	public int Border
	{
		get
		{
			return (Size - 1);
		}
	}

	public bool HasBoard
	{
		get
		{
			return (transform.FindChild(Names.Tiles).childCount > 0);
		}
	}
	
	// Methods
	void Start()
	{
		tileTransform = transform.FindChild(Names.Tiles);
	}

	public void Init()
	{
		GenerateBoard();
		CreateLinkPrefabs();
		CalcNeighbors();
	}
	
	void GenerateBoard()
	{
		if(tileTransform == null || tiles == null)
		{
			tileTransform = transform.FindChild(Names.Tiles);
			tiles = new Tile[rows,columns];
		}
		
		float spacing = tileSpacing * tileSize;
		
		for(int i = 0; i < rows; i++)
		{
			for(int j = 0; j < columns; j++)
			{
				Vector3 position = new Vector3(j * (tileSize + spacing), 0, i * (tileSize + spacing));
				GameObject instance = GameObject.Instantiate(tilePrefab,position,Quaternion.identity) as GameObject;
				instance.name = "Tile_" + (i * columns + j);
				instance.transform.SetParent(transform.FindChild(Names.Tiles));
				instance.transform.localScale = new Vector3(tileSize,instance.transform.localScale.y,tileSize);
				tiles[i,j] = instance.GetComponent<Tile>();
				tiles[i,j].row = i;
				tiles[i,j].col = j;
				tiles[i,j].Init();
			}
		}
		
		//Transform mainCamera = GameObject.Find(Tags.MainCamera).transform;
		//mainCamera.position = new Vector3(0.5f * columns * (tileSize + spacing) - 0.5f * tileSize, mainCamera.position.y, mainCamera.position.z);
	}
	
	void CreateLinkPrefabs()
	{
		links = new List<Link>();
	}

	void CalcNeighbors()
	{
		for(int i = 0; i < rows; i++)
		{
			for(int j = 0; j < columns; j++)
			{
				if(j < Border)
				{
					AddLink(tiles[i,j], tiles[i,j+1]);
				}
				if(i < Border)
				{
					AddLink(tiles[i,j], tiles[i+1,j]);
				}
			}
		}
	}

	void AddLink(Tile a, Tile b)
	{
		if(a.Neighbor(b) || b.Neighbor(a))
		{
			return;
		}

		a.neighbors.Add(b);
		b.neighbors.Add(a);

		Link link = GetLink();
		link.Visible = true;
		link.SetTiles(a,b);
	}

	void RemoveLink(Tile a, Tile b)
	{
		Link link = GetLink(a,b);
		if(link != null)
		{
			link.Visible = false;
			link.RemoveTiles();
		}

		a.neighbors.Remove (b);
		b.neighbors.Remove (a);
	}

	Link GetLink()
	{
		// If an available Link already exists in the pool
		for(int i = 0; i < links.Count; i++)
		{
			if(links[i].Free)
			{
				return links[i];
			}
		}

		// Else create a new and add to the pool
		GameObject instance = GameObject.Instantiate(linkPrefab, new Vector3(), Quaternion.identity) as GameObject;
		instance.transform.SetParent(GameObject.Find(Names.Board).transform.FindChild(Names.Links));
		Link newLink = instance.GetComponent<Link>();
		newLink.Init();
		links.Add(newLink);
		return newLink;
	}

	Link GetLink(Tile a, Tile b)
	{
		for(int i = 0; i < links.Count; i++)
		{
			if(links[i].TileA == a)
			{
				if(links[i].TileB == b)
				{
					return links[i];
				}
			}
			else if(links[i].TileA == b)
			{
				if(links[i].TileB == a)
				{
					return links[i];
				}
			}
		}

		return null;
	}
	
	public void DestroyBoard()
	{
		if(tileTransform == null)
		{
			tileTransform = transform.FindChild(Names.Tiles);
			tiles = null;
		}
		
		while(tileTransform.childCount > 0)
		{
			DestroyImmediate(tileTransform.GetChild(0).gameObject);
		}
	}
	
	public Tile GetPointedTile()
	{
		for(int i = 0; i < tileTransform.childCount; i++)
		{
			if(tileTransform.GetChild(i).GetComponent<Tile>().HasMouseOver)
			{
				return tileTransform.GetChild(i).GetComponent<Tile>();
			}
		}
		
		return null;
	}
	
	public Tile GetTileAt(Vector3 position)
	{
		foreach(Tile tile in tiles)
		{
			if(tile.transform.position.x == position.x && tile.transform.position.z == position.z)
			{
				return tile;
			}
		}
		
		return null;
	}
	
	public Tile GetTileAt(int row, int col)
	{
		return tiles[row,col];
	}

	public bool IsValidPosition(int mRow, int mCol)
	{
		return (mRow >= 0 && mRow <= Border && mCol >= 0 && mCol <= Border);
	}

	public void MovePawnTo(Pawn pawn, int row, int col)
	{
		pawn.Tile.RemovePawn();
		pawn.Tile = tiles[row,col];
	}

	public void SetWall(Wall wall, int row, int col, bool horizontal)
	{
		Tile tile = GetTileAt(row, col);
		Tile right = GetTileAt(tile.row, tile.col+1);
		Tile below = GetTileAt(tile.row-1, tile.col);
		Tile rightBelow = GetTileAt(tile.row-1, tile.col+1);
		
		if(horizontal)
		{
			RemoveLink(tile, below);
			RemoveLink(right, rightBelow);
		}
		else
		{
			RemoveLink(tile, right);
			RemoveLink(below, rightBelow);
		}

		if(right.Neighbor(below) || below.Neighbor(right))
		{
			RemoveLink(right,below);
			wall.TempLinkBroken = true;
		}

		wall.Tile = tiles[row,col];
		wall.Horizontal = horizontal;
	}

	public void RemoveWall(int row, int col)
	{
		Tile tile = GetTileAt(row, col);
		Wall wall = tile.Wall;
		
		Tile right = GetTileAt(tile.row, tile.col+1);
		Tile below = GetTileAt(tile.row-1, tile.col);
		Tile rightBelow = GetTileAt(tile.row-1, tile.col+1);
		
		if(wall.Horizontal)
		{
			AddLink(tile, below);
			AddLink(right, rightBelow);
		}
		else
		{
			AddLink(tile, right);
			AddLink(below, rightBelow);
		}
		
		if(wall.TempLinkBroken)
		{
			wall.TempLinkBroken = false;
			AddLink(right,below);
		}

		tile.RemoveWall();
	}

	public void SetTempLinks(Tile tile)
	{
		List<Tile> tiles = new List<Tile>();

		foreach(Tile x in tile.neighbors)
		{
			if(Tile.Distance(tile,x) == 1)
			{
				tiles.Add(x);
			}
		}

		//print ("SET Temp Link Tiles : " + tiles.Count);

		foreach(Tile a in tiles)
		{
			foreach(Tile b in tiles)
			{
				if(!a.Equals(b))
				{
					if(CanBeNeighbors(a,b))
					{
						AddLink(a,b);
					}
				}
			}
		}
	}
	
	public void RemoveTempLinks(Tile tile)
	{
		List<Tile> tiles = new List<Tile>();
		
		foreach(Tile x in tile.neighbors)
		{
			if(Tile.Distance(tile,x) == 1)
			{
				tiles.Add(x);
			}
		}

		//print ("REMOVE Temp Link Tiles : " + tiles.Count);

		foreach(Tile a in tiles)
		{
			foreach(Tile b in tiles)
			{
				if(!a.Equals(b))
				{
					RemoveLink(a,b);
				}
			}
		}
	}

	public bool CanBeNeighbors(Tile a, Tile b)
	{
		if(Tile.Contiguous(a,b))
		{
			if(Tile.SameRow(a,b))
			{
				Tile comp = a.Leftside(b) ? a : b;

				if(comp.HasWall && comp.Wall.Vertical)
				{
					return false;
				}
				
				if(IsValidPosition(comp.row+1,comp.col))
				{
					Tile temp = tiles[comp.row+1,comp.col];
					if(temp.HasWall && temp.Wall.Vertical)
					{
						return false;
					}
				}
				
				return true;
			}
			else if(Tile.SameCol(a,b))
			{
				Tile comp = a.Below(b) ? a : b;
				
				if(comp.HasWall && comp.Wall.Horizontal)
				{
					return false;
				}
				
				if(IsValidPosition(comp.row,comp.col-1))
				{
					Tile temp = tiles[comp.row,comp.col-1];
					if(temp.HasWall && temp.Wall.Horizontal)
					{
						return false;
					}
				}
				
				return true;				
			}
			else
			{
				Tile comp = a.Above(b) ? a : b;
				Tile notComp = b.Equals(comp) ? a : b;

				if(comp.HasWall && comp.Wall.Horizontal)
				{
					return false;
				}

				if(comp.Rightside(notComp))
				{
					Tile temp = tiles[comp.row,comp.col-1];
					if(temp.HasWall && temp.Wall.Horizontal)
					{
						return false;
					}
				}

				return true;
			}
		}
		else
		{
			float dist = Tile.Distance(a,b);

			if(dist <= 2f)
			{
				return true;
			}
		}
		
		return false;
	}

	public void SetTilesValues(int value)
	{
		for(int i = 0; i < Size; i++)
		{
			for(int j = 0; j < Size; j++)
			{
				tiles[i,j].SetValue(0,value);
				tiles[i,j].SetValue(1,value);
			}
		}
	}
}
