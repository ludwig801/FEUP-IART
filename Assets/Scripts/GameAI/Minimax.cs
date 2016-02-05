using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Minimax : MonoBehaviour, IAlgorithm
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

    GameBoard _gameBoard;
    float[,] _weights;
    Move _bestMove;
    bool _running, _finished, _error;

    void Start()
    {
        _gameBoard = GameBoard.Instance;
        _weights = new float[4, _gameBoard.Players.Count];

        ReadWeights();
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
        if (depth <= 0 || _gameBoard.IsGameOver())
        {
            node.heuristicValue = CalcHeuristicValue(_gameBoard.CurrentPlayer);
            _running = false;
            _finished = true;
            return true;
        }

        // Assign Moves
        var moves = _gameBoard.GetPossibleMoves();
 
        // Evaluate Moves
        for (int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            var child = new MinimaxNode(!node.isMaximizer);

            if (!_gameBoard.PlayMove(move))
                continue;

            if (IsBoardValid())
            {
                child.alpha = node.alpha;
                child.beta = node.beta;
                child.move = move;
                if (!MinimaxAlphaBeta(child, depth - 1))
                {
                    _error = true;
                    return false;
                }

                if (node.isMaximizer)
                {
                    if ((depth == Depth) && (child.heuristicValue > node.heuristicValue))
                    {
                        _bestMove = move;
                    }
                    node.heuristicValue = Mathf.Max(node.heuristicValue, child.heuristicValue);
                }
                else
                {
                    node.heuristicValue = Mathf.Min(node.heuristicValue, child.heuristicValue);
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

    public bool IsBoardValid()
    {
        return AStar.CalculateDistancesToObjective();
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

    float CalcHeuristicValue(int player)
    {
        // F3 & F4
        if (!AStar.CalculateDistancesToNextRow())
            return -1;

        var nextPlayer = _gameBoard.GetNextPlayer();

        var weightA = _weights[0, player];
        var weightB = _weights[1, player];
        var weightC = _weights[2, player];
        var weightD = _weights[3, player];
        var row = GetPlayerRowAbs(nextPlayer);

        var heuristicValueA = weightA * (0.012000f) * (80 - AStar.LastResult[player]);
        var heuristicValueB = weightB * (0.012000f) * (AStar.LastResult[nextPlayer] - 80);
        var heuristicValueC = weightC * (0.111000f) * (8 - AStar.LastResult[player]);
        var heuristicValueD = weightD * (0.012321f) * (AStar.LastResult[nextPlayer] - 8) * row;

        return (heuristicValueA + heuristicValueB + heuristicValueC + heuristicValueD) * Random.Range(0.99f, 1.01f);
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

    public void SetWeight(int player, int evalFunctionIndex, float newWeight)
    {
        _weights[evalFunctionIndex, player] = newWeight;
    }

    public float GetWeight(int player, int evalFunctionIndex)
    {
        return _weights[evalFunctionIndex, player];
    }

    public void SaveWeights()
    {
        StreamWriter stream = new StreamWriter(Names.SaveWeightsPath_ + Names.SaveExt, false);
        stream.WriteLine(_weights[0, 0] + " " + _weights[1, 0] + " " + _weights[2, 0] + " " + _weights[3, 0]);
        stream.WriteLine(_weights[0, 1] + " " + _weights[1, 1] + " " + _weights[2, 1] + " " + _weights[3, 1]);
        stream.Close();
    }

    void ReadWeights()
    {
        if (File.Exists(Application.dataPath + Names.SaveWeightsPath_ + Names.SaveExt))
        {
            StreamReader stream = new StreamReader(Names.SaveWeightsPath_ + Names.SaveExt);

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
