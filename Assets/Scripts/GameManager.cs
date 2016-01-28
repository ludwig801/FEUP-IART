using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(Minimax))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        { 
            return _instance;
        }
    }

    public enum GameState
    {
        Error = -1,
        Over = 0,
        Draw = 1,
        Ongoing = 2,
        Stopped = 3,
        Paused = 4
    }

    public enum GameMode
    {
        None = 0,
        MovePawn = 1,
        PlaceWallH = 2,
        PlaceWallV = 3
    }

    public int NumPlayers = 2;
    public int BoardSize = 9;
    public Transform wallPrefab;
    public Transform boardPrefab;
    public Transform pawnPrefab;
    public bool CPU_0;
    public bool CPU_1;
    public bool testMode;
    public GameState CurrentGameState;
    public GameMode CurrentGameMode;
    public int numWallsPerPlayer;
    public int minimaxDepth;
    public int numGamesPerPlayer;
    public int MaxMoveCount;
    public int CurrentPlayer;
    public int Winner;
    public int[] _availableWalls;
    public Tile selectedTile = null;
    public Pawn[] Pawns;
    public Wall[,] Walls;
    public GameBoard GameBoard;

    public Minimax Minimax
    {
        get
        {
            if (_minimax == null)
            {
                _minimax = GetComponent<Minimax>();
            }
            return _minimax;
        }
    }

    static GameManager _instance;

    Minimax _minimax;
    Stack<Move> _moveHistory;
    int _moveCount;

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        GameBoard = Instantiate(boardPrefab).GetComponent<GameBoard>();
        GameBoard.name = Names.Board;
        GameBoard.Init();

        Pawns = new Pawn[NumPlayers];
        Walls = new Wall[NumPlayers, numWallsPerPlayer];
        for (var playerIndex = 0; playerIndex < NumPlayers; playerIndex++)
        {
            var _pawnsParentTransform = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + playerIndex);
            var _wallsParentTransform = _pawnsParentTransform.FindChild(Names.Walls);

            Pawns[playerIndex] = Instantiate(pawnPrefab).GetComponent<Pawn>();
            Pawns[playerIndex].name = Names.Pawn_ + playerIndex;
            Pawns[playerIndex].transform.parent = _pawnsParentTransform;
            Pawns[playerIndex].player = playerIndex;

            for (var wallIndex = 0; wallIndex < numWallsPerPlayer; wallIndex++)
            {
                Walls[playerIndex, wallIndex] = Instantiate(wallPrefab).GetComponent<Wall>();
                Walls[playerIndex, wallIndex].name = Names.Wall_ + wallIndex;
                Walls[playerIndex, wallIndex].transform.parent = _wallsParentTransform;
                Walls[playerIndex, wallIndex].Init();
                Walls[playerIndex, wallIndex].player = playerIndex;
            }
        }

        _moveHistory = new Stack<Move>();
        _availableWalls = new int[NumPlayers];
        _moveCount = 0;

        CurrentGameState = GameState.Stopped;
    }

    void Update()
    {
        if (CurrentGameState == GameState.Ongoing)
        {
            if (IsCPU(CurrentPlayer))
            {
                if (!Minimax.RunAlgorithm() || !PlayMove(_moveHistory, Minimax.BestMove, CurrentPlayer))
                {
                    CurrentGameState = GameState.Error;
                    return;
                }

                if (IsFinalState(CurrentPlayer))
                {
                    CurrentGameState = GameState.Over;
                    Winner = CurrentPlayer;
                }
                else
                {
                    _moveCount++;
                    if (CPU_0 && CPU_1 && (_moveCount >= MaxMoveCount))
                    {
                        CurrentGameState = GameState.Draw;
                        Winner = -1;
                    }
                    NextTurn();
                }
            }
        }
    }

    public void NewGame(int initialPlayer)
    {
        GameBoard.Reset();

        for (var playerIndex = 0; playerIndex < NumPlayers; playerIndex++)
        {
            if (Pawns[playerIndex].HasTile)
            {
                GameBoard.RemoveTileTempLinks(Pawns[playerIndex].Tile);
                Pawns[playerIndex].Tile.RemovePawn();
            }

            Pawns[playerIndex].Tile = GetInitialTile(playerIndex);

            for (var wallIndex = 0; wallIndex < numWallsPerPlayer; wallIndex++)
            {
                if (Walls[playerIndex, wallIndex].HasTile)
                {
                    Walls[playerIndex, wallIndex].Tile.RemoveWall();
                    Walls[playerIndex, wallIndex].Tile = null;
                }
            }

            _availableWalls[playerIndex] = numWallsPerPlayer;
        }

        CurrentPlayer = initialPlayer;
        GameBoard.CreateTileTempLinks(Pawns[GetNextPlayer(CurrentPlayer)].Tile);

        if (selectedTile != null)
        {
            OnTileDeselected(selectedTile);
        }

        _moveHistory.Clear();

        CurrentGameMode = GameMode.None;
        CurrentGameState = GameState.Ongoing;
    }

    public Tile GetInitialTile(int playerIndex)
    {
        return playerIndex == 0 ? GameBoard.GetTileAt(0, GameBoard.Size / 2) : GameBoard.GetTileAt(GameBoard.Border, GameBoard.Size / 2);
    }

    public void UndoMove()
    {
        if (_moveHistory != null && _moveHistory.Count > 0)
        {
            CurrentPlayer = GetPreviousPlayer(CurrentPlayer);

            UndoMove(_moveHistory.Peek(), CurrentPlayer);
        }
    }

    public bool PlayMove(Stack<Move> history, Move move, int player)
    {
        GameBoard.RemoveTileTempLinks(Pawns[GetNextPlayer(player)].Tile);
        GameBoard.RemoveTileTempLinks(Pawns[player].Tile);

        switch (move.Type)
        {
            case Move.MovePawn:
                history.Push(new Move(Pawns[player].Tile.row, Pawns[player].Tile.col));
                GameBoard.MovePawnTo(Pawns[player], move.Row, move.Col);
                break;

            case Move.SetWall:
                Wall wall = GetWall(player);
                if (wall == null)
                {
                    return false;
                }
                history.Push(new Move(move));
                GameBoard.PutWallAt(wall, move.Row, move.Col, move.IsHorizontal);
                _availableWalls[player]--;
                break;

            default:
                return false;
        }

        GameBoard.CreateTileTempLinks(Pawns[player].Tile);
        return true;
    }

    public bool UndoMove(Move move, int player)
    {
        GameBoard.RemoveTileTempLinks(Pawns[player].Tile);
        GameBoard.RemoveTileTempLinks(Pawns[GetNextPlayer(player)].Tile);

        switch (move.Type)
        {
            case Move.MovePawn:
                GameBoard.MovePawnTo(Pawns[player], move.Row, move.Col);
                break;

            case Move.SetWall:
                GameBoard.RemoveWall(move.Row, move.Col);
                _availableWalls[player]++;
                break;

            default:
                return false;
        }

        GameBoard.CreateTileTempLinks(Pawns[GetNextPlayer(player)].Tile);
        return true;
    }

    public Wall GetWall(int player)
    {
        for (int j = 0; j < numWallsPerPlayer; j++)
        {
            Wall wall = Walls[player, j];
            if (wall.Free)
            {
                return wall;
            }
        }

        return null;
    }

    public bool IsCPU(int player)
    {
        if (player == 0)
        {
            return CPU_0;
        }
        else
        {
            return CPU_1;
        }
    }

    public int GetNextPlayer(int player)
    {
        player++;
        player %= NumPlayers;
        return player;
    }

    public Pawn GetPlayerPawn(int player)
    {
        return Pawns[player];
    }

    public Tile GetPlayerTile(int player)
    {
        return Pawns[player].Tile;
    }

    public int GetPreviousPlayer(int player)
    {
        player--;
        if (player < 0)
        {
            player = NumPlayers + player;
        }
        return player;
    }

    public bool CanPlaceWall(int player, Tile tile, bool horizontal)
    {
        if (tile.row == 0 || tile.col == GameBoard.Border || tile.HasWall)
            return false;

        if (horizontal)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                if (GameBoard.IsBoardPosition(tile.row, tile.col + i))
                {
                    Tile b = GameBoard.GetTileAt(tile.row, tile.col + i);
                    if (b.HasWall && b.Wall.Horizontal)
                    {
                        return false;
                    }
                }
            }
        }
        else if (!horizontal)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                if (GameBoard.IsBoardPosition(tile.row + i, tile.col))
                {
                    Tile b = GameBoard.GetTileAt(tile.row + i, tile.col);
                    if (b.HasWall && b.Wall.Vertical)
                        return false;
                }
            }
        }
        else
        {
            return false; 
        }    

        return true;
    }

    public bool IsFinalState(int player)
    {
        switch (player)
        {
            case 0:
                if (Pawns[player].Tile.row == GameBoard.Border)
                {
                    return true;
                }
                break;
            case 1:
                if (Pawns[player].Tile.row == 0)
                {
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    public bool IsFinalState()
    {
        return (IsFinalState(0) || IsFinalState(1));
    }

    public List<Move> GetPossibleMoves(int player)
    {
        var moves = new List<Move>();
        var playerTile = Pawns[player].Tile;

        // pawn movements
        for (int i = 0; i < Pawns[player].Tile.neighbors.Count; i++)
        {
            Tile tile = playerTile.neighbors[i];
            if (!tile.HasPawn)
            {
                moves.Add(new Move(tile.row, tile.col));
            }
        }

        if (GetWall(player) != null)
        {
            // horizontal wall placements
            for (int i = 0; i < GameBoard.Size; i++)
            {
                for (int j = 0; j < GameBoard.Size; j++)
                {
                    if (CanPlaceWall(player, GameBoard.GetTileAt(i, j), true))
                    {
                        moves.Add(new Move(i, j, true));
                    }
                }
            }

            // vertical wall placements
            for (int i = 0; i < GameBoard.Size; i++)
            {
                for (int j = 0; j < GameBoard.Size; j++)
                {
                    if (CanPlaceWall(player, GameBoard.GetTileAt(i, j), false))
                    {
                        moves.Add(new Move(i, j, false));
                    }
                }
            }
        }

        return moves;
    }

    public void OnTileSelected(Tile tile)
    {
        if (CurrentGameState == GameState.Ongoing)
        {
            if (CurrentGameMode == GameManager.GameMode.None)
            {
                if (testMode)
                {
                    if (selectedTile == null)
                    {
                        tile.Selected = true;
                        selectedTile = tile;
                    }
                    else
                    {
                        selectedTile.Selected = false;
                        selectedTile = tile;
                        selectedTile.Selected = true;
                    }
                    if (tile.HasPawn)
                    {
                        CurrentGameMode = GameManager.GameMode.MovePawn;
                    }
                }
                else
                {
                    if (tile.HasPawn && tile.Pawn.player == CurrentPlayer && !IsCPU(CurrentPlayer))
                    {
                        if (selectedTile == null)
                        {
                            tile.Selected = true;
                            selectedTile = tile;
                        }
                        else
                        {
                            selectedTile.Selected = false;
                            selectedTile = tile;
                            selectedTile.Selected = true;
                        }
                        CurrentGameMode = GameManager.GameMode.MovePawn;
                    }
                    else
                    {
                        //Debug.Log("Cannot select enemy piece or empty tile on game mode (Toggle 'Debug Mode').");
                    }
                }
            }
            else if (CurrentGameMode == GameManager.GameMode.MovePawn)
            {
                if (CanMovePiece(selectedTile, tile))
                {
                    PlayMove(_moveHistory, new Move(tile.row, tile.col), selectedTile.Pawn.player);
                    if (IsFinalState(CurrentPlayer))
                    {
                        CurrentGameState = GameState.Over;
                        Winner = CurrentPlayer;
                    }
                    OnTileDeselected(selectedTile);
                    NextTurn();
                    CurrentGameMode = GameManager.GameMode.None;
                }
            }
            else if (CurrentGameMode == GameManager.GameMode.PlaceWallH)
            {
                if (CanPlaceWall(CurrentPlayer, tile, true))
                {
                    if (PlayMove(_moveHistory, new Move(tile.row, tile.col, true), CurrentPlayer))
                    {
                        if (Minimax.IsBoardValid())
                        {
                            NextTurn();
                            CurrentGameMode = GameManager.GameMode.None;
                        }
                        else
                        {
                            UndoMove(_moveHistory.Pop(), CurrentPlayer);
                        }
                    }
                }
            }
            else if (CurrentGameMode == GameManager.GameMode.PlaceWallV)
            {
                if (CanPlaceWall(CurrentPlayer, tile, false))
                {
                    if (PlayMove(_moveHistory, new Move(tile.row, tile.col, false), CurrentPlayer))
                    {
                        if (Minimax.IsBoardValid())
                        {
                            NextTurn();
                            CurrentGameMode = GameManager.GameMode.None;
                        }
                        else
                        {
                            UndoMove(_moveHistory.Pop(), CurrentPlayer);
                        }
                    }
                }
            }
        }
    }

    public void OnTileDeselected(Tile tile)
    {
        tile.Selected = false;
        selectedTile = null;
        CurrentGameMode = GameManager.GameMode.None;
    }

    public void ChangeModeTo(GameManager.GameMode newMode)
    {
        if (CurrentGameMode == GameManager.GameMode.MovePawn)
        {
            OnTileDeselected(selectedTile);
        }

        CurrentGameMode = newMode;
    }

    public bool CanMovePiece(Tile a, Tile b)
    {
        return (a.IsNeighborOf(b) && !(b.HasPawn));
    }

    public void NextTurn()
    {
        CurrentPlayer = GetNextPlayer(CurrentPlayer);
    }
}
