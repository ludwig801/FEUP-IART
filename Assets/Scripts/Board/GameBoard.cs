using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    public static GameBoard Instance
    {
        get
        {
            return _instance;
        }
    }

    static GameBoard _instance;

    public Transform TilesTransform, EdgesTransform, VisualBoard, WallsTransform;
    public GameObject TilePrefab, EdgePrefab, WallPrefab, FocusPrefab;
    public Minimax Minimax;
    public CameraManager Camera;
    [Range(9, 9)]
    public int Size;
    [Range(1, 5)]
    public int TileSize;
    [Range(0, 1)]
    public float TileSpacingFactor;
    [Range(0.25f, 1.25f)]
    public float MinWallWidth;
    [Range(-1, 1)]
    public int CurrentPlayer;
    public Stack<Move> Moves;
    public Move.Types MoveType;
    public Tile FocusedTile;
    public bool ShowEdges, Ongoing;
    public List<Edge> Edges;
    public List<Wall> Walls;
    public List<Player> Players;
    public Tile[,] Tiles;

    Transform _referenceFocused;
    Wall _referenceWall;
    Quaternion _rotateCameraPivotTo;

    public int Border
    {
        get
        {
            return Size - 1;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        Moves = new Stack<Move>();

        _referenceWall = GetNewWall();
        Walls.Remove(_referenceWall);
        _referenceWall.Free = false;
        _referenceWall.name = "Reference Wall";
        _referenceWall.Tile = null;

        CreateTiles();
        CreateEdges();

        _referenceFocused = Instantiate(FocusPrefab).transform;
        _referenceFocused.localScale = new Vector3(0.75f * TileSize, 0.25f, 0.75f * TileSize);

        StartCoroutine(UpdateEdges());

        Ongoing = false;
        //Restart();
    }

    void Update()
    {
        if (Ongoing && !IsGameOver() && Players[CurrentPlayer].IsCpu)
        {
            if (Minimax.IsRunning())
            {
                if (Minimax.InErrorState())
                {
                    Debug.Log("An Error Occurred...");
                }
            }
            else
            {
                if (Minimax.IsFinished())
                {
                    Move best = Minimax.GetResult() as Move;
                    PlayMove(best);
                }
                else
                {
                    Minimax.RunAlgorithm();                 
                }
            }
        }

        HandleKeysInput();

        UpdateVisualElements();
    }

    public void Pause()
    {
        Ongoing = false;
    }

    public void Resume()
    {
        Ongoing = true;
    }

    public void NewGame()
    {
        Ongoing = true;
        PlacePawns();

        CurrentPlayer = -1;
        NextTurn();

        FocusedTile = Players[CurrentPlayer].Pawn.Tile;
    }

    IEnumerator UpdateEdges()
    {
        while (true)
        {
            foreach (var edge in Edges)
            {
                edge.gameObject.SetActive(ShowEdges);
            }

            yield return new WaitForSeconds(1);
        }
    }

    void HandleKeysInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            QuitApplication();

        if (!Ongoing || IsGameOver())
            return;

        if (Players[CurrentPlayer].IsCpu)
            return;

        if (Input.GetKeyUp(KeyCode.Tab))
            ChangeToNextMode();

        if (Input.GetKeyUp(KeyCode.Delete))
            UndoMove();

        if (Input.GetKeyUp(KeyCode.Space))
            OnAction();

        if (Input.GetKeyDown(KeyCode.Q))
            ChangeWallOrientation();

        if (Input.GetKeyDown(KeyCode.A))
            FocusTile(-1, 0);

        if (Input.GetKeyDown(KeyCode.D))
            FocusTile(+1, 0);

        if (Input.GetKeyDown(KeyCode.S))
            FocusTile(0, -1);

        if (Input.GetKeyDown(KeyCode.W))
            FocusTile(0, +1);
    }

    void UpdateVisualElements()
    {
        VisualBoard.localScale = new Vector3(Size * (TileSize + TileSpacingFactor) + TileSize, Size * (TileSize + TileSpacingFactor) + TileSize, 1);

        _referenceFocused.gameObject.SetActive(FocusedTile != null);
        if (FocusedTile != null)
        {
            _referenceFocused.position = Vector3.Lerp(_referenceFocused.position, FocusedTile.transform.position + new Vector3(0, 0.5f, 0), Time.deltaTime * 8f);
            _referenceWall.Invalid = !CanPlaceWall(FocusedTile, _referenceWall.Horizontal);
            _referenceWall.Tile = (MoveType == Move.Types.PlaceWall) ? FocusedTile : null;
        }
    }

    public void FocusTile(int horizontalInput, int verticalInput)
    {
        var rotatedHorizontalInput = (Camera.EulerHorizontal < 45 || Camera.EulerHorizontal >= 315) ? horizontalInput :
            Camera.EulerHorizontal < 135 ? verticalInput : Camera.EulerHorizontal < 225 ? -horizontalInput : -verticalInput;

        var rotatedVerticalInput = (Camera.EulerHorizontal < 45 || Camera.EulerHorizontal >= 315) ? verticalInput :
            Camera.EulerHorizontal < 135 ? -horizontalInput : Camera.EulerHorizontal < 225 ? -verticalInput : horizontalInput;

        if (IsBoardPosition(FocusedTile.Row + rotatedVerticalInput, FocusedTile.Col + rotatedHorizontalInput))
        {
            FocusedTile = Tiles[FocusedTile.Row + rotatedVerticalInput, FocusedTile.Col + rotatedHorizontalInput]; 
        } 
    }

    public void ChangeWallOrientation()
    {
        _referenceWall.Horizontal = !_referenceWall.Horizontal;
    }

    public void ChangeToNextMode()
    {
        MoveType = Move.GetNextType(MoveType);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    void SelectTile(Tile tile)
    {
        if (tile == null)
            return;
        
        tile.Selected = true;
        foreach (var edge in tile.Edges)
        {
            var neighbor = edge.GetNeighborOf(tile);
            if (neighbor.CanMoveTo(tile))
            {
                neighbor.Selected = false;
                neighbor.Highlighted = true;                 
            } 
        }
    }

    void SetPropertiesForAllTiles(bool selected, bool highlighted, bool objective, bool resetOccupied)
    {
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                var tile = Tiles[row, col];
                tile.Selected = selected;
                tile.Highlighted = highlighted;
                tile.Objective = objective;
                if (resetOccupied)
                    tile.Occupied = false;
            }
        }
    }

    void CreateTiles()
    {
        Tiles = new Tile[Size, Size];

        var tileSpacing = (TileSize + TileSpacingFactor * TileSize);
        var offset = -(tileSpacing * 0.5f * (Size - 1));

        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                var newTile = Instantiate(TilePrefab).GetComponent<Tile>();
                newTile.name = "Tile_" + row + "_" + col;
                newTile.transform.SetParent(transform.FindChild(Names.Tiles));
                newTile.transform.localScale = new Vector3(TileSize, newTile.Height, TileSize);
                newTile.transform.position = new Vector3(col * tileSpacing + offset, 0.5f * newTile.transform.localScale.y, row * tileSpacing + offset);
                newTile.Row = row;
                newTile.Col = col;

                Tiles[row, col] = newTile;
            }
        }
    }

    void CreateEdges()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                var tile = Tiles[row, col];
                if (col < Border)
                    CreateEdge(tile, Tiles[row, col + 1]);
                if (row < Border)
                    CreateEdge(tile, Tiles[row + 1, col]);
            }
        }
    }

    void PlacePawns()
    {
        SetPropertiesForAllTiles(false, false, false, true);

        var col = Size / 2;
        for (var i = 0; i < Players.Count; i++)
        {
            var row = Mathf.Max(i * Size - 1, 0);
            Players[i].Pawn.Tile = Tiles[row, col];
            Players[i].Pawn.Tile.Occupied = true;
            Players[i].ObjectiveRow = Mathf.Max(Size - i * Size - 1, 0);
        }
    }

    void CreateEdge(Tile src, Tile dest)
    {
        foreach (Edge edge in Edges)
        {
            if (edge.Connects(src, dest))
                return;
        }

        Edge newEdge = GetNewEdge();
        newEdge.A = src;
        newEdge.B = dest;
        newEdge.Active = true;
        newEdge.Free = false;
        newEdge.gameObject.SetActive(ShowEdges);

        src.Edges.Add(newEdge);
        dest.Edges.Add(newEdge);
    }

    void RemoveEdge(Tile src, Tile dest)
    {
        foreach (var edge in Edges)
        {
            if (edge.Connects(src, dest))
            {
                RemoveEdge(edge);
                return;
            }
        }
    }

    void RemoveEdge(Edge edge)
    {
        edge.A.Edges.Remove(edge);
        edge.B.Edges.Remove(edge);
        Edges.Remove(edge);
        Destroy(edge.gameObject);
    }

    Edge GetNewEdge()
    {
        // If an available edge already exists in the pool
        foreach (var edge in Edges)
        {
            if (edge.Free)
                return edge;           
        }

        // Else create a new and add to the pool
        var newEdge = GameObject.Instantiate(EdgePrefab).GetComponent<Edge>();
        newEdge.transform.SetParent(EdgesTransform);
        newEdge.name = Names.Edge_;
        Edges.Add(newEdge);

        return newEdge;
    }

    Wall GetNewWall()
    {
        // If an available wall already exists in the pool
        foreach (var wall in Walls)
        {
            if (wall.Free)
                return wall;           
        }

        // Else create a new and add to the pool
        var newWall = GameObject.Instantiate(WallPrefab).GetComponent<Wall>();
        newWall.transform.SetParent(WallsTransform);
        newWall.transform.localScale = new Vector3(1.9f * TileSize + TileSpacingFactor * TileSize, 1, Mathf.Max(TileSpacingFactor * TileSize, MinWallWidth));
        newWall.name = Names.Wall_;
        Walls.Add(newWall);

        return newWall;        
    }

    void SetConnectionActive(Tile src, Tile dest, bool active)
    {
        foreach (var edge in Edges)
        {
            if (edge.Connects(src, dest))
            {
                edge.Active = active;
                return;
            }
        }
    }

    public int GetNextPlayer()
    {
        var next = CurrentPlayer + 1;
        next %= Players.Count;
        return next;
    }

    public int GetPreviousPlayer()
    {
        var previous = CurrentPlayer - 1;
        if (previous < 0)
            previous = Players.Count + previous;
        return previous;
    }

    public void OnAction()
    {
        if (MoveType == Move.Types.PlaceWall)
        {
            if (CreateWall(FocusedTile, _referenceWall.Horizontal))
                NextTurn();
        }
        else if (MoveType == Move.Types.MovePawn)
        {
            if (MovePawnTo(Players[CurrentPlayer].Pawn, FocusedTile))
                NextTurn();
        }
    }

    public bool PlayMove(Move move)
    {
        bool moveIsOk = false;

        if (move.GetType() == typeof(MovePawn))
        {
            var movePawn = move as MovePawn;
            moveIsOk = MovePawnTo(movePawn.Pawn, movePawn.Destination);
        }
        else if (move.GetType() == typeof(PlaceWall))
        {
            var placeWall = move as PlaceWall;
            moveIsOk = CreateWall(placeWall.Tile, placeWall.Horizontal);
        }

        if (moveIsOk)
            NextTurn();

        return moveIsOk;
    }

    bool IsBoardPosition(int row, int col)
    {
        return (row >= 0) && (row < Size) &&
        (col >= 0) && (col < Size);
    }

    bool RemoveWall(Tile tile, bool horizontal)
    {
        var tileEast = Tiles[tile.Row, tile.Col + 1];
        var tileSouth = Tiles[tile.Row - 1, tile.Col];
        var tileSoutheast = Tiles[tile.Row - 1, tile.Col + 1];

        if (horizontal)
        {
            SetConnectionActive(tile, tileSouth, true);
            SetConnectionActive(tileEast, tileSouth, true);
            SetConnectionActive(tileEast, tileSoutheast, true);
        }
        else
        {
            SetConnectionActive(tile, tileEast, true);
            SetConnectionActive(tile, tileSoutheast, true);
            SetConnectionActive(tileSouth, tileSoutheast, true);
        }

        for (var i = 0; i < Walls.Count; i++)
        {
            var wall = Walls[i];
            if (wall.Tile == tile)
            {
                Destroy(wall.gameObject);
                Walls.Remove(wall);
                return true;
            }
        }

        return false;
    }

    void MarkObjectiveRow(int player)
    {
        var objectiveRow = Players[player].ObjectiveRow;

        for (var col = 0; col < Size; col++)
        {
            Tiles[objectiveRow, col].Objective = true;
        }
    }

    void CreateTemporaryEdges(int currentPlayer)
    {
        for (var i = 0; i < Players.Count; i++)
        {
            if (i != currentPlayer)
            {
                CreateTemporaryEdges(Players[i].Pawn.Tile);
            }
        }
    }

    void CreateTemporaryEdges(Tile tile)
    {
        var neighbors = new List<Tile>();

        foreach (var edge in tile.Edges)
        {
            if (edge.Active)
                neighbors.Add(edge.GetNeighborOf(tile));
        }

        for (var i = 0; i < neighbors.Count - 1; i++)
        {
            var neighborA = neighbors[i];
            for (var j = i + 1; j < neighbors.Count; j++)
            {
                var neighborB = neighbors[j];
                if (!neighborA.IsNeighborOf(neighborB))
                    CreateEdge(neighborA, neighborB);
            }
        }
    }

    void RemoveTemporaryEdges(Tile tile)
    {
        for (var i = 0; i < tile.Edges.Count; i++)
        {
            var edge = tile.Edges[i];
            if (Tile.Distance(tile, edge.GetNeighborOf(tile)) > 1)
                RemoveEdge(edge);
        }
    }

    void RemoveTemporaryEdges()
    {
        for (var i = 0; i < Edges.Count; i++)
        {
            var edge = Edges[i];
            if (Tile.Distance(edge.A, edge.B) > 1)
            {
                RemoveEdge(edge);
                i--;
            }              
        }
    }

    bool CanPlaceWall(Tile tile, bool horizontal)
    {
        if (tile == null)
            return false;
        
        if (!IsBoardPosition(tile.Row - 1, tile.Col + 1))
            return false;
        
        if (horizontal)
        {
            foreach (var wall in Walls)
            {
                if (wall.Tile == tile)
                    return false;
                else if (wall.Tile.LeftTo(tile) && wall.Horizontal)
                    return false;
                else if (wall.Tile.RightTo(tile) && wall.Horizontal)
                    return false;
            }
        }
        else
        {
            foreach (var wall in Walls)
            {
                if (wall.Tile == tile)
                    return false;
                else if (wall.Tile.AboveTo(tile) && !wall.Horizontal)
                    return false;
                else if (wall.Tile.BelowTo(tile) && !wall.Horizontal)
                    return false;
            }           
        }

        return true;
    }

    bool CanBeTemporaryNeighbors(Tile a, Tile b, Tile center)
    {
//        if (Tile.SameRow(a, b))
//        {
//            var left = a.Leftside(b) ? a : b;
//            var right = a.Leftside(b) ? b : a;
//
//            while (!left.Equals(right))
//            {
//                if (left.HasWall && left.Wall.Vertical)
//                {
//                    return false;
//                }
//                
//                if (IsBoardPosition(left.Row + 1, left.Col))
//                {
//                    var above = Tiles[left.Row + 1, left.Col];
//                    if (above.HasWall && above.Wall.Vertical)
//                    {
//                        return false;
//                    }
//                }
//
//                left = Tiles[left.Row, left.Col + 1];
//            }
//
//            return true;
//        }
//        else if (Tile.SameCol(a, b))
//        {
//            var below = a.Below(b) ? a : b;
//            var above = a.Below(b) ? b : a;
//
//            while (!above.Equals(below))
//            {
//                if (above.HasWall && above.Wall.Horizontal)
//                {
//                    return false;
//                }
//
//                if (IsBoardPosition(above.Row, above.Col - 1))
//                {
//                    Tile left = Tiles[above.Row, above.Col - 1];
//                    if (left.HasWall && left.Wall.Horizontal)
//                    {
//                        return false;
//                    }
//                }
//
//                above = Tiles[above.Row - 1, above.Col];
//            }
//
//            return true;
//        }
//        else
//        {
//            Tile comp = a.Above(b) ? a : b;
//            Tile notComp = b.Equals(comp) ? a : b;
//
//            if (comp.Leftside(notComp))
//            {
//                if (comp.Above(center))
//                {
//                    if (comp.HasWall && (comp.Wall.Horizontal || comp.Wall.Vertical))
//                    {
//                        return false;
//                    }
//                    else if (center.HasWall && center.Wall.Vertical)
//                    {
//                        return false;
//                    }
//                    else if (IsBoardPosition(comp.Row, comp.Col - 1))
//                    {
//                        Tile left = Tiles[comp.Row, comp.Col - 1];
//                        if (left.HasWall && left.Wall.Horizontal)
//                        {
//                            return false;
//                        }
//                    }
//                }
//                else if (comp.Leftside(center))
//                {
//                    if (comp.HasWall && (comp.Wall.Horizontal || comp.Wall.Vertical))
//                    {
//                        return false;
//                    }
//                    else if (center.HasWall && center.Wall.Horizontal)
//                    {
//                        return false;
//                    }
//                    else if (IsBoardPosition(comp.Row + 1, comp.Col))
//                    {
//                        Tile above = Tiles[comp.Row + 1, comp.Col];
//                        if (above.HasWall && above.Wall.Vertical)
//                        {
//                            return false;
//                        }
//                    }
//                }
//                else
//                {
//                    return false;
//                }
//            }
//            else
//            { // comp rightside to notComp
//                if (comp.Above(center))
//                {
//                    if (comp.HasWall && comp.Wall.Horizontal)
//                    {
//                        return false;
//                    }
//                    else if (notComp.HasWall && notComp.Wall.Vertical)
//                    {
//                        return false;
//                    }
//                    else if (IsBoardPosition(comp.Row, comp.Col - 1))
//                    {
//                        Tile left = Tiles[comp.Row, comp.Col - 1];
//                        if (left.HasWall && (left.Wall.Vertical || left.Wall.Horizontal))
//                        {
//                            return false;
//                        }
//                    }
//                }
//                else if (comp.Rightside(center))
//                {
//                    if (center.HasWall && (center.Wall.Horizontal || center.Wall.Vertical))
//                    {
//                        return false;
//                    }
//                    else if (IsBoardPosition(comp.Row, comp.Col - 2))
//                    {
//                        Tile left = Tiles[comp.Row, comp.Col - 2];
//                        if (left.HasWall && left.Wall.Horizontal)
//                        {
//                            return false;
//                        }
//                    }
//                    else if (IsBoardPosition(comp.Row + 1, comp.Col - 1))
//                    {
//                        Tile left = Tiles[comp.Row + 1, comp.Col - 1];
//                        if (left.HasWall && left.Wall.Vertical)
//                        {
//                            return false;
//                        }
//                    }
//                }
//                else
//                {
//                    return false;
//                }
//            }
//
//            return true;
//        }
        return false;
    }

    public List<Move> GetPossibleMoves()
    {
        var moves = new List<Move>();
        var currentPlayerPawn = Players[CurrentPlayer].Pawn;

        // Move pawn
        foreach (var edge in currentPlayerPawn.Tile.Edges)
        {
            var neighbor = edge.GetNeighborOf(currentPlayerPawn.Tile);
            if (neighbor.CanMoveTo(currentPlayerPawn.Tile))
                moves.Add(new MovePawn(currentPlayerPawn, currentPlayerPawn.Tile, neighbor));
        }

        // Place horizontal walls

        // Place vertical walls

        return moves;
    }

    public void NextTurn()
    {
        _referenceWall.Tile = null;
        RemoveTemporaryEdges();
        SetPropertiesForAllTiles(false, false, false, false);

        CurrentPlayer = GetNextPlayer();
        CreateTemporaryEdges(CurrentPlayer);
        MarkObjectiveRow(CurrentPlayer);

        SelectTile(Players[CurrentPlayer].Pawn.Tile);
        FocusedTile = Players[CurrentPlayer].Pawn.Tile;
        MoveType = Move.Types.MovePawn;
    }

    public void PreviousTurn()
    {
        RemoveTemporaryEdges();

        CurrentPlayer = GetPreviousPlayer();

        for (var i = 0; i < Players.Count; i++)
        {
            if (i != CurrentPlayer)
            {
                CreateTemporaryEdges(Players[i].Pawn.Tile);
            }
        }
    }

    public bool MovePawnTo(Pawn pawn, Tile dest)
    {
        if (!dest.CanMoveTo(pawn.Tile))
            return false;

        var src = pawn.Tile;
        src.Occupied = false;
        dest.Occupied = true;
        pawn.Tile = dest;

        Moves.Push(new MovePawn(pawn, src, dest));
        return true;
    }

    public bool CreateWall(Tile tile, bool horizontal)
    {
        if (tile == null)
            return false;

        if (!CanPlaceWall(tile, horizontal))
            return false;

        var tileEast = Tiles[tile.Row, tile.Col + 1];
        var tileSouth = Tiles[tile.Row - 1, tile.Col];
        var tileSoutheast = Tiles[tile.Row - 1, tile.Col + 1];

        if (horizontal)
        {
            SetConnectionActive(tile, tileSouth, false);
            SetConnectionActive(tileEast, tileSouth, false);
            SetConnectionActive(tileEast, tileSoutheast, false);
        }
        else
        {
            SetConnectionActive(tile, tileEast, false);
            SetConnectionActive(tile, tileSoutheast, false);
            SetConnectionActive(tileSouth, tileSoutheast, false);
        }

        var wall = GetNewWall();
        wall.name = Names.Wall_;
        wall.transform.SetParent(WallsTransform);
        wall.Tile = tile;
        wall.Horizontal = horizontal;
        wall.Free = false;

        Moves.Push(new PlaceWall(tile, horizontal));
        return true;
    }

    public bool UndoMove()
    {
        if (Moves == null || Moves.Count == 0)
            return false;
        
        var lastMove = Moves.Peek();

        if (lastMove.GetType() == typeof(MovePawn))
        {
            var move = lastMove as MovePawn;
            if (!MovePawnTo(move.Pawn, move.Source))
                return false;
        }
        else if (lastMove.GetType() == typeof(PlaceWall))
        {
            var move = lastMove as PlaceWall;
            if (!RemoveWall(move.Tile, move.Horizontal))
                return false;
        }

        Moves.Pop();
        PreviousTurn();

        return true;
    }

    public bool IsGameOver()
    {
        foreach (var player in Players)
        {
            if (player.Pawn.Tile.Row == player.ObjectiveRow)
                return true;
        }

        return false;
    }
}
