using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class Minimax : MonoBehaviour, IBestMoveAlgorithm
{
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
    [Range(1, 3)]
    public int Depth;
    public AStar AStar;
    public float HeuristicValue;
    [Range(5, 15)]
    public int MaxAlgorithmTime;
    public float[,] HeuristicFunctionsWeights;

    int MaxAlgorithmTimeInMilliseconds
    {
        get
        {
            return MaxAlgorithmTime * 1000;
        }
    }

    System.Diagnostics.Stopwatch _timer;
    GameBoard _gameBoard;
    float _bestMoveValue;
    Move _bestMove;
    [SerializeField]
    bool _running, _finished;

    void Start()
    {
        _gameBoard = GameBoard.Instance;
        SetWeights();

        _running = false;
        _bestMove = null;
    }

    public void RunAlgorithm(GameBoard gameBoard)
    {
        _gameBoard = gameBoard;

        SetWeights();
        _bestMoveValue = float.MinValue;
        _bestMove = null;

        _running = true;
        _finished = false;

        _timer = new System.Diagnostics.Stopwatch();
        _timer.Start();

        MaxAlgorithmTime = Depth * 5;
        MinimaxAlphaBeta(_gameBoard, 0, float.MinValue, float.MaxValue, true, _gameBoard.CurrentPlayer);

        _running = false;
        _finished = true;

        _timer.Stop();
        //Debug.Log("Recursive algorithm took [" + _timer.ElapsedMilliseconds + "]ms to complete");
    }

    float MinimaxAlphaBeta(GameBoard board, int currentDepth, float alpha, float beta, bool isMaximizer, int player)
    {
        if (currentDepth == Depth || board.CheckGameOver())
            return CalculateHeuristicValue(player);

        if(_timer.ElapsedMilliseconds > MaxAlgorithmTimeInMilliseconds)
            return CalculateHeuristicValue(player);

        var moves = board.GetCurrentPossibleMoves();
        var bestValue = isMaximizer ? float.MinValue : float.MaxValue;
        foreach (var move in moves)
        {
            if (board.PlayMove(move))
            {
                if (isMaximizer)
                    bestValue = Mathf.Max(bestValue, MinimaxAlphaBeta(board, currentDepth + 1, alpha, beta, false, player));
                else
                    bestValue = Mathf.Min(bestValue, MinimaxAlphaBeta(board, currentDepth + 1, alpha, beta, true, player));
                
                CheckIfBestMove(bestValue, currentDepth, move);

                if (isMaximizer)
                    alpha = Mathf.Max(alpha, bestValue);
                else
                    beta = Mathf.Min(beta, bestValue);

                board.UndoMove();

                if (beta <= alpha)
                    break;
            }
        }

        return bestValue;
    }

    void CheckIfBestMove(float heuristicValue, int currentDepth, Move currentMove)
    {
        if (currentDepth != 0)
            return;

        if (_bestMoveValue < heuristicValue)
        {
            //Debug.Log("Found a best move!");
            _bestMoveValue = heuristicValue;
            _bestMove = currentMove;
        }
    }

    void CalculateHeuristicAlphaBeta(MinimaxNode node, float childHeuristicValue)
    {
        if (node.IsMaximizer)
        {
            node.HeuristicValue = Mathf.Max(node.HeuristicValue, childHeuristicValue);
            node.Alpha = Mathf.Max(node.Alpha, node.HeuristicValue);
        }
        else
        {
            node.HeuristicValue = Mathf.Min(node.HeuristicValue, childHeuristicValue);
            node.Beta = Mathf.Min(node.Beta, node.HeuristicValue);
        }
    }

    bool AlphaBetaCut(MinimaxNode node)
    {
        return node.Beta <= node.Alpha;
    }

    public float CalculateHeuristicValue(int player)
    {
        // F1 & F2
        // Global Objective
        if (!AStar.CalculateDistancesToObjective())
            return 0;

        // playerProgressToObjective = 1 - (playerDist / maxStepsToGoal)
        //  - Range: [0, 1]
        //  - The bigger the number, the closer the player is to its objective
        var playerDistanceToObjective = AStar.LastCalculatedResults[player];
        if (playerDistanceToObjective <= 0)
            return 100;
        var maxStepsToGoal = (_gameBoard.Size * _gameBoard.Size) - 1f;
        var playerProgressToObjective = 1 - (playerDistanceToObjective / maxStepsToGoal);

        // playerProgressToObjectiveVsOpponent = (opponentDist - playerDist) / maxStepsToGoal
        //  - Range: [0, 1]
        //  - The bigger the number, the better is the player's progress, comparing to its opponent
        var opponent = _gameBoard.GetNextPlayer(player);
        var opponentDistanceToObjective = AStar.LastCalculatedResults[opponent];
        if (opponentDistanceToObjective <= 0)
            return -100;
        var playerProgressToObjectiveVsOpponent = (opponentDistanceToObjective - playerDistanceToObjective) / maxStepsToGoal;

        // F3 & F4
        // Local Objective
        if (!AStar.CalculateDistancesToNextRow())
            return 0;

        // playerProgressToNextRow = 1 - (playerStepsToNextRow / maxStepsToNextRow)
        //  - Range: [0, 1]
        //  - The bigger the number, the closest the player is to the objective row
        var maxStepsToNextRow = _gameBoard.Size - 1f;

        var playerDistanceToNextRow = AStar.LastCalculatedResults[player];
        var playerProgressToNextRow = 1 - (playerDistanceToNextRow / maxStepsToNextRow);

        // playerProgressToNextRowVsOpponent = (opponentDistToNextRow - playerDistToNextRow) / maxStepsToNextRow
        //  - Range: [0, 1]
        //  - The bigger the number, the better is the player's local progress, comparing to its opponent
        var opponentDistanceToNextRow = AStar.LastCalculatedResults[opponent];
        var playerProgressToNextRowVsOpponent = (opponentDistanceToNextRow - playerDistanceToNextRow) / maxStepsToNextRow;

        var heuristicValueA = HeuristicFunctionsWeights[0, player] * playerProgressToObjective;
        var heuristicValueB = HeuristicFunctionsWeights[1, player] * playerProgressToObjectiveVsOpponent;
        var heuristicValueC = HeuristicFunctionsWeights[2, player] * playerProgressToNextRow;
        var heuristicValueD = HeuristicFunctionsWeights[2, player] * playerProgressToNextRowVsOpponent;

        HeuristicValue = (heuristicValueA + heuristicValueB + heuristicValueC + heuristicValueD) * Random.Range(0.96f, 1f);

        return HeuristicValue;
    }

    int GetPlayerRowAbs(int player)
    {
        return (player == 0 ? _gameBoard.Players[0].Pawn.Tile.Row : _gameBoard.Border - _gameBoard.Players[0].Pawn.Tile.Row);
    }

    public bool IsRunning()
    {
        return _running;
    }

    public bool IsFinished()
    {
        return _finished;
    }
        
    public Move GetResult()
    {
        _finished = false;
        return _bestMove;
    }

    public void SetWeights(float[,] param = null)
    {
        if (param == null)
        {
            HeuristicFunctionsWeights = new float[4, _gameBoard.Players.Count];

            HeuristicFunctionsWeights[0, 0] = 1.00f;
            HeuristicFunctionsWeights[1, 0] = 0.75f;
            HeuristicFunctionsWeights[2, 0] = 0.00f;
            HeuristicFunctionsWeights[3, 0] = 0.25f;

            HeuristicFunctionsWeights[0, 1] = 1.00f;
            HeuristicFunctionsWeights[1, 1] = 0.75f;
            HeuristicFunctionsWeights[2, 1] = 0.00f;
            HeuristicFunctionsWeights[3, 1] = 0.25f;
        }
        else
        {
            HeuristicFunctionsWeights = param;
        }
    }

    void SetWeight(int player, int evalFunctionIndex, float newWeight)
    {
        HeuristicFunctionsWeights[evalFunctionIndex, player] = newWeight;
    }

    float GetWeight(int player, int evalFunctionIndex)
    {
        return HeuristicFunctionsWeights[evalFunctionIndex, player];
    }

    void SaveWeights()
    {
        StreamWriter stream = new StreamWriter(Names.WeightsPath_ + Names.SaveExt, false);
        stream.WriteLine(HeuristicFunctionsWeights[0, 0] + " " + HeuristicFunctionsWeights[1, 0] + " " + HeuristicFunctionsWeights[2, 0] + " " + HeuristicFunctionsWeights[3, 0]);
        stream.WriteLine(HeuristicFunctionsWeights[0, 1] + " " + HeuristicFunctionsWeights[1, 1] + " " + HeuristicFunctionsWeights[2, 1] + " " + HeuristicFunctionsWeights[3, 1]);
        stream.Close();
    }

    void ReadWeights()
    {
        if (File.Exists(Application.dataPath + Names.WeightsPath_ + Names.SaveExt))
        {
            StreamReader stream = new StreamReader(Names.WeightsPath_ + Names.SaveExt);

            string w = stream.ReadLine();
            string[] w0 = w.Split(' ');
            for (int i = 0; i < w0.Length; i++)
            {
                HeuristicFunctionsWeights[i, 0] = float.Parse(w0[i]);
            }

            w = stream.ReadLine();
            string[] w1 = w.Split(' ');
            for (int i = 0; i < w1.Length; i++)
            {
                HeuristicFunctionsWeights[i, 1] = float.Parse(w1[i]);
            }

            stream.Close();
        }
        else
        {
            SetWeights();
        }
    }
}
