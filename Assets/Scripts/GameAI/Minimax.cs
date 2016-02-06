using UnityEngine;
using System.Collections.Generic;
using System.IO;

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

    GameBoard _gameBoard;
    float[,] _weights;
    Move _bestMove;
    bool _running, _finished, _error;

    void Start()
    {
        _gameBoard = GameBoard.Instance;
        _weights = new float[4, _gameBoard.Players.Count];

//        ReadWeights();
        SetWeights();
        _running = false;
        _bestMove = null;
    }
        
    public void RunAlgorithm()
    {
        _bestMove = null;

        var root = new MinimaxNode(true);

        _error = false;
        _running = true;
        _finished = false;

        MinimaxAlphaBeta(root, Depth);
    }

    public bool IsRunning()
    {
        return _running;
    }

    public bool IsFinished()
    {
        return _finished && _bestMove != null;
    }

    public bool InErrorState()
    {
        return _error;
    }

    public object GetResult()
    {
        _finished = false;
        _running = false;
        _error = false;
        return _bestMove;
    }

    bool MinimaxAlphaBeta(MinimaxNode node, int depth)
    {
        // Terminal condition
        var isGameOver = _gameBoard.IsGameOver();
        if (depth <= 0 ||isGameOver)
        {
            node.HeuristicValue = CalculateHeuristicValue(_gameBoard.CurrentPlayer);
            if (isGameOver)
            {
                _running = false;
                _finished = true;  
            }
            return true;
        }

        var moves = _gameBoard.GetCurrentPossibleMoves();

        while(moves.Count > 0)
        {
            var move = moves.Dequeue();
            var child = new MinimaxNode(!node.IsMaximizer);

            if (!_gameBoard.PlayMove(move))
                continue;


            if (IsBoardValid())
            {
                child.Alpha = node.Alpha;
                child.Beta = node.Beta;
                child.AssociatedMove = move;

                if (!MinimaxAlphaBeta(child, depth - 1))
                {
                    _error = true;
                    return false;
                }

                if (node.IsMaximizer)
                {
                    if ((depth == Depth) && (child.HeuristicValue > node.HeuristicValue))
                        _bestMove = move;
                    node.HeuristicValue = Mathf.Max(node.HeuristicValue, child.HeuristicValue);
                }
                else
                {
                    node.HeuristicValue = Mathf.Min(node.HeuristicValue, child.HeuristicValue);
                }
            }

            if (!_gameBoard.UndoMove())
            {
                _error = true;
                return false;
            }

            if (AlphaBetaCut(node, child))
                break;
        }

        return true;
    }

    bool MinimaxAlphaBetaLinear()
    {
        var movesArray = new Stack<Queue<Move>>();
        var currentDepth = 1;
        var currentNode = new MinimaxNode(true);
        movesArray.Push(_gameBoard.GetCurrentPossibleMoves());

        while (true)
        {
            var depthMoves = movesArray.Peek();
            if (depthMoves.Count > 0)
            {
                var topMove = depthMoves.Dequeue();
                _gameBoard.PlayMove(topMove);

                movesArray.Push(_gameBoard.GetCurrentPossibleMoves());
                currentDepth++;
            }
            else
            {
                movesArray.Pop();
                _gameBoard.UndoMove();
                currentDepth--;
            }

            // TODO : alphabeta cut

            // Terminal condition
            if (currentDepth == Depth || _gameBoard.IsGameOver())
            {
                currentNode.HeuristicValue = CalculateHeuristicValue(_gameBoard.CurrentPlayer);
                _gameBoard.UndoMove();
                currentDepth--;

                // TODO: Evaluate alpha & beta
            }
        }

        _running = false;
        _finished = true;

        return true;
    }

    bool IsBoardValid()
    {
        return AStar.CalculateDistancesToObjective();
    }

    bool AlphaBetaCut(MinimaxNode node, MinimaxNode child)
    {
        if (node.IsMaximizer)
        {
            node.Alpha = Mathf.Max(node.Alpha, node.HeuristicValue);
            if (node.Alpha > node.Beta)
            {
                return true;
            }
        }
        else
        {
            node.Beta = Mathf.Min(node.Beta, node.HeuristicValue);
            if (node.Alpha > node.Beta)
            {
                return true;
            }
        }

        return false;
    }

    public float CalculateHeuristicValue(int player)
    {
        // F1 & F2
        // Global Objective
        if (!AStar.CalculateDistancesToObjective())
            return -1;

        // playerProgressToObjective = 1 - (playerDist / maxStepsToGoal)
        //  - Range: [0, 1]
        //  - The bigger the number, the closer the player is to its objective
        var playerDistanceToObjective = AStar.LastCalculatedResults[player];
        var maxStepsToGoal = _gameBoard.Size * _gameBoard.Size - 1f;
        var playerProgressToObjective = 1 - (playerDistanceToObjective / maxStepsToGoal);

        // playerProgressToObjectiveVsOpponent = (opponentDist - playerDist) / maxStepsToGoal
        //  - Range: [0, 1]
        //  - The bigger the number, the better is the player's progress, comparing to its opponent
        var opponent = _gameBoard.GetNextPlayer(player);
        var opponentDistanceToObjective = AStar.LastCalculatedResults[opponent];
        var playerProgressToObjectiveVsOpponent = (opponentDistanceToObjective - playerDistanceToObjective) / maxStepsToGoal;

        // F3 & F4
        // Local Objective
        if (!AStar.CalculateDistancesToNextRow())
            return -1;

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

        var heuristicValueA =  _weights[0, player] * playerProgressToObjective;
        var heuristicValueB =_weights[1, player] * playerProgressToObjectiveVsOpponent;
        var heuristicValueC = _weights[2, player] * playerProgressToNextRow;
        var heuristicValueD = _weights[2, player] * playerProgressToNextRowVsOpponent;

        HeuristicValue = (heuristicValueA + heuristicValueB + heuristicValueC + heuristicValueD) /** Random.Range(0.9f, 1.1f) */;

        return HeuristicValue;
    }

    int GetPlayerRowAbs(int player)
    {
        return (player == 0 ? _gameBoard.Players[0].Pawn.Tile.Row : _gameBoard.Border - _gameBoard.Players[0].Pawn.Tile.Row);
    }

    public void SetWeights(float[,] param = null)
    {
        if (param == null)
        {
            _weights[0, 0] = 3f;
            _weights[1, 0] = 1f;
            _weights[2, 0] = 0f;
            _weights[3, 0] = 1f;

            _weights[0, 1] = 3f;
            _weights[1, 1] = 1f;
            _weights[2, 1] = 0f;
            _weights[3, 1] = 0f;
        }
        else
        {
            _weights = param;
        }
    }

    void SetWeight(int player, int evalFunctionIndex, float newWeight)
    {
        _weights[evalFunctionIndex, player] = newWeight;
    }

    float GetWeight(int player, int evalFunctionIndex)
    {
        return _weights[evalFunctionIndex, player];
    }

    void SaveWeights()
    {
        StreamWriter stream = new StreamWriter(Names.WeightsPath_ + Names.SaveExt, false);
        stream.WriteLine(_weights[0, 0] + " " + _weights[1, 0] + " " + _weights[2, 0] + " " + _weights[3, 0]);
        stream.WriteLine(_weights[0, 1] + " " + _weights[1, 1] + " " + _weights[2, 1] + " " + _weights[3, 1]);
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
                _weights[i, 0] = float.Parse(w0[i]);
            }

            w = stream.ReadLine();
            string[] w1 = w.Split(' ');
            for (int i = 0; i < w1.Length; i++)
            {
                _weights[i, 1] = float.Parse(w1[i]);
            }

            stream.Close();
        }
        else
        {
            SetWeights();
        }
    }
}
