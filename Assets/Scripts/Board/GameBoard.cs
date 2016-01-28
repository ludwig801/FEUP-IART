using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    public Transform TilesTransform, LinksTransform, VisualBoardTransform;
    public GameObject tilePrefab, linkPrefab;

    [Range(1, 5)]
    public int tileSize;
    [Range(1, 5)]
    public float tileSpacing;
    public int rows;
    public int columns;
    public Tile[,] Tiles;
    public List<Link> Links;

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

    public int Border
    {
        get
        {
            return Size - 1;
        }
    }

    public void Init()
    {
        GenerateBoard();
        CalcNeighbors();
    }

    public void Reset()
    {
        CalcNeighbors();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Tiles[i, j].Reset();
            }
        }
    }

    void GenerateBoard()
    {
        if (Tiles == null)
        {
            Tiles = new Tile[rows, columns];
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
                Tiles[i, j] = instance.GetComponent<Tile>();
                Tiles[i, j].row = i;
                Tiles[i, j].col = j;
                Tiles[i, j].Init();
            }
        }
    }

    void CalcNeighbors()
    {
        var border = Size - 1;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < border)
                {
                    RemoveLinkAt(Tiles[i, j], Tiles[i, j + 1]);
                }
                if (i < border)
                {
                    RemoveLinkAt(Tiles[i, j], Tiles[i + 1, j]);
                }
            }
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (j < border)
                {
                    AddLinkAt(Tiles[i, j], Tiles[i, j + 1]);
                }
                if (i < border)
                {
                    AddLinkAt(Tiles[i, j], Tiles[i + 1, j]);
                }
            }
        }
    }

    void AddLinkAt(Tile a, Tile b)
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

    void RemoveLinkAt(Tile a, Tile b)
    {
        Link link = GetLinkAt(a, b);
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
        for (int i = 0; i < Links.Count; i++)
        {
            if (Links[i].Free)
            {
                return Links[i];
            }
        }

        // Else create a new and add to the pool
        GameObject instance = GameObject.Instantiate(linkPrefab, new Vector3(), Quaternion.identity) as GameObject;
        instance.transform.SetParent(GameObject.Find(Names.Board).transform.FindChild(Names.Links));
        Link newLink = instance.GetComponent<Link>();
        newLink.Init();
        Links.Add(newLink);
        return newLink;
    }

    Link GetLinkAt(Tile a, Tile b)
    {
        for (int i = 0; i < Links.Count; i++)
        {
            if (Links[i].TileA == a)
            {
                if (Links[i].TileB == b)
                {
                    return Links[i];
                }
            }
            else if (Links[i].TileA == b)
            {
                if (Links[i].TileB == a)
                {
                    return Links[i];
                }
            }
        }

        return null;
    }

    public Tile GetTileAt(Vector3 position)
    {
        foreach (Tile tile in Tiles)
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
        return Tiles[row, col];
    }

    public bool IsBoardPosition(int row, int col)
    {
        return (row >= 0) &&
        (row < Size) &&
        (col >= 0) &&
        (col < Size);
    }

    public void MovePawnTo(Pawn pawn, int row, int col)
    {
        pawn.Tile.RemovePawn();
        pawn.Tile = Tiles[row, col];
    }

    public void PutWallAt(Wall wall, int row, int col, bool horizontal)
    {
        Tile tile = GetTileAt(row, col);
        Tile right = GetTileAt(tile.row, tile.col + 1);
        Tile below = GetTileAt(tile.row - 1, tile.col);
        Tile rightBelow = GetTileAt(tile.row - 1, tile.col + 1);

        if (horizontal)
        {
            RemoveLinkAt(tile, below);
            RemoveLinkAt(right, rightBelow);
            RemoveLinkAt(right, below);
        }
        else
        {
            RemoveLinkAt(tile, right);
            RemoveLinkAt(below, rightBelow);
            RemoveLinkAt(tile, rightBelow);
        }

        wall.Tile = Tiles[row, col];
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
            AddLinkAt(tile, below);
            AddLinkAt(right, rightBelow);
        }
        else
        {
            AddLinkAt(tile, right);
            AddLinkAt(below, rightBelow);
        }

        tile.RemoveWall();
    }

    public void CreateTileTempLinks(Tile tile)
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
                        AddLinkAt(a, b);
                    }
                }
            }
        }
    }

    public void RemoveTileTempLinks(Tile tile)
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
                    RemoveLinkAt(a, b);
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
                
                if (IsBoardPosition(left.row + 1, left.col))
                {
                    Tile above = Tiles[left.row + 1, left.col];
                    if (above.HasWall && above.Wall.Vertical)
                    {
                        return false;
                    }
                }

                left = Tiles[left.row, left.col + 1];
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

                if (IsBoardPosition(above.row, above.col - 1))
                {
                    Tile left = Tiles[above.row, above.col - 1];
                    if (left.HasWall && left.Wall.Horizontal)
                    {
                        return false;
                    }
                }

                above = Tiles[above.row - 1, above.col];
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
                    else if (center.HasWall && center.Wall.Vertical)
                    {
                        return false;
                    }
                    else if (IsBoardPosition(comp.row, comp.col - 1))
                    {
                        Tile left = Tiles[comp.row, comp.col - 1];
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
                    else if (IsBoardPosition(comp.row + 1, comp.col))
                    {
                        Tile above = Tiles[comp.row + 1, comp.col];
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
            else
            { // comp rightside to notComp
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
                    else if (IsBoardPosition(comp.row, comp.col - 1))
                    {
                        Tile left = Tiles[comp.row, comp.col - 1];
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
                    else if (IsBoardPosition(comp.row, comp.col - 2))
                    {
                        Tile left = Tiles[comp.row, comp.col - 2];
                        if (left.HasWall && left.Wall.Horizontal)
                        {
                            return false;
                        }
                    }
                    else if (IsBoardPosition(comp.row + 1, comp.col - 1))
                    {
                        Tile left = Tiles[comp.row + 1, comp.col - 1];
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
