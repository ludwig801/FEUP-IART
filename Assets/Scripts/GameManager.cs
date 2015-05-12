using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public const int NumPlayers = 2;
    public const int BoardSize = 9;
    public const int BoardBorder = BoardSize - 1;
    public const int NumEvalFeatures = 4;

    public Transform wallPrefab;
    public Transform boardPrefab;
    public Transform pawnPrefab;

    public enum GameState
    {
        Error = -1,
        Over = 0,
        Draw = 1,
        Ongoing = 2,
        Stopped = 3
    }

    public GameState gameState;

    public int numWallsPerPlayer;
    public int minimaxDepth;
    public int numGamesPerPlayer;
    public int maxPlies;
    int currentPlayer;
    Move bestMove;
    Stack<Move> moveHistory;
    Pawn[] pawns;
    Wall[,] walls;
    Board board;
    Counter plieCounter;

    // >> Heuristics <<
    // 
    // > Features
    //    > F1 : Max's distance to goal
    //    > F2 : Difference between F1(Max) and F1(Min)
    //    > F3 : Max's minimum moves to next column (closer to Min's border)
    //    > F4 : Min's minimum moves to next column (closer to Max's border) 
    //
    // > Evalutation Function
    //      H(S) = W(Fi)*Fi(S) + Random;
    //
    float F1, F2, F3, F4;
    Tile currentTile;
    int[] distanceToNextRow;
    int[] distanceToGoal;
    float[,] weight;

    public int winner = -1;

    // Methods
    void Start()
    {
        CreateBoard();
        CreatePawns();
        CreateWalls();
        CreateGameVars();
    }
    
    void Update()
    {
        if (gameState == GameState.Ongoing)
        {
            GameState retVal = PlayAIMove();

            switch (retVal)
            {
                case GameState.Error:
                    gameState = GameState.Error;
                    break;
                case GameState.Over:
                    gameState = GameState.Over;
                    break;
                case GameState.Ongoing:
                    plieCounter.Inc();
                    if (plieCounter.Value >= this.maxPlies)
                    {
                        gameState = GameState.Over;
                    }
                    else
                    {
                        gameState = GameState.Ongoing;
                    }
                    break;
                case GameState.Draw:
                    gameState = GameState.Draw;
                    break;
                default:
                    gameState = GameState.Error;
                    break;
            }
        }
    }

    void CreateBoard()
    {
        Transform instance = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.identity) as Transform;
        instance.name = Names.Board;
        board = instance.GetComponent<Board>();
        board.Init();
    }

    void CreatePawns()
    {
        pawns = new Pawn[NumPlayers];

        for (int i = 0; i < NumPlayers; i++)
        {
            Transform instance = Instantiate(pawnPrefab, new Vector3(0, 0, 0), Quaternion.identity) as Transform;
            instance.name = Names.Pawn_ + i;
            instance.parent = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i);
            pawns[i] = instance.GetComponent<Pawn>();
            pawns[i].player = i;
        }
    }

    void CreateWalls()
    {
        walls = new Wall[NumPlayers, numWallsPerPlayer];

        for (int i = 0; i < NumPlayers; i++)
        {
            Transform parent = GameObject.Find(Names.GamePieces).transform.FindChild(Names.Player_ + i).transform.FindChild(Names.Walls).transform;

            for (int j = 0; j < numWallsPerPlayer; j++)
            {
                Transform instance = Instantiate(wallPrefab, new Vector3(0, 0, 0), Quaternion.identity) as Transform;
                instance.parent = parent;
                instance.name = Names.Wall_ + j;
                walls[i, j] = instance.GetComponent<Wall>();
                walls[i, j].Init();
                walls[i, j].player = i;
            }
        }
    }

    void CreateGameVars()
    {
        distanceToNextRow = new int[NumPlayers];
        distanceToGoal = new int[NumPlayers];
        weight = new float[NumEvalFeatures, NumPlayers];
        moveHistory = new Stack<Move>();

        plieCounter = new Counter();

        NewGame(0);

        gameState = GameState.Stopped;
    }

    public void Reset()
    {
        gameState = GameState.Stopped;
    }

    public void NewGame(int initialPlayer)
    {
        board.Reset();

        for (int i = 0; i < NumPlayers; i++)
        {
            for (int j = 0; j < numWallsPerPlayer; j++)
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

        gameState = GameState.Ongoing;
    }

    public GameState PlayAIMove()
    {
        moveHistory.Clear();
        bestMove = null;

        MinimaxNode root = new MinimaxNode(true);
        currentTile = pawns[currentPlayer].Tile;
        GameState retVal = MinimaxAlphaBeta(root, minimaxDepth, currentPlayer);

        switch (retVal)
        {
            case GameState.Error:
                break;
            case GameState.Over:
                break;
            case GameState.Ongoing:
                if (bestMove == null)
                {
                    retVal = GameState.Error;
                }
                else
                {
                    PlayMove(bestMove, currentPlayer);
                }
                break;
            default:
                retVal = GameState.Error;
                break;
        }

        currentPlayer = GetNextPlayer(currentPlayer);
        return retVal;
    }

    public void PlayAIMove(Move move, int player)
    {
        PlayMove(move, player);
        float val = CalcHeuristicValue(currentPlayer);
        Debug.Log("Player: " + currentPlayer + " : " + moveHistory.Peek().ToString() + "  [" + F1 + ", " + F2 + ", " + F3 + ", " + F4 + "; " + val + "]");
        moveHistory.Pop();
    }
    
    public void UndoAIMove()
    {
        currentPlayer = GetPreviousPlayer(currentPlayer);

        UndoMove(moveHistory.Peek(), currentPlayer);
    }

    #region Minimax

    GameState MinimaxAlphaBeta(MinimaxNode node, int depth, int player)
    {
        if (FinalState(currentPlayer))
        {
            winner = currentPlayer;
            return GameState.Over;
        }

        if (0 == depth)
        {
            node.heuristicValue = CalcHeuristicValue(currentPlayer);
            return GameState.Ongoing;
        }

        // Assign Moves
        List<Move> moves = GetPossibleMoves(player);
        MinimaxNode child;

        // Evaluate Moves
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];

            child = new MinimaxNode(!node.isMaximizer);

            if (!PlayMove(move, player))
            {
                return GameState.Error;
            }

            if (ValidBoard())
            {
                child.alpha = node.alpha;
                child.beta = node.beta;
                child.move = move;

                GameState retVal = MinimaxAlphaBeta(child, depth - 1, GetNextPlayer(player));

                if (retVal != GameState.Ongoing)
                {
                    return retVal;
                }

                if (node.isMaximizer)
                {
                    if ((depth == minimaxDepth) && (child.heuristicValue > node.heuristicValue))
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
            else
            {
                //Debug.Log("Invalid Board!");
            }

            if (!UndoMove(moveHistory.Pop(), player))
            {
                return GameState.Error;
            }

            if (AlphaBetaCut(node, child))
            {
                break;
            }
        }

        child = null;
        moves = null;
        return GameState.Ongoing;
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

        if (GetWall(player) != null)
        {
            // horizontal wall placements
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    if (CanPlaceWall(player, board.GetTileAt(i, j), true))
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
                    if (CanPlaceWall(player, board.GetTileAt(i, j), false))
                    {
                        moves.Add(new Move(i, j, false));
                    }
                }
            }
        }

        return moves;
    }

    float CalcHeuristicValue(int player)
    {
        // F3 & F4
        AStarDistanceToNextRow();

        int nextPlayer = GetNextPlayer(player);

        float w1 = weight[0, player];
        float w2 = weight[1, player];
        float w3 = weight[2, player];
        float w4 = weight[3, player];

        F1 = w1 * ((81f - distanceToGoal[player]) / 81f);
        F2 = w2 * ((distanceToGoal[nextPlayer] - 81f) / 81f);
        F3 = w3 * ((9f - distanceToNextRow[player]) / 9f);
        F4 = w4 * ((distanceToNextRow[nextPlayer] - 9f) / 9f);

        return (F1 + F2 + F3 + F4) * Random.Range(0.99f,1.01f);
    }
    #endregion

    #region AStar

    bool AStarDistanceToNextRow()
    {
        int player = 0;
        int objective = currentTile.row + 1;
        int[] retVal = new int[NumPlayers];

        retVal[player] = AStar(player, objective, true);
        if (retVal[player] < 0)
        {
            return false;
        }

        player = 1;
        objective = currentTile.row - 1;
        retVal[player] = AStar(player, objective, true);
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
        int objective = board.Border;
        int[] retVal = new int[NumPlayers];

        retVal[player] = AStar(player, objective, false);
        if (retVal[player] < 0)
        {
            return false;
        }

        player = 1;
        objective = 0;
        retVal[player] = AStar(player, objective, false);
        if (retVal[player] < 0)
        {
            return false;
        }

        distanceToGoal[0] = retVal[0];
        distanceToGoal[1] = retVal[1];

        return true;
    }

    int AStar(int player, int objective, bool verifyHasPawn)
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
            current = AStarBest(openList, closedList);

            if (current == null)
            {
                break;
            }

            // If objective reached
            if ((player == 0 && current.row >= objective) || (player == 1 && current.row <= objective))
            {
                int val = AStarReconstruct(start, current);
                openList = null;
                closedList = null;
                return val;
            }

            // Remove current from the open list and add it to closed list
            openList.Remove(current);
            closedList.Add(current);

            // Add current's neighbors to the open list
            for (int i = 0; i < current.neighbors.Count; i++)
            {
                Tile temp = current.neighbors[i];

                if (!(closedList.Contains(temp) || (verifyHasPawn && temp.HasPawn)))
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

    Tile AStarBest(List<Tile> openList, List<Tile> closedList)
    {
        int best = -1;
        for (int i = 0; i < openList.Count; i++)
        {
            if ((best < 0) || (openList[i].fValue < openList[best].fValue))
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
        for (int j = 0; j < numWallsPerPlayer; j++)
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

    bool CanPlaceWall(int player, Tile tile, bool horizontal)
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
}
