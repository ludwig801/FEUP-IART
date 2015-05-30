using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(UIManager))]
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
        Stopped = 3,
        Paused = 4
    }

    public enum Mode
    {
        None = 0,
        MovePawn = 1,
        PlaceWallH = 2,
        PlaceWallV = 3
    }

    public bool CPU_0;
    public bool CPU_1;
    public bool testMode;
    public GameState gameState;
    public Mode mode;
    public int numWallsPerPlayer;
    public int minimaxDepth;
    public int numGamesPerPlayer;
    public int maxPlies;
    public int currentPlayer;
    public int winner = -1;
    public int[] availableWalls;
    public Tile selectedTile = null;

    Move bestMove;
    Stack<Move> minimaxHistory;
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
    int[] distanceToNextRow;
    int[] distanceToGoal;
    float[,] weight;
    bool wait;
    float waitingTime;

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
            if (IsCPU(currentPlayer))
            {
                if (wait && waitingTime <= 0.450f)
                {
                    waitingTime += Time.deltaTime;
                }
                else
                {
                    waitingTime = 0;
                    wait = false;
                    //Debug.Log("Player " + currentPlayer + " is CPU");

                    int res = PlayAIMove();

                    if (res > 0) // everything went ok
                    {
                        if (FinalState(currentPlayer))
                        {
                            gameState = GameState.Over;
                            winner = currentPlayer;
                            PrintGameOver();
                        }
                        else
                        {
                            plieCounter.Inc();
                            if (CPU_0 && CPU_1 && (plieCounter.Value >= this.maxPlies))
                            {
                                gameState = GameState.Draw;
                            }
                            NextTurn();
                        }
                    }
                    else if (res < 0)
                    {
                        gameState = GameState.Error;
                        PrintError();
                    }
                }
            }
        }
        //else
        //{
        //    Debug.Log("Game Stopped");
        //}
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
        //SetWeights();
        ReadWeights();
        moveHistory = new Stack<Move>();
        plieCounter = new Counter();

        availableWalls = new int[NumPlayers];

        //CPU_0 = true;
        //CPU_1 = true;

        //NewGame(0);
        gameState = GameState.Stopped;
    }

    public void Reset()
    {
        gameState = GameState.Stopped;
        winner = -1;
    }

    public void NewGame(int initialPlayer)
    {
        gameState = GameState.Stopped;
        winner = -1;

        board.Reset();

        for (int i = 0; i < NumPlayers; i++)
        {
            for (int j = 0; j < numWallsPerPlayer; j++)
            {
                if (walls[i, j].Tile != null)
                {
                    walls[i, j].Tile.RemoveWall();
                    walls[i, j].Tile = null;
                }
            }
        }

        if (pawns[0].Tile != null)
        {
            board.RemoveTempLinks(pawns[0].Tile);
            pawns[0].Tile.RemovePawn();
        }
        if (pawns[1].Tile != null)
        {
            board.RemoveTempLinks(pawns[1].Tile);
            pawns[1].Tile.RemovePawn();
        }
        pawns[0].Tile = board.GetTileAt(0, board.Size / 2);
        pawns[1].Tile = board.GetTileAt(board.Border, board.Size / 2);
        currentPlayer = initialPlayer;
        board.SetTempLinks(pawns[GetNextPlayer(currentPlayer)].Tile);

        if (selectedTile != null)
        {
            OnTileDeselected(selectedTile);
        }
        selectedTile = null;

        availableWalls[0] = numWallsPerPlayer;
        availableWalls[1] = numWallsPerPlayer;

        moveHistory = new Stack<Move>();

        gameState = GameState.Ongoing;
        mode = Mode.None;
    }

    public int PlayAIMove()
    {
        int res = MinimaxAlphaBeta();

        if (res >= 0) // Everything went ok (verify bestMove)
        {
            if (bestMove == null)
            {
                return -1;
            }

            if (!PlayMove(moveHistory, bestMove, currentPlayer))
            {
                return -1;
            }
        }
        else if (res < 0) // Error ocurred
        {
            return -1;
        }

        return 1;
    }

    public void PlayAIMove(Move move, int player)
    {
        PlayMove(moveHistory, move, player);
    }

    public void UndoLastMove()
    {
        if (moveHistory != null && moveHistory.Count > 0)
        {
            currentPlayer = GetPreviousPlayer(currentPlayer);

            UndoLastMove(moveHistory.Peek(), currentPlayer);
        }
    }

    #region Minimax

    int MinimaxAlphaBeta()
    {
        bestMove = null;
        minimaxHistory = new Stack<Move>();
        MinimaxNode root = new MinimaxNode(true);

        int res = MinimaxAlphaBeta(root, minimaxDepth, currentPlayer);

        return res;
    }

    int MinimaxAlphaBeta(MinimaxNode node, int depth, int player)
    {
        if ((0 == depth) || FinalState())
        {
            node.heuristicValue = CalcHeuristicValue(currentPlayer);
            return 1;
        }

        // Assign Moves
        List<Move> moves = GetPossibleMoves(player);
        MinimaxNode child;

        // Evaluate Moves
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];

            child = new MinimaxNode(!node.isMaximizer);

            if (!PlayMove(minimaxHistory, move, player))
            {
                continue;
            }

            if (ValidBoard())
            {
                child.alpha = node.alpha;
                child.beta = node.beta;
                child.move = move;

                int res = MinimaxAlphaBeta(child, depth - 1, GetNextPlayer(player));

                if (res < 0)
                {
                    return -1;
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

            if (!UndoLastMove(minimaxHistory.Pop(), player))
            {
                return -1;
            }

            if (AlphaBetaCut(node, child))
            {
                break;
            }
        }

        return 1;
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

    bool FinalState()
    {
        return (pawns[0].Tile.row == BoardBorder || pawns[1].Tile.row == 0);
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
        int row = getRowAbs(nextPlayer);

        //F1 = w1 * ((81f - distanceToGoal[player]) / 81f);
        //F2 = w2 * ((distanceToGoal[nextPlayer] - 81f) / 81f);
        //F3 = w3 * ((9f - distanceToNextRow[player]) / 9f);
        //F4 = w4 * ((distanceToNextRow[nextPlayer] - 9f) / 9f);
        F1 = w1 * (0.012f) * (80 - distanceToGoal[player]);
        F2 = w2 * (0.012f) * (distanceToGoal[nextPlayer] - 80);
        F3 = w3 * (0.111f) * (8 - distanceToNextRow[player]);
        F4 = w4 * (0.111f) * (distanceToNextRow[nextPlayer] - 8) * (0.111f) * row;

        return (F1 + F2 + F3 + F4) * Random.Range(0.99f, 1.01f);
    }
    #endregion

    #region AStar

    bool AStarDistanceToNextRow()
    {
        int player = 0;
        int objective = pawns[0].Tile.row + 1;
        int[] retVal = new int[NumPlayers];

        retVal[player] = AStar(player, objective, true);
        //Debug.Log(retVal[player]);
        if (retVal[player] < 0)
        {
            return false;
        }

        player = 1;
        objective = pawns[1].Tile.row - 1;
        retVal[player] = AStar(player, objective, true);
        //Debug.Log(retVal[player]);
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

    int getRowAbs(int player)
    {
        return (player == 0 ? pawns[0].Tile.row : BoardBorder - pawns[1].Tile.row);
    }

    bool PlayMove(Stack<Move> history, Move move, int player)
    {
        board.RemoveTempLinks(pawns[GetNextPlayer(player)].Tile);
        board.RemoveTempLinks(pawns[player].Tile);

        switch (move.type)
        {
            case Move.MovePawn:
                history.Push(new Move(pawns[player].Tile.row, pawns[player].Tile.col));
                board.MovePawnTo(pawns[player], move.row, move.col);
                break;

            case Move.SetWall:
                Wall wall = GetWall(player);
                if (wall == null)
                {
                    return false;
                }
                history.Push(new Move(move));
                board.SetWall(wall, move.row, move.col, move.isHorizontal);
                availableWalls[player]--;
                break;

            default:
                return false;
        }

        board.SetTempLinks(pawns[player].Tile);
        return true;
    }

    bool UndoLastMove(Move move, int player)
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
                availableWalls[player]++;
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

    bool IsCPU(int player)
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

    public bool IsCPUCurrentPlayer()
    {
        return IsCPU(currentPlayer);
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

    public void OnTileSelected(Tile tile)
    {
        if (gameState == GameState.Ongoing)
        {
            if (mode == GameManager.Mode.None)
            {
                //Debug.Log("Test mode: " + testMode);
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
                        mode = GameManager.Mode.MovePawn;
                    }
                }
                else
                {
                    if (tile.HasPawn && tile.Pawn.player == currentPlayer && !IsCPU(currentPlayer))
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
                        mode = GameManager.Mode.MovePawn;
                    }
                    else
                    {
                        //Debug.Log("Cannot select enemy piece or empty tile on game mode (Toggle 'Debug Mode').");
                    }
                }
            }
            else if (mode == GameManager.Mode.MovePawn)
            {
                if (CanMovePiece(selectedTile, tile))
                {
                    PlayMove(moveHistory, new Move(tile.row, tile.col), selectedTile.Pawn.player);
                    if (FinalState(currentPlayer))
                    {
                        gameState = GameState.Over;
                        winner = currentPlayer;
                        PrintGameOver();
                    }
                    OnTileDeselected(selectedTile);
                    NextTurn();
                    mode = GameManager.Mode.None;
                    wait = true;
                }
            }
            else if (mode == GameManager.Mode.PlaceWallH)
            {
                if (CanPlaceWall(currentPlayer, tile, true))
                {
                    if (PlayMove(moveHistory, new Move(tile.row, tile.col, true), currentPlayer))
                    {
                        if (ValidBoard())
                        {
                            NextTurn();
                            mode = GameManager.Mode.None;
                            wait = true;
                        }
                        else
                        {
                            UndoLastMove(moveHistory.Pop(), currentPlayer);
                        }
                    }
                }
            }
            else if (mode == GameManager.Mode.PlaceWallV)
            {
                if (CanPlaceWall(currentPlayer, tile, false))
                {
                    if (PlayMove(moveHistory, new Move(tile.row, tile.col, false), currentPlayer))
                    {
                        if (ValidBoard())
                        {
                            NextTurn();
                            mode = GameManager.Mode.None;
                            wait = true;
                        }
                        else
                        {
                            UndoLastMove(moveHistory.Pop(), currentPlayer);
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
        mode = GameManager.Mode.None;
    }

    public void ChangeModeTo(GameManager.Mode newMode)
    {
        if (mode == GameManager.Mode.MovePawn)
        {
            OnTileDeselected(selectedTile);
        }

        mode = newMode;
    }

    public bool CanMovePiece(Tile a, Tile b)
    {
        return (a.IsNeighborOf(b) && !(b.HasPawn));
    }

    public void NextTurn()
    {
        currentPlayer = GetNextPlayer(currentPlayer);
        waitingTime = 0;
    }

    public void SetWeights(float[,] param = null)
    {
        if (param == null)
        {
            weight[0, 0] = 3f;
            weight[1, 0] = 1f;
            weight[2, 0] = 0f;
            weight[3, 0] = 1f;

            weight[0, 1] = 3f;
            weight[1, 1] = 1f;
            weight[2, 1] = 0f;
            weight[3, 1] = 0f;
        }
        else
        {
            weight = param;
        }
    }

    public void SetWeight(int player, int index, float val)
    {
        weight[index, player] = val;
    }

    public float GetWeight(int player, int index)
    {
        return weight[index, player];
    }

    public void PrintDebug()
    {
        //AStarDistanceToNextRow();
        //AStarDistanceToGoal();
        //float val = CalcHeuristicValue(0);
        //Debug.Log("Player: " + 0 + " : " + F1 + " | " + F2 + " | " + F3 + " | " + F4 + " | " + val);
        //val = CalcHeuristicValue(1);
        //Debug.Log("Player: " + 1 + " : " + F1 + " | " + F2 + " | " + F3 + " | " + F4 + " | " + val);
        string debug = "State: " + gameState.ToString() + System.Environment.NewLine;
        debug += "Mode: " + mode.ToString() + System.Environment.NewLine;
        debug += "Wait: " + wait + System.Environment.NewLine;
        debug += "Elapsed: " + waitingTime + System.Environment.NewLine;
        debug += "Best: " + ((bestMove == null) ?  "Undefined" : bestMove.ToString()) + System.Environment.NewLine;
        debug += "Is CPU: " + IsCPU(0) + " | " + IsCPU(1) + System.Environment.NewLine;

        Debug.Log(debug);
    }

    public void PrintGameOver()
    {
        Debug.Log("GameOver: " + winner);
    }

    public void PrintError()
    {
        Debug.Log("ERROR");
    }

    public void PrintWeights()
    {
        Debug.Log("Weights 0: " + weight[0, 0] + "| " + weight[1, 0] + "| " + weight[2, 0] + "| " + weight[3, 0]);
        Debug.Log("Weights 1: " + weight[0, 1] + "| " + weight[1, 1] + "| " + weight[2, 1] + "| " + weight[3, 1]);
    }

    public void SaveWeights()
    {
        StreamWriter stream = new StreamWriter(Names.SaveWeightsPath_ + Names.SaveExt, false);
        stream.WriteLine(weight[0, 0] + " " + weight[1, 0] + " " + weight[2, 0] + " " + weight[3, 0]);
        stream.WriteLine(weight[0, 1] + " " + weight[1, 1] + " " + weight[2, 1] + " " + weight[3, 1]);
        stream.Close();
    }

    public void ReadWeights()
    {
        if (File.Exists(Names.SaveWeightsPath_ + Names.SaveExt))
        {
            StreamReader stream = new StreamReader(Names.SaveWeightsPath_ + Names.SaveExt);

            string w = stream.ReadLine();
            string[] w0 = w.Split(' ');
            for (int i = 0; i < w0.Length; i++)
            {
                weight[i, 0] = float.Parse(w0[i]);
            }

            w = stream.ReadLine();
            string[] w1 = w.Split(' ');
            for (int i = 0; i < w1.Length; i++)
            {
                weight[i, 1] = float.Parse(w1[i]);
            }

            stream.Close();
        }
        else
        {
            SetWeights();
        }
    }
}
