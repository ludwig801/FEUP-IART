using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    // Singleton
    public static GameBoard Instance { get { return _instance; } }

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
    public int StartingPlayer, CurrentPlayer, Winner;
    [Range(0, 10)]
    public int MaxWallsPerPlayer;
    [Range(50, 500)]
    public int MaxMoves;
    public int MoveCount;
    public Stack<Move> Moves;
    public Move.Types MoveType;
    public Tile FocusedTile;
    public bool ShowEdges, Ongoing, IsGameOver;
    [HideInInspector]
    public List<Edge> Edges;
    [HideInInspector]
    public List<Wall> Walls;
    [HideInInspector]
    public List<Player> Players;
    public Tile[,] Tiles;

    public int Border { get { return Size - 1; } }

    Transform _referenceFocused;
    Wall _referenceWall;
    Quaternion _rotateCameraPivotTo;
    float _waitForNextPlay;

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
        if (Ongoing)
        {
            IsGameOver = CheckGameOver();
            if (!IsGameOver && Players[CurrentPlayer].IsCpu)
            {
                if (!Minimax.IsRunning())
                {
                    if (Minimax.IsFinished())
                    {
                        //Debug.Log("Detected algorithm finished!");
                        Move best = Minimax.GetResult();
                        if (!PlayMove(best))
                        {
//                            Debug.Log("Could not play best move: " + best);
//                            Pause();
                        }
//                        else
//                        {
//                            Debug.Log("Played best move: " + best);
//                            Debug.Log("Value for this move: " + Minimax.CalculateHeuristicValue(GetPreviousPlayer(CurrentPlayer)));
//                        }
                    }
                    else
                    {
                        //Debug.Log("Trying to get algorithm to run...");
                        //StartCoroutine(Minimax.RunAlgorithmLinear(this));
                        Minimax.RunAlgorithm(this);
                    }
                }
            }
        }

        HandleKeysInput();

        UpdateVisualElements();
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

        if (!Ongoing || CheckGameOver())
            return;

        if (Players[CurrentPlayer].IsCpu)
            return;

        if (Input.GetKeyUp(KeyCode.Tab))
            ChangeToNextMode();

        if (Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Z))
        {
            Debug.Log("Undo");
            UndoMove();           
        }

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

        _referenceFocused.gameObject.SetActive(FocusedTile != null && !Players[CurrentPlayer].IsCpu);
        if (FocusedTile != null)
        {
            _referenceFocused.position = Vector3.Lerp(_referenceFocused.position, FocusedTile.transform.position + new Vector3(0, 0.5f, 0), Time.deltaTime * 8f);
            _referenceWall.Invalid = !CanPlaceWall(FocusedTile, _referenceWall.Horizontal);
            _referenceWall.Tile = (MoveType == Move.Types.PlaceWall) ? FocusedTile : null;
        }
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

    void RemoveAllWalls()
    {
        for (var i = Walls.Count - 1; i >= 0; i--)
        {
            var wall = Walls[i];
            RemoveWall(wall.Tile, wall.Horizontal);
        }
    }

    void MarkObjectiveRow(int player)
    {
        var objectiveRow = Players[player].ObjectiveRow;

        for (var col = 0; col < Size; col++)
        {
            Tiles[objectiveRow, col].Objective = true;
        }
    }

    void CreateTemporaryEdgesForOtherPlayers(int currentPlayer)
    {
        for (var i = 0; i < Players.Count; i++)
        {
            if (i != currentPlayer)
            {
                CreateTemporaryEdgesForOtherPlayers(Players[i].Pawn.Tile);
            }
        }
    }

    void CreateTemporaryEdgesForOtherPlayers(Tile tile)
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

        CurrentPlayer = StartingPlayer - 1;
        NextTurn();

        foreach (var player in Players)
        {
            player.Walls = 0;
        }
            
        RemoveAllWalls();

        FocusedTile = Players[CurrentPlayer].Pawn.Tile;

        Minimax.SetWeights();

        MoveCount = 0;
        IsGameOver = false;
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

    public Queue<Move> GetCurrentPossibleMoves()
    {
        var moves = new Queue<Move>();
        var currentPlayerPawn = Players[CurrentPlayer].Pawn;

        // Move pawn
        foreach (var edge in currentPlayerPawn.Tile.Edges)
        {
            var neighbor = edge.GetNeighborOf(currentPlayerPawn.Tile);
            if (neighbor.CanMoveTo(currentPlayerPawn.Tile))
                moves.Enqueue(new MovePawn(currentPlayerPawn, currentPlayerPawn.Tile, neighbor));
        }

        var hasWalls = MaxWallsPerPlayer - Players[CurrentPlayer].Walls > 0;
        if (hasWalls)
        {
            // Place horizontal walls
            for (var row = 0; row < Size; row++)
            {
                for (var col = 0; col < Size; col++)
                {
                    var tile = Tiles[row, col];

                    if (CanPlaceWall(tile, true))
                        moves.Enqueue(new PlaceWall(tile, true));
                }
            }

            // Place vertical walls
            for (var row = 0; row < Size; row++)
            {
                for (var col = 0; col < Size; col++)
                {
                    var tile = Tiles[row, col];

                    if (CanPlaceWall(tile, false))
                        moves.Enqueue(new PlaceWall(tile, false));
                }
            }        
        }

        return moves;
    }

    public int GetNextPlayer(int player)
    {
        var next = player + 1;
        next %= Players.Count;
        return next;
    }

    public int GetPreviousPlayer(int player)
    {
        var previous = player - 1;
        if (previous < 0)
            previous = Players.Count + previous;
        return previous;
    }

    public void OnAction()
    {
        switch (MoveType)
        {
            case Move.Types.MovePawn:
                PlayMove(new MovePawn(Players[CurrentPlayer].Pawn, Players[CurrentPlayer].Pawn.Tile, FocusedTile));
                break;

            case Move.Types.PlaceWall:
                PlayMove(new PlaceWall(FocusedTile, _referenceWall.Horizontal));
                break;
        }
    }

    public bool PlayMove(Move move)
    {
        if (move == null)
        {
            Debug.LogWarning("Move to play is null!");
            return false;
        }

        if (move.GetType() == typeof(MovePawn))
        {
            var movePawn = move as MovePawn;
            if (movePawn.Destination.CanMoveTo(movePawn.Source))
                MovePawnTo(movePawn.Pawn, movePawn.Destination);
            else
                return false;
        }
        else if (move.GetType() == typeof(PlaceWall))
        {
            var placeWall = move as PlaceWall;
            if (CanPlaceWall(placeWall.Tile, placeWall.Horizontal))
            {
                PlaceWall(placeWall.Tile, placeWall.Horizontal);
                Players[CurrentPlayer].Walls++;          
            }
            else
                return false;
        }

        Moves.Push(move);
        NextTurn();
        MoveCount++;         

        return true;
    }

    public bool UndoMove()
    {
        if (Moves == null || Moves.Count == 0)
        {
            Debug.Log("No moves accounted for...");
            return false;
        }

        var lastMove = Moves.Peek();

        if (lastMove.GetType() == typeof(MovePawn))
        {
            var move = lastMove as MovePawn;
            MovePawnTo(move.Pawn, move.Source);
        }
        else if (lastMove.GetType() == typeof(PlaceWall))
        {
            var move = lastMove as PlaceWall;
            RemoveWall(move.Tile, move.Horizontal);
            Players[GetPreviousPlayer(CurrentPlayer)].Walls--;
        }
            
        Moves.Pop();
        PreviousTurn();
        MoveCount--;

        return true;
    }

    public void MovePawnTo(Pawn pawn, Tile dest)
    {
        var src = pawn.Tile;
        src.Occupied = false;
        dest.Occupied = true;
        pawn.Tile = dest;
    }

    public void PlaceWall(Tile tile, bool horizontal)
    {
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
    }

    public void NextTurn()
    {
        _referenceWall.Tile = null;
        RemoveTemporaryEdges();
        SetPropertiesForAllTiles(false, false, false, false);

        CurrentPlayer = GetNextPlayer(CurrentPlayer);
        CreateTemporaryEdgesForOtherPlayers(CurrentPlayer);
        MarkObjectiveRow(CurrentPlayer);

        SelectTile(Players[CurrentPlayer].Pawn.Tile);
        FocusedTile = Players[CurrentPlayer].Pawn.Tile;
        MoveType = Move.Types.MovePawn;
    }

    public void PreviousTurn()
    {
        _referenceWall.Tile = null;
        RemoveTemporaryEdges();
        SetPropertiesForAllTiles(false, false, false, false);

        CurrentPlayer = GetPreviousPlayer(CurrentPlayer);
        CreateTemporaryEdgesForOtherPlayers(CurrentPlayer);

        SelectTile(Players[CurrentPlayer].Pawn.Tile);
        FocusedTile = Players[CurrentPlayer].Pawn.Tile;
        MoveType = Move.Types.MovePawn;

        MarkObjectiveRow(CurrentPlayer);
    }

    public bool CheckGameOver()
    {
        for (var i = 0; i < Players.Count; i++)
        {
            var player = Players[i];
            if (player.Pawn.Tile.Row == player.ObjectiveRow)
            {
                Winner = i;
                return true; 
            }
        }

        Winner = -1;

        if (MoveCount >= MaxMoves)
            return true;

        return false;
    }
}
