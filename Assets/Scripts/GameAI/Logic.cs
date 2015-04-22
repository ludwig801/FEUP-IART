using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class Logic
{
    public const int NumPlayers = 2;
    public const int NumWallsPerPlayer = 20;
    public const int BoardSize = 9;
    public const int BoardBorder = BoardSize - 1;
    public const int NumEvalFeatures = 4;

    public Pawn[] pawns;
    public Wall[,] walls;
    public Board board;
    public bool GameOver = false;

    private int minimaxDepth;
    private int currentPlayer;
    private int[] wallCount;
    private Move bestMove;
    private Stack<Move> moveHistory;
    private Text boardText;
    private Stopwatch stopwatch;
    private Counter cuts;

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

    // Methods
    public Logic(int minimaxDepth)
    {
        this.minimaxDepth = minimaxDepth;

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

        pawns[0].Tile = board.GetTileAt(0, board.Size / 2);
        pawns[1].Tile = board.GetTileAt(board.Border, board.Size / 2);

        // Heuristics
        distanceToGoal = new int[NumPlayers];
        distanceToNextRow = new int[NumPlayers];
        weight = new float[NumEvalFeatures,NumPlayers];
        SetEvalWeights();

        //
        moveHistory = new Stack<Move>();
        currentPlayer = 0;

        stopwatch = new Stopwatch();
        cuts = new Counter();
        cuts.Reset();

        boardText = GameObject.Find("Canvas").transform.FindChild("Panel").FindChild("Board Text").GetComponent<Text>();

        //
        //Tests();
    }

    void Tests()
    {
        Move placeWall0 = new Move(5, 4, true);
        Move placeWall1 = new Move(4, 3, false);
        Move placeWall2 = new Move(4, 5, false);
        PlayMove(placeWall0, 0);
        PlayMove(placeWall1, 0);
        PlayMove(placeWall2, 0);

        stopwatch.Reset();
        stopwatch.Start();

        if (!AStar())
        {
            UnityEngine.Debug.Log("ERROR");
        }
        else
        {
            UnityEngine.Debug.Log("Elapsed Time: " + stopwatch.ElapsedMilliseconds);
            UnityEngine.Debug.Log("DNR[0] = " + distanceToNextRow[0]);
            UnityEngine.Debug.Log("DNR[1] = " + distanceToNextRow[1]);
        }
    }

    public void MoveAI(int depth)
    {
        stopwatch.Reset();
        stopwatch.Start();

        moveHistory = new Stack<Move>();
        bestMove = null;

        minimaxDepth = depth;
        MinimaxNode root = new MinimaxNode(true);
        MinimaxAlphaBeta(root, depth, currentPlayer);

        UnityEngine.Debug.Log("Will play: " + bestMove.ToString() + "  [" + root.heuristicValue + "]");
        PlayMove(bestMove, currentPlayer);
        UnityEngine.Debug.Log("Elapsed Time: " + stopwatch.ElapsedMilliseconds);
        AStar();
        UnityEngine.Debug.Log("DNR[0] = " + distanceToNextRow[0]);
        UnityEngine.Debug.Log("DNR[1] = " + distanceToNextRow[1]);
        moveHistory.Pop();
        currentPlayer = GetNextPlayer(currentPlayer);

        PrintBoard(currentPlayer);
    }

    #region Minimax

    bool MinimaxAlphaBeta(MinimaxNode node, int depth, int player)
    {
        if (0 == depth || FinalNode(player))
        {
            node.heuristicValue = CalcHeuristicValue(player);
            return true;
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
                return false;
            }

            child.alpha = node.alpha;
            child.beta = node.beta;

            if (!MinimaxAlphaBeta(child, depth - 1, GetNextPlayer(player)))
            {
                return false;
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

            if (!UndoMove(moveHistory.Pop(), player))
            {
                return false;
            }

            if (AlphaBetaCut(node, child))
            {
                child = null;
                break;
            }

            child = null;
        }

        moves = null;
        return true;
    }

    bool FinalNode(int player)
    {
        switch (player)
        {
            case 0:
                if(pawns[player].Tile.row == BoardBorder)
                {
                    return true;
                }
                break;
            case 1:
                if(pawns[player].Tile.row == 0)
                {
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
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
        AStar();
        //AStar(player);

        int nextPlayer = GetNextPlayer(player);

        float F1 = weight[0, player] * distanceToGoal[player];
        float F2 = weight[1, player] * (distanceToGoal[player] - distanceToGoal[nextPlayer]);
        float F3 = weight[2, player] * (10f - distanceToNextRow[player]);
        float F4 = weight[3, player] * (distanceToNextRow[nextPlayer] - 10f);

        return F1 + F2 + F3 + F4 + Random.Range(0f,0.5f);
    }

    void CalcDistanceToGoal()
    {
        distanceToGoal[0] = pawns[0].Tile.row;
        distanceToGoal[1] = board.Size - pawns[1].Tile.row;
    }

    void SetEvalWeights()
    {
        // Must study weights
        weight[0, 0] = 1.6f;
        weight[1, 0] = 2f;
        weight[2, 0] = 1f;
        weight[3, 0] = 1f;

        weight[0, 1] = 1.6f;
        weight[1, 1] = 2f;
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

    bool AStar()
    {

        //UnityEngine.Debug.Log("AStar(0)");

        if (!AStar(0))
        {
            return false;
        }

        //UnityEngine.Debug.Log("AStar(1)");

        if (!AStar(1))
        {
            return false;
        }

        return true;
    }

    bool AStar(int player)
    {
        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        Tile start = pawns[player].Tile;
        int objective = (player == 0 ? start.row + 1 : start.row - 1);
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
                distanceToNextRow[player] = AStarReconstruct(start, current);
                openList = null;
                closedList = null;
                return true;
            }

            openList.Remove(current);
            closedList.Add(current);

            for (int i = 0; i < current.neighbors.Count; i++)
            {
                Tile temp = current.neighbors[i];
                if(closedList.Contains(temp) || temp.HasPawn)
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
                        if(!inOpenList)
                        {
                            openList.Add(temp);
                        }
                    }
                }
            }
        }

        openList = null;
        closedList = null;
        return false;
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

    #region Debug Print

    void PrintBoard(int player)
    {
        //StringWriter stream = new StringWriter();

        //for (int row = board.Size - 1; row >= 0; row--)
        //{
        //    int nextRow = row + 1;
        //    for (int col = 0; col < board.Size; col++)
        //    {
        //        Tile tile = board.GetTileAt(row, col);

        //        stream.Write("[");
        //        if (tile.GetValue(player) == int.MaxValue)
        //        {
        //            stream.Write("X");
        //        }
        //        else
        //        {
        //            stream.Write(tile.GetValue(player));
        //        }
        //        stream.Write("]");

        //        if (tile.HasWall && tile.Wall.Vertical)
        //        {
        //            stream.Write("|");
        //        }
        //        else if (row < (board.Size - 1) && board.GetTileAt(nextRow, col).HasWall && board.GetTileAt(nextRow, col).Wall.Vertical)
        //        {
        //            stream.Write("|");
        //        }
        //        else
        //        {
        //            if (tile.GetValue(player) < 10)
        //            {
        //                stream.Write(" ");
        //            }
        //        }
        //    }
        //    stream.WriteLine();
        //    if (row < board.Border)
        //    {
        //        for (int col = 0; col < board.Size; col++)
        //        {
        //            if (board.GetTileAt(row, col).HasWall)
        //            {
        //                if (board.GetTileAt(row, col).Wall.Horizontal)
        //                {
        //                    stream.Write("----");
        //                }
        //                else
        //                {
        //                    if (col > 0 && board.GetTileAt(row, col - 1).HasWall && board.GetTileAt(row, col - 1).Wall.Horizontal)
        //                    {
        //                        stream.Write("---|");
        //                    }
        //                    else
        //                    {
        //                        stream.Write("   |");
        //                    }
        //                }
        //            }
        //            else if (col > 0 && board.GetTileAt(row, col - 1).HasWall && board.GetTileAt(row, col - 1).Wall.Horizontal)
        //            {
        //                stream.Write("--- ");
        //            }
        //            else
        //            {
        //                stream.Write("    ");
        //            }
        //        }
        //    }
        //    stream.WriteLine();
        //}

        //boardText.text = stream.ToString();
    }

    #endregion
}
