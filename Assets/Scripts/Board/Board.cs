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

    public void Reset()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < Border)
                {
                    RemoveLink(tiles[i, j], tiles[i, j + 1]);
                }
                if (i < Border)
                {
                    RemoveLink(tiles[i, j], tiles[i + 1, j]);
                }
            }
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < Border)
                {
                    AddLink(tiles[i, j], tiles[i, j + 1]);
                }
                if (i < Border)
                {
                    AddLink(tiles[i, j], tiles[i + 1, j]);
                }
            }
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                tiles[i, j].Reset();
            }
        }
    }

    void GenerateBoard()
    {
        if (tileTransform == null || tiles == null)
        {
            tileTransform = transform.FindChild(Names.Tiles);
            tiles = new Tile[rows, columns];
        }

        float spacing = tileSpacing * tileSize;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 position = new Vector3(j * (tileSize + spacing), 0, i * (tileSize + spacing));
                GameObject instance = GameObject.Instantiate(tilePrefab, position, Quaternion.identity) as GameObject;
                instance.name = "Tile_" + (i * columns + j);
                instance.transform.SetParent(transform.FindChild(Names.Tiles));
                instance.transform.localScale = new Vector3(tileSize, instance.transform.localScale.y, tileSize);
                tiles[i, j] = instance.GetComponent<Tile>();
                tiles[i, j].row = i;
                tiles[i, j].col = j;
                tiles[i, j].Init();
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
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < Border)
                {
                    RemoveLink(tiles[i, j], tiles[i, j + 1]);
                }
                if (i < Border)
                {
                    RemoveLink(tiles[i, j], tiles[i + 1, j]);
                }
            }
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < Border)
                {
                    AddLink(tiles[i, j], tiles[i, j + 1]);
                }
                if (i < Border)
                {
                    AddLink(tiles[i, j], tiles[i + 1, j]);
                }
            }
        }
    }

    void AddLink(Tile a, Tile b)
    {
        if (a.Neighbor(b) || b.Neighbor(a))
        {
            return;
        }

        a.neighbors.Add(b);
        b.neighbors.Add(a);

        Link link = GetLink();
        link.Visible = true;
        link.SetTiles(a, b);
    }

    void RemoveLink(Tile a, Tile b)
    {
        Link link = GetLink(a, b);
        if (link != null)
        {
            link.Visible = false;
            link.RemoveTiles();
        }

        a.RemoveNeighbor(b);
        b.RemoveNeighbor(a);
    }

    Link GetLink()
    {
        // If an available Link already exists in the pool
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].Free)
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
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i].TileA == a)
            {
                if (links[i].TileB == b)
                {
                    return links[i];
                }
            }
            else if (links[i].TileA == b)
            {
                if (links[i].TileB == a)
                {
                    return links[i];
                }
            }
        }

        return null;
    }

    public void DestroyBoard()
    {
        if (tileTransform == null)
        {
            tileTransform = transform.FindChild(Names.Tiles);
            tiles = null;
        }

        while (tileTransform.childCount > 0)
        {
            DestroyImmediate(tileTransform.GetChild(0).gameObject);
        }
    }

    public Tile GetPointedTile()
    {
        for (int i = 0; i < tileTransform.childCount; i++)
        {
            if (tileTransform.GetChild(i).GetComponent<Tile>().HasMouseOver)
            {
                return tileTransform.GetChild(i).GetComponent<Tile>();
            }
        }

        return null;
    }

    public Tile GetTileAt(Vector3 position)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.transform.position.x == position.x && tile.transform.position.z == position.z)
            {
                return tile;
            }
        }

        return null;
    }

    public Tile GetTileAt(int row, int col)
    {
        return tiles[row, col];
    }

    public bool IsValidPosition(int mRow, int mCol)
    {
        return (mRow >= 0 && mRow <= Border && mCol >= 0 && mCol <= Border);
    }

    public void MovePawnTo(Pawn pawn, int row, int col)
    {
        pawn.Tile.RemovePawn();
        pawn.Tile = tiles[row, col];
    }

    public void SetWall(Wall wall, int row, int col, bool horizontal)
    {
        Tile tile = GetTileAt(row, col);
        Tile right = GetTileAt(tile.row, tile.col + 1);
        Tile below = GetTileAt(tile.row - 1, tile.col);
        Tile rightBelow = GetTileAt(tile.row - 1, tile.col + 1);

        if (horizontal)
        {
            RemoveLink(tile, below);
            RemoveLink(right, rightBelow);
            RemoveLink(right, below);
        }
        else
        {
            RemoveLink(tile, right);
            RemoveLink(below, rightBelow);
            RemoveLink(tile, rightBelow);
        }

        wall.Tile = tiles[row, col];
        wall.Horizontal = horizontal;
    }

    public void RemoveWall(int row, int col)
    {
        Tile tile = GetTileAt(row, col);
        Wall wall = tile.Wall;

        Tile right = GetTileAt(tile.row, tile.col + 1);
        Tile below = GetTileAt(tile.row - 1, tile.col);
        Tile rightBelow = GetTileAt(tile.row - 1, tile.col + 1);

        if (wall.Horizontal)
        {
            AddLink(tile, below);
            AddLink(right, rightBelow);
        }
        else
        {
            AddLink(tile, right);
            AddLink(below, rightBelow);
        }

        tile.RemoveWall();
    }

    public void SetTempLinks(Tile tile)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (Tile x in tile.neighbors)
        {
            if (Tile.Distance(tile, x) == 1)
            {
                tiles.Add(x);
            }
        }

        foreach (Tile a in tiles)
        {
            foreach (Tile b in tiles)
            {
                if (!a.Equals(b))
                {
                    if (CanBeTempNeighbors(a, b, tile))
                    {
                        AddLink(a, b);
                    }
                }
            }
        }
    }

    public void RemoveTempLinks(Tile tile)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (Tile x in tile.neighbors)
        {
            if (Tile.Distance(tile, x) == 1)
            {
                tiles.Add(x);
            }
        }

        foreach (Tile a in tiles)
        {
            foreach (Tile b in tiles)
            {
                if (!a.Equals(b))
                {
                    RemoveLink(a, b);
                }
            }
        }
    }

    public bool CanBeTempNeighbors(Tile a, Tile b, Tile center)
    {
        if (Tile.SameRow(a, b))
        {
            Tile left = a.Leftside(b) ? a : b;
            Tile right = a.Leftside(b) ? b : a;

            while (!left.Equals(right))
            {
                if (left.HasWall && left.Wall.Vertical)
                {
                    return false;
                }
                
                if (IsValidPosition(left.row + 1, left.col))
                {
                    Tile above = tiles[left.row + 1, left.col];
                    if (above.HasWall && above.Wall.Vertical)
                    {
                        return false;
                    }
                }

                left = tiles[left.row, left.col + 1];
            }

            return true;
        }
        else if (Tile.SameCol(a, b))
        {
            Tile below = a.Below(b) ? a : b;
            Tile above = a.Below(b) ? b : a;

            while (!above.Equals(below))
            {
                if (above.HasWall && above.Wall.Horizontal)
                {
                    return false;
                }

                if (IsValidPosition(above.row, above.col - 1))
                {
                    Tile left = tiles[above.row, above.col - 1];
                    if (left.HasWall && left.Wall.Horizontal)
                    {
                        return false;
                    }
                }

                above = tiles[above.row - 1, above.col];
            }

            return true;
        }
        else
        {
            Tile comp = a.Above(b) ? a : b;
            Tile notComp = b.Equals(comp) ? a : b;

            if (comp.Leftside(notComp))
            {
                if (comp.Above(center))
                {
                    if (comp.HasWall && (comp.Wall.Horizontal || comp.Wall.Vertical))
                    {
                        return false;
                    }
                    else if(center.HasWall && center.Wall.Vertical)
                    {
                        return false;
                    }
                    else if (IsValidPosition(comp.row, comp.col - 1))
                    {
                        Tile left = tiles[comp.row, comp.col - 1];
                        if (left.HasWall && left.Wall.Horizontal)
                        {
                            return false;
                        }
                    }
                }
                else if (comp.Leftside(center))
                {
                    if (comp.HasWall && (comp.Wall.Horizontal || comp.Wall.Vertical))
                    {
                        return false;
                    }
                    else if (center.HasWall && center.Wall.Horizontal)
                    {
                        return false;
                    }
                    else if (IsValidPosition(comp.row + 1, comp.col))
                    {
                        Tile above = tiles[comp.row + 1, comp.col];
                        if (above.HasWall && above.Wall.Vertical)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else // comp rightside to notComp
            {
                if (comp.Above(center))
                {
                    if (comp.HasWall && comp.Wall.Horizontal)
                    {
                        return false;
                    }
                    else if (notComp.HasWall && notComp.Wall.Vertical)
                    {
                        return false;
                    }
                    else if (IsValidPosition(comp.row, comp.col - 1))
                    {
                        Tile left = tiles[comp.row, comp.col - 1];
                        if (left.HasWall && (left.Wall.Vertical || left.Wall.Horizontal))
                        {
                            return false;
                        }
                    }
                }
                else if (comp.Rightside(center))
                {
                    if (center.HasWall && (center.Wall.Horizontal || center.Wall.Vertical))
                    {
                        return false;
                    }
                    else if (IsValidPosition(comp.row, comp.col - 2))
                    {
                        Tile left = tiles[comp.row, comp.col - 2];
                        if (left.HasWall && left.Wall.Horizontal)
                        {
                            return false;
                        }
                    }
                    else if (IsValidPosition(comp.row + 1, comp.col - 1))
                    {
                        Tile left = tiles[comp.row + 1, comp.col - 1];
                        if (left.HasWall && left.Wall.Vertical)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
