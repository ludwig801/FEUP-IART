using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{
    public const int NumPlayers = 2;
    public const int NumWallsPerPlayer = 20;
    public const int BoardSize = 9;
    public const int BoardBorder = BoardSize - 1;
    public const int NumEvalFeatures = 4;

    public Pawn[] pawns;
    public Wall[,] walls;
    public Board board;

    public GameObject wallPrefab;

    public enum ReturnValue
    {
        ErrorPlay = -2,
        ErrorUndo = -1,
        GameOver = 0,
        GameDraw = 1,
        Success = 2
    }

    public int minimaxDepth;
    public int numGamesPerPlayer;
    public int maxPlies;
    int currentPlayer;
    int[] wallCount;
    Move bestMove;
    Stack<Move> moveHistory;
    Stopwatch stopwatch;
    Counter cuts;
    Counter plies;
    Counter totalTime;
    Counter[] wins;
    Counter[] losses;
    Counter draws;
    Counter numGames;

    // >> Heuristics <<
    // 
    // > Features
    //    > F1 : Max's distance to goal (measured in rows to final row)  
    //    > F2 : Difference between F1(Max) and F1(Min)
    //    > F3 : Max's minimum moves to next column (closer to Min's border)
    //    > F4 : Min's minimum moves to next column (closer to Max's border) 
    //
    // > Evalutation Function
    //      H(S) = W(Fi)*Fi(S) + Random;
    //
    // W(Fi)
    //
    // | F1 | x |
    // | F2 | x |
    // | F3 | x |
    // | F4 | x |
    int[] distanceToGoal;
    int[] distanceToNextRow;
    float[,] weight;
    int winner = -1;

    public bool runBattery = false;

    bool gameInProgress = false;
    int gameCount = 0;

    // Methods
    void Start()
    {
        CreateWalls();
        Init();
        Tests();
    }

    void Tests()
    {
        Reset(0);
        PlayMove(new Move(7, 4), 0);
        PlayMove(new Move(7, 0, true), 0);
        PlayMove(new Move(7, 3, true), 1);
        PlayMove(new Move(8, 1, true), 0);
        PlayMove(new Move(8, 3, true), 1);
        PlayMove(new Move(7, 4, false), 0);
        PlayMove(new Move(7, 2), 1);
    }

    void Update()
    {
        if (runBattery)
        {
            UpdateGameInProgress();
        }
    }

    void UpdateGameInProgress()
    {
        if (gameInProgress)
        {
            stopwatch.Reset();
            stopwatch.Start();

            ReturnValue retVal = MoveAI();
            currentPlayer = GetNextPlayer(currentPlayer);

            totalTime.Add(stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();

            switch (retVal)
            {
                case ReturnValue.ErrorPlay:
                    UnityEngine.Debug.Log("ERROR OCURRED!");
                    gameInProgress = false;
                    break;
                case ReturnValue.ErrorUndo:
                    UnityEngine.Debug.Log("ERROR OCURRED!");
                    gameInProgress = false;
                    break;
                case ReturnValue.GameOver:
                    numGames.Inc();
                    wins[winner].Inc();
                    losses[GetNextPlayer(winner)].Inc();
                    gameInProgress = false;
                    break;
                case ReturnValue.Success:
                    plies.Inc();
                    if (plies.Value >= this.maxPlies)
                    {
                        retVal = ReturnValue.GameDraw;
                        draws.Inc();
                        UnityEngine.Debug.Log("Draw. No accountability will be proccessed.");
                        gameInProgress = false;
                    }
                    break;
                case ReturnValue.GameDraw:
                    draws.Inc();
                    UnityEngine.Debug.Log("Draw. No accountability will be proccessed.");
                    gameInProgress = false;
                    break;
                default:
                    UnityEngine.Debug.Log("Unknown return value from AI move.");
                    break;
            }
        }
        else if (gameCount < numGamesPerPlayer * NumPlayers)
        {
            if (gameCount < numGamesPerPlayer)
            {
                Reset(0);
                gameInProgress = true;
            }
            else
            {
                Reset(1);
                gameInProgress = true;
            }
            gameCount++;
        }
        else
        {
            runBattery = false;
        }
    }

    // Methods
    public void Init()
    {
        board = GameObject.Find(Names.Board).GetComponent<Board>();
        board.Init();

        walls = new Wall[NumPlayers, NumWallsPerPlayer];
        pawns = new Pawn[NumPlayers];
        for (int i = 0; i < NumPlayers; i++)
        {
            for (int j = 0; j < NumWallsPerPlayer; j++)
            {
                walls[i, j] = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Walls).transform.GetChild(j).GetComponent<Wall>();
                walls[i, j].Init();
            }
            pawns[i] = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Pawn).GetComponent<Pawn>();
        }

        distanceToGoal = new int[NumPlayers];
        distanceToNextRow = new int[NumPlayers];
        weight = new float[NumEvalFeatures, NumPlayers];
        moveHistory = new Stack<Move>();

        stopwatch = new Stopwatch();
        cuts = new Counter();
        plies = new Counter();
        totalTime = new Counter();
        numGames = new Counter();
        wins = new Counter[NumPlayers];
        wins[0] = new Counter();
        wins[1] = new Counter();
        losses = new Counter[NumPlayers];
        losses[0] = new Counter();
        losses[1] = new Counter();
        draws = new Counter();

        // Heuristics
        SetEvaluationFeaturesWeights();

        // Statistics
        wins[0].Reset();
        wins[1].Reset();
        losses[0].Reset();
        losses[1].Reset();
        draws.Reset();
        Reset();

        gameInProgress = false;
        gameCount = 0;
    }

    public void Reset(int initialPlayer = 0)
    {
        board.Reset();

        SetEvaluationFeaturesWeights();

        for (int i = 0; i < NumPlayers; i++)
        {
            for (int j = 0; j < NumWallsPerPlayer; j++)
            {
                walls[i, j].Tile = null;
            }
        }

        if (pawns[0].Tile != null) board.RemoveTempLinks(pawns[0].Tile);
        if (pawns[1].Tile != null) board.RemoveTempLinks(pawns[1].Tile);
        pawns[0].Tile = board.GetTileAt(0, board.Size / 2);
        pawns[1].Tile = board.GetTileAt(board.Border, board.Size / 2);

        currentPlayer = initialPlayer;

        board.SetTempLinks(pawns[GetNextPlayer(initialPlayer)].Tile);
        stopwatch.Stop();
        stopwatch.Reset();
        cuts.Reset();
        plies.Reset();
        totalTime.Reset();
    }

    public ReturnValue MoveAI()
    {
        moveHistory.Clear();
        bestMove = null;

        MinimaxNode root = new MinimaxNode(true);
        ReturnValue retVal = MinimaxAlphaBeta(root, minimaxDepth, currentPlayer);

        switch (retVal)
        {
            case ReturnValue.ErrorPlay:
                UnityEngine.Debug.Log("Error trying to play a move.");
                break;
            case ReturnValue.ErrorUndo:
                UnityEngine.Debug.Log("Error trying to undo a move.");
                break;
            case ReturnValue.GameOver:
                UnityEngine.Debug.Log("GameOver. Winner: Player " + winner);
                break;
            case ReturnValue.Success:
                if (bestMove == null)
                {
                    retVal = ReturnValue.ErrorPlay;
                }
                else
                {
                    UnityEngine.Debug.Log("Player: " + currentPlayer + " : " + bestMove.ToString() + "  [" + root.heuristicValue + "]");
                    PlayMove(bestMove, currentPlayer);
                    moveHistory.Pop();
                }
                break;
            default:
                UnityEngine.Debug.Log("Unknown return value from Minimax Algorithm");
                break;
        }

        return retVal;
    }

    #region Minimax

    ReturnValue MinimaxAlphaBeta(MinimaxNode node, int depth, int player)
    {
        if (FinalState(currentPlayer))
        {
            winner = currentPlayer;
            return ReturnValue.GameOver;
        }

        if (0 == depth)
        {
            node.heuristicValue = CalcHeuristicValue(player);
            return ReturnValue.Success;
        }

        // Assign Moves
        List<Move> moves = GetPossibleMoves(player);

        // Evaluate Moves
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];

            MinimaxNode child = new MinimaxNode(!node.isMaximizer);

            if (!PlayMove(move, player))
            {
                return ReturnValue.ErrorPlay;
            }

            if (ValidBoard())
            {
                child.alpha = node.alpha;
                child.beta = node.beta;

                ReturnValue retVal = MinimaxAlphaBeta(child, depth - 1, GetNextPlayer(player));

                if (retVal != ReturnValue.Success)
                {
                    return retVal;
                }

                if (node.isMaximizer)
                {
                    if (depth == minimaxDepth && child.heuristicValue > node.heuristicValue)
                    {
                        bestMove = move;
                    }
                    node.heuristicValue = Mathf.Max(node.heuristicValue, child.heuristicValue);
                }
                else
                {
                    node.heuristicValue = Mathf.Min(node.heuristicValue, child.heuristicValue);
                }
            }

            if (!UndoMove(moveHistory.Pop(), player))
            {
                return ReturnValue.ErrorUndo;
            }

            if (AlphaBetaCut(node, child))
            {
                child = null;
                break;
            }

            child = null;
        }

        moves = null;
        return ReturnValue.Success;
    }

    bool FinalState(int player)
    {
        switch (player)
        {
            case 0:
                if (pawns[player].Tile.row == BoardBorder)
                {
                    return true;
                }
                break;
            case 1:
                if (pawns[player].Tile.row == 0)
                {
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    public bool ValidBoard()
    {
        return AStarDistanceToGoal();
    }

    bool AlphaBetaCut(MinimaxNode node, MinimaxNode child)
    {
        if (node.isMaximizer)
        {
            node.alpha = Mathf.Max(node.alpha, node.heuristicValue);
            if (node.alpha > node.beta)
            {
                return true;
            }

        }
        else
        {
            node.beta = Mathf.Min(node.beta, node.heuristicValue);
            if (node.alpha > node.beta)
            {
                return true;
            }

        }

        return false;
    }

    List<Move> GetPossibleMoves(int player)
    {
        List<Move> moves = new List<Move>();

        // pawn movements
        for (int i = 0; i < pawns[player].Tile.neighbors.Count; i++)
        {
            Tile tile = pawns[player].Tile.neighbors[i];
            if (!tile.HasPawn)
            {
                moves.Add(new Move(tile.row, tile.col));
            }
        }

        // horizontal wall placements
        for (int i = 0; i < board.Size; i++)
        {
            for (int j = 0; j < board.Size; j++)
            {
                if (CanPlaceWall(board.GetTileAt(i, j), true))
                {
                    moves.Add(new Move(i, j, true));
                }
            }
        }

        // vertical wall placements
        for (int i = 0; i < board.Size; i++)
        {
            for (int j = 0; j < board.Size; j++)
            {
                if (CanPlaceWall(board.GetTileAt(i, j), false))
                {
                    moves.Add(new Move(i, j, false));
                }
            }
        }

        return moves;
    }

    #endregion

    #region Heuristics

    float CalcHeuristicValue(int player)
    {
        // F1 & F2
        CalcDistanceToGoal();

        // F3 & F4
        AStarDistanceToNextRow();

        int nextPlayer = GetNextPlayer(player);

        float F1 = weight[0, player] * ((8f - distanceToGoal[player]) / 8f); // [0-1] ~normalized
        float F2 = weight[1, player] * ((distanceToGoal[nextPlayer] - distanceToGoal[player]) / 8f); // [0-1] ~normalized
        float F3 = weight[2, player] * ((10f - distanceToNextRow[player]) / 10f); // [0-1] ~normalized
        float F4 = weight[3, player] * ((distanceToNextRow[nextPlayer] - 10f) / 10f); // [0-1] ~normalized

        return F1 + F2 + F3 + F4 /*+ Random.Range(0f, 0.5f)*/;
    }

    void CalcDistanceToGoal()
    {
        distanceToGoal[0] = board.Border - pawns[0].Tile.row;
        distanceToGoal[1] = pawns[1].Tile.row;
    }

    void SetEvaluationFeaturesWeights()
    {
        // Must study weights
        //weight[0, 0] = Random.Range(0f, 2f);
        //weight[1, 0] = Random.Range(0f, 2f);
        //weight[2, 0] = Random.Range(0f, 2f);
        //weight[3, 0] = Random.Range(0f, 2f);

        //weight[0, 1] = Random.Range(0f, 2f);
        //weight[1, 1] = Random.Range(0f, 2f);
        //weight[2, 1] = Random.Range(0f, 2f);
        //weight[3, 1] = Random.Range(0f, 2f);

        weight[0, 0] = 1f;
        weight[1, 0] = 1f;
        weight[2, 0] = 2f;
        weight[3, 0] = 1f;

        weight[0, 1] = 1f;
        weight[1, 1] = 1f;
        weight[2, 1] = 1f;
        weight[3, 1] = 1f;
    }
    #endregion

    #region Misc

    bool PlayMove(Move move, int player)
    {
        board.RemoveTempLinks(pawns[GetNextPlayer(player)].Tile);
        board.RemoveTempLinks(pawns[player].Tile);

        switch (move.type)
        {
            case Move.MovePawn:
                moveHistory.Push(new Move(pawns[player].Tile.row, pawns[player].Tile.col));
                board.MovePawnTo(pawns[player], move.row, move.col);
                break;

            case Move.SetWall:
                Wall wall = GetWall(player);
                if (wall == null)
                {
                    return false;
                }
                moveHistory.Push(new Move(move));
                board.SetWall(wall, move.row, move.col, move.isHorizontal);
                break;

            default:
                return false;
        }

        board.SetTempLinks(pawns[player].Tile);
        return true;
    }

    bool UndoMove(Move move, int player)
    {
        board.RemoveTempLinks(pawns[player].Tile);
        board.RemoveTempLinks(pawns[GetNextPlayer(player)].Tile);

        switch (move.type)
        {
            case Move.MovePawn:
                board.MovePawnTo(pawns[player], move.row, move.col);
                break;

            case Move.SetWall:
                board.RemoveWall(move.row, move.col);
                break;

            default:
                return false;
        }

        board.SetTempLinks(pawns[GetNextPlayer(player)].Tile);
        return true;
    }

    Wall GetWall(int player)
    {
        for (int j = 0; j < NumWallsPerPlayer; j++)
        {
            Wall wall = walls[player, j];
            if (wall.Free)
            {
                return wall;
            }
        }

        return null;
    }

    int GetNextPlayer(int player)
    {
        player++;
        player %= NumPlayers;
        return player;
    }

    int GetPreviousPlayer(int player)
    {
        player--;
        if (player < 0)
        {
            player = NumPlayers + player;
        }
        return player;
    }

    bool CanPlaceWall(Tile tile, bool horizontal)
    {
        int row = tile.row;
        int col = tile.col;

        if (row == 0 || col == board.Border || tile.HasWall)
        {
            return false;
        }
        else if (horizontal)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                if (board.IsValidPosition(row, col + i))
                {
                    Tile b = board.GetTileAt(row, col + i);
                    if (b.HasWall && b.Wall.Horizontal)
                    {
                        return false;
                    }
                }
            }
        }
        else // vertical
        {
            for (int i = -1; i <= 1; i += 2)
            {
                if (board.IsValidPosition(row + i, col))
                {
                    Tile b = board.GetTileAt(row + i, col);
                    if (b.HasWall && b.Wall.Vertical)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    #endregion

    #region AStar

    bool AStarDistanceToNextRow()
    {
        int player = 0;
        int objective = pawns[player].Tile.row + 1;
        int[] retVal = new int[NumPlayers];

        retVal[player] = AStar(player, objective);
        if (retVal[player] < 0)
        {
            return false;
        }

        player = 1;
        objective = pawns[player].Tile.row - 1;
        if (retVal[player] < 0)
        {
            return false;
        }

        distanceToNextRow[0] = retVal[0];
        distanceToNextRow[1] = retVal[1];

        return false;
    }

    bool AStarDistanceToGoal()
    {
        int player = 0;
        int objective = BoardBorder;
        int[] retVal = new int[NumPlayers];

        retVal[player] = AStar(player, objective);
        if (retVal[player] < 0)
        {
            return false;
        }

        player = 1;
        objective = 0;
        retVal[player] = AStar(player, objective);
        if (retVal[player] < 0)
        {
            return false;
        }

        return true;
    }

    int AStar(int player, int objective)
    {
        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        Tile start = pawns[player].Tile;
        start.gValue = 0;
        start.hValue = AStarHeuristicValue(start, objective);
        start.fValue = start.gValue + start.hValue;
        start.parent = null;

        openList.Add(start);

        Tile current;

        while (openList.Count > 0)
        {
            current = AStarBest(openList);

            if ((player == 0 && current.row >= objective) || (player == 1 && current.row <= objective))
            {
                int val = AStarReconstruct(start, current);
                openList = null;
                closedList = null;
                return val;
            }

            openList.Remove(current);
            closedList.Add(current);

            for (int i = 0; i < current.neighbors.Count; i++)
            {
                Tile temp = current.neighbors[i];
                if (closedList.Contains(temp) || temp.HasPawn)
                {
                    continue;
                }
                else
                {
                    int tempG = current.gValue + 1;
                    bool inOpenList = openList.Contains(temp);

                    if (!inOpenList || tempG < temp.gValue)
                    {
                        temp.parent = current;
                        temp.gValue = tempG;
                        temp.hValue = AStarHeuristicValue(temp, objective);
                        temp.fValue = temp.gValue + temp.hValue;
                        if (!inOpenList)
                        {
                            openList.Add(temp);
                        }
                    }
                }
            }
        }

        openList = null;
        closedList = null;
        return -1;
    }

    int AStarReconstruct(Tile start, Tile current)
    {
        int val = 0;

        while (current != start)
        {
            val++;
            current = current.parent;
        }

        return val;
    }

    Tile AStarBest(List<Tile> openList)
    {
        int best = -1;
        for (int i = 0; i < openList.Count; i++)
        {
            if (best < 0)
            {
                best = i;
            }
            else if (openList[i].fValue < openList[best].fValue)
            {
                best = i;
            }
        }

        return openList[best];
    }

    int AStarHeuristicValue(Tile node, int objective)
    {
        return Mathf.Abs(node.row - objective);
    }

    #endregion

    void CreateWalls()
    {
        for (int i = 0; i < NumPlayers; i++)
        {
            Transform walls = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Walls).transform;

            for (int j = 0; j < NumWallsPerPlayer; j++)
            {
                GameObject wall = Instantiate(wallPrefab, new Vector3(), Quaternion.identity) as GameObject;
                wall.transform.SetParent(walls);
            }
        }
    }
}
