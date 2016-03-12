using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Minimax))]
[RequireComponent(typeof(AStar))]
public class GameBoard : MonoBehaviour
{
    // Singleton
    public static GameBoard Instance { get { return _instance; } }
    static GameBoard _instance;

    // Private variables
    [SerializeField] Transform TilesTransform;
    [SerializeField] Transform EdgesTransform;
    [SerializeField] Transform VisualBoard;
    [SerializeField] Transform WallsTransform;
    [SerializeField] GameObject TilePrefab;
    [SerializeField] GameObject EdgePrefab;
    [SerializeField] GameObject WallPrefab;
    [SerializeField] GameObject FocusPrefab;
    [SerializeField] CameraManager Camera;
    [SerializeField] List<Player> _players;
    [Range(9, 9)]
    [SerializeField] int _size = 9;
    [Range(1, 5)]
    [SerializeField] int _tileSize;
    [Range(0, 1)]
    [SerializeField] float _tileSpacingFactor;
    [Range(0.25f, 1.25f)]
    [SerializeField] float _minWallWidth;
    [Range(0, 10)]
    [SerializeField] int _numWallsPerPlayer;
    [Range(50, 500)]
    [SerializeField] int _maxMovesPerGame;
    [Range(0, 2)]
    [SerializeField] int _cpuTurnWait = 1;
    [Range(1, 10)]
    [SerializeField] int _boardScaleMultiplier = 5;
    Minimax _minimax;
    AStar _aStar;
    Transform _referenceFocused;
    Wall _referenceWall;
    Quaternion _rotateCameraPivotTo;
    Stack<Move> _moves;
    List<Edge> _edges;
    List<Wall> _walls;
    Tile[,] _tiles;
    Tile _focusedTile;
    float _waitForNextPlay;
    public Move.Types CurrentMoveType;

    // Public variables
    [Range(-1, 1)]
    public int StartingPlayer;
    public bool ShowEdges, Ongoing, IsGameOver;

    // Properties
    public int PlayersCount
    {
        get
        {
            return _players.Count;
        }
    }

    public int Size
    {
        get
        {
            return _size;
        }
    }

    public int Border
    {
        get
        {
            return Size - 1;
        }
    }

    public int TileSize
    {
        get
        {
            return _tileSize;
        }
    }

    public float TileSpacing
    {
        get
        {
            return _tileSpacingFactor;
        }
    }

    public int Winner
    {
        get;

        private set;
    }

    public Minimax Minimax
    {
        get
        {
            return _minimax;
        }
    }

    public int MoveCount
    {
        get;
        private set;
    }

    public int CurrentPlayerIndex
    {
        get;
        private set;
    }

    public Player CurrentPlayer
    {
        get
        {
            return _players[CurrentPlayerIndex];
        }
    }

    public bool IsCurrentPlayerCPU
    {
        get
        {
            return _players[CurrentPlayerIndex].IsCpu;
        }
    }

    // Methods
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        _minimax = GetComponent<Minimax>();
        _aStar = GetComponent<AStar>();

        _moves = new Stack<Move>();
        _edges = new List<Edge>();
        _walls = new List<Wall>();

        _referenceWall = GetNewWall();
        _walls.Remove(_referenceWall);
        _referenceWall.Free = false;
        _referenceWall.name = "Reference Wall";
        _referenceWall.Tile = null;
        _referenceWall.gameObject.SetActive(false);

        CreateTiles();
        CreateEdges();

        _referenceFocused = Instantiate(FocusPrefab).transform;
        _referenceFocused.SetParent(transform);
        _referenceFocused.name = "Reference Focus Tile";
        _referenceFocused.localScale = new Vector3(0.75f * _tileSize, _referenceFocused.localScale.y, 0.75f * _tileSize);
        _referenceFocused.transform.position = new Vector3(0, TilePrefab.transform.localScale.y * 0.5f, 0);

        StartCoroutine(UpdateEdges());

        Ongoing = false;
    }

    void Update()
    {
        if (Ongoing)
        {
            IsGameOver = GameOver();
            if (!IsGameOver && IsCurrentPlayerCPU)
            {
                _waitForNextPlay += Time.deltaTime;
                if (_waitForNextPlay >= _cpuTurnWait && !_minimax.IsRunning())
                {
                    _waitForNextPlay = 0;

                    if (_minimax.IsFinished())
                    {
                        PlayMove(_minimax.GetResult());
                    }
                    else
                    {
                        _minimax.RunAlgorithm(this);
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
            foreach (var edge in _edges)
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

        if (!Ongoing || GameOver())
            return;

        if (IsCurrentPlayerCPU)
            return;

        if (Input.GetKeyUp(KeyCode.Tab))
            ChangeToNextMode();

        if (Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Z))
            UndoMove();

        if (Input.GetKeyUp(KeyCode.Space))
            OnAction();

        if (Input.GetKeyDown(KeyCode.Q))
            ChangeWallOrientation();

        if (Input.GetKeyDown(KeyCode.A))
            ChangeFocusedTile(-1, 0);

        if (Input.GetKeyDown(KeyCode.D))
            ChangeFocusedTile(+1, 0);

        if (Input.GetKeyDown(KeyCode.S))
            ChangeFocusedTile(0, -1);

        if (Input.GetKeyDown(KeyCode.W))
            ChangeFocusedTile(0, +1);
    }

    void UpdateVisualElements()
    {
        _referenceFocused.gameObject.SetActive(!IsGameOver && _focusedTile != null && !IsCurrentPlayerCPU);
        if (_focusedTile != null)
        {
            var newPosition = new Vector3(_focusedTile.transform.position.x, _referenceFocused.position.y, _focusedTile.transform.position.z);
            _referenceFocused.position = Vector3.Lerp(_referenceFocused.position, newPosition, Time.deltaTime * 8f);
            _referenceWall.gameObject.SetActive(CurrentMoveType == Move.Types.PlaceWall);
            _referenceWall.Invalid = !CanPlaceWall(_focusedTile, _referenceWall.Horizontal);
            _referenceWall.Tile = (CurrentMoveType == Move.Types.PlaceWall) ? _focusedTile : null;
        }

        var visualBoardScale = _size * (_tileSize + _tileSpacingFactor) + _tileSize * _boardScaleMultiplier;
        VisualBoard.localScale = new Vector3(visualBoardScale, visualBoardScale, 1);
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
        for (var row = 0; row < _size; row++)
        {
            for (var col = 0; col < _size; col++)
            {
                var tile = _tiles[row, col];
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
        _tiles = new Tile[_size, _size];

        var tileSpacing = (_tileSize + _tileSpacingFactor * _tileSize);
        var offset = -(tileSpacing * 0.5f * (_size - 1));

        for (var row = 0; row < _size; row++)
        {
            for (var col = 0; col < _size; col++)
            {
                var newTile = Instantiate(TilePrefab).GetComponent<Tile>();
                newTile.name = "Tile_" + row + "_" + col;
                newTile.transform.SetParent(transform.FindChild(Names.Tiles));
                newTile.transform.localScale = new Vector3(_tileSize, newTile.Height, _tileSize);
                newTile.transform.position = new Vector3(col * tileSpacing + offset, 0.5f * newTile.transform.localScale.y, row * tileSpacing + offset);
                newTile.Row = row;
                newTile.Col = col;

                _tiles[row, col] = newTile;
            }
        }
    }

    void CreateEdges()
    {
        for (int row = 0; row < _size; row++)
        {
            for (int col = 0; col < _size; col++)
            {
                var tile = _tiles[row, col];
                if (col < Border)
                    CreateEdge(tile, _tiles[row, col + 1]);
                if (row < Border)
                    CreateEdge(tile, _tiles[row + 1, col]);
            }
        }
    }

    void PlacePawns()
    {
        SetPropertiesForAllTiles(false, false, false, true);

        var col = _size / 2;
        for (var i = 0; i < _players.Count; i++)
        {
            var row = Mathf.Max(i * _size - 1, 0);
            _players[i].Pawn.Tile = _tiles[row, col];
            _players[i].Pawn.Tile.Occupied = true;
            _players[i].ObjectiveRow = Mathf.Max(_size - i * _size - 1, 0);
        }
    }

    void CreateEdge(Tile src, Tile dest)
    {
        foreach (Edge edge in _edges)
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
        foreach (var edge in _edges)
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
        _edges.Remove(edge);
        Destroy(edge.gameObject);
    }

    Edge GetNewEdge()
    {
        // If an available edge already exists in the pool
        foreach (var edge in _edges)
        {
            if (edge.Free)
                return edge;           
        }

        // Else create a new and add to the pool
        var newEdge = GameObject.Instantiate(EdgePrefab).GetComponent<Edge>();
        newEdge.transform.SetParent(EdgesTransform);
        newEdge.name = Names.Edge_;
        _edges.Add(newEdge);

        return newEdge;
    }

    Wall GetNewWall()
    {
        // If an available wall already exists in the pool
        foreach (var wall in _walls)
        {
            if (wall.Free)
                return wall;           
        }

        // Else create a new and add to the pool
        var newWall = GameObject.Instantiate(WallPrefab).GetComponent<Wall>();
        newWall.transform.SetParent(WallsTransform);
        newWall.transform.localScale = new Vector3(1.9f * _tileSize + _tileSpacingFactor * _tileSize, 1, Mathf.Max(_tileSpacingFactor * _tileSize, _minWallWidth));
        newWall.name = Names.Wall_;
        _walls.Add(newWall);

        return newWall;        
    }

    void SetConnectionActive(Tile src, Tile dest, bool active)
    {
        foreach (var edge in _edges)
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
        return (row >= 0) && (row < _size) &&
        (col >= 0) && (col < _size);
    }

    bool RemoveWall(Tile tile, bool horizontal)
    {
        var tileEast = _tiles[tile.Row, tile.Col + 1];
        var tileSouth = _tiles[tile.Row - 1, tile.Col];
        var tileSoutheast = _tiles[tile.Row - 1, tile.Col + 1];

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

        for (var i = 0; i < _walls.Count; i++)
        {
            var wall = _walls[i];
            if (wall.Tile == tile)
            {
                Destroy(wall.gameObject);
                _walls.Remove(wall);
                return true;
            }
        }

        return false;
    }

    void RemoveAllWalls()
    {
        for (var i = _walls.Count - 1; i >= 0; i--)
        {
            var wall = _walls[i];
            RemoveWall(wall.Tile, wall.Horizontal);
        }
    }

    void MarkObjectiveRow(int player)
    {
        var objectiveRow = _players[player].ObjectiveRow;

        for (var col = 0; col < _size; col++)
        {
            _tiles[objectiveRow, col].Objective = true;
        }
    }

    void CreateTemporaryEdgesForOtherPlayers(int currentPlayer)
    {
        for (var i = 0; i < _players.Count; i++)
        {
            if (i != currentPlayer)
            {
                CreateTemporaryEdgesForOtherPlayers(_players[i].Pawn.Tile);
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
        for (var i = 0; i < _edges.Count; i++)
        {
            var edge = _edges[i];
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

        if (_players[CurrentPlayerIndex].Walls <= 0)
            return false;
        
        if (horizontal)
        {
            foreach (var wall in _walls)
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
            foreach (var wall in _walls)
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

        CurrentPlayerIndex = StartingPlayer - 1;
        NextTurn();

        foreach (var player in _players)
        {
            player.Walls = _numWallsPerPlayer;
        }
            
        RemoveAllWalls();

        _focusedTile = _players[CurrentPlayerIndex].Pawn.Tile;

        _minimax.SetWeights();

        MoveCount = 0;
        IsGameOver = false;
    }

    public void ChangeFocusedTile(int horizontalInput, int verticalInput)
    {
        var rotatedHorizontalInput = (Camera.EulerHorizontal < 45 || Camera.EulerHorizontal >= 315) ? horizontalInput :
            Camera.EulerHorizontal < 135 ? verticalInput : Camera.EulerHorizontal < 225 ? -horizontalInput : -verticalInput;

        var rotatedVerticalInput = (Camera.EulerHorizontal < 45 || Camera.EulerHorizontal >= 315) ? verticalInput :
            Camera.EulerHorizontal < 135 ? -horizontalInput : Camera.EulerHorizontal < 225 ? -verticalInput : horizontalInput;

        if (IsBoardPosition(_focusedTile.Row + rotatedVerticalInput, _focusedTile.Col + rotatedHorizontalInput))
        {
            _focusedTile = _tiles[_focusedTile.Row + rotatedVerticalInput, _focusedTile.Col + rotatedHorizontalInput]; 
        } 
    }

    public void ChangeWallOrientation()
    {
        _referenceWall.Horizontal = !_referenceWall.Horizontal;
    }

    public void ChangeToNextMode()
    {
        CurrentMoveType = Move.GetNextType(CurrentMoveType);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public Queue<Move> GetCurrenPlayerMoves()
    {
        var moves = new Queue<Move>();
        var currentPlayerPawn = _players[CurrentPlayerIndex].Pawn;

        // Move pawn
        foreach (var edge in currentPlayerPawn.Tile.Edges)
        {
            var neighbor = edge.GetNeighborOf(currentPlayerPawn.Tile);
            if (neighbor.CanMoveTo(currentPlayerPawn.Tile))
                moves.Enqueue(new MovePawn(currentPlayerPawn, currentPlayerPawn.Tile, neighbor));
        }

        var hasWalls = (_players[CurrentPlayerIndex].Walls > 0);
        if (hasWalls)
        {
            // Place horizontal walls
            for (var row = 0; row < _size; row++)
            {
                for (var col = 0; col < _size; col++)
                {
                    var tile = _tiles[row, col];

                    if (CanPlaceWall(tile, true))
                        moves.Enqueue(new PlaceWall(tile, true));
                }
            }

            // Place vertical walls
            for (var row = 0; row < _size; row++)
            {
                for (var col = 0; col < _size; col++)
                {
                    var tile = _tiles[row, col];

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
        next %= _players.Count;
        return next;
    }

    public int GetPreviousPlayer(int player)
    {
        var previous = player - 1;
        if (previous < 0)
            previous = _players.Count + previous;
        return previous;
    }

    public void OnAction()
    {
        switch (CurrentMoveType)
        {
            case Move.Types.MovePawn:
                PlayMove(new MovePawn(_players[CurrentPlayerIndex].Pawn, _players[CurrentPlayerIndex].Pawn.Tile, _focusedTile));
                break;

            case Move.Types.PlaceWall:
                PlayMove(new PlaceWall(_focusedTile, _referenceWall.Horizontal));
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
                _players[CurrentPlayerIndex].Walls--;          
            }
            else
                return false;
        }

        _moves.Push(move);
        NextTurn();
        MoveCount++;       

        if (!IsBoardValid())
        {
            UndoMove();
            return false;
        }

        return true;
    }

    public bool UndoMove()
    {
        if (_moves == null || _moves.Count == 0)
        {
            Debug.Log("No moves accounted for...");
            return false;
        }

        var lastMove = _moves.Peek();

        if (lastMove.GetType() == typeof(MovePawn))
        {
            var move = lastMove as MovePawn;
            MovePawnTo(move.Pawn, move.Source);
        }
        else if (lastMove.GetType() == typeof(PlaceWall))
        {
            var move = lastMove as PlaceWall;
            RemoveWall(move.Tile, move.Horizontal);
            _players[GetPreviousPlayer(CurrentPlayerIndex)].Walls++;
        }
            
        _moves.Pop();
        PreviousTurn();
        MoveCount--;

        return true;
    }

    bool IsBoardValid()
    {
        return _aStar.CalculateDistancesToObjective();
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
        var tileEast = _tiles[tile.Row, tile.Col + 1];
        var tileSouth = _tiles[tile.Row - 1, tile.Col];
        var tileSoutheast = _tiles[tile.Row - 1, tile.Col + 1];

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
        ChangeTurn(true);
    }

    public void PreviousTurn()
    {
        ChangeTurn(false);
    }

    void ChangeTurn(bool forward)
    {
        _referenceWall.Tile = null;
        RemoveTemporaryEdges();
        SetPropertiesForAllTiles(false, false, false, false);

        CurrentPlayerIndex = forward ? GetNextPlayer(CurrentPlayerIndex) : GetPreviousPlayer(CurrentPlayerIndex);

        if (!GameOver())
        {
            CreateTemporaryEdgesForOtherPlayers(CurrentPlayerIndex);
            MarkObjectiveRow(CurrentPlayerIndex);
            SelectTile(CurrentPlayer.Pawn.Tile);
            _focusedTile = CurrentPlayer.Pawn.Tile;
            CurrentMoveType = Move.Types.MovePawn; 
        }
    }

    public bool GameOver()
    {
        for (var i = 0; i < _players.Count; i++)
        {
            var player = _players[i];
            if (player.Pawn.Tile.Row == player.ObjectiveRow)
            {
                Winner = i;
                return true; 
            }
        }

        Winner = -1;

        if (MoveCount >= _maxMovesPerGame)
            return true;

        return false;
    }

    public Player GetPlayer(int index)
    {
        return _players[index];
    }
}
