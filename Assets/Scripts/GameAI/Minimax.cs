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

    GameBoard _gameBoard;
    int[] _distanceToNextRow;
    int[] _distanceToObjective;
    float[,] _weights;
    Move _bestMove;
    bool _running;

    void Start()
    {
        _gameBoard = GameBoard.Instance;
        _distanceToNextRow = new int[_gameBoard.Players.Count];
        _distanceToObjective = new int[_gameBoard.Players.Count];
        _weights = new float[4, _gameBoard.Players.Count];

        ReadWeights();
        _running = false;
        _bestMove = null;
    }
        
    public void RunAlgorithm(GameBoard currentBoard)
    {
        _gameBoard = currentBoard;
        _bestMove = null;
        _distanceToNextRow = new int[_gameBoard.Players.Count];
        _distanceToObjective = new int[_gameBoard.Players.Count];

        var root = new MinimaxNode(true);

        _running = true;

        MinimaxAlphaBeta(root, Depth);
    }

    public bool IsAlgorithmRunning()
    {
        return _running;
    }

    public bool IsAlgorithmFinished()
    {
        return !IsAlgorithmRunning() && _bestMove != null;
    }

    bool MinimaxAlphaBeta(MinimaxNode node, int depth)
    {
        if (depth <= 0 || _gameBoard.IsGameOver())
        {
            node.heuristicValue = CalcHeuristicValue(_gameBoard.CurrentPlayer);
            _running = false;
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

            _gameBoard.NextTurn();

            if (IsBoardValid())
            {
                child.alpha = node.alpha;
                child.beta = node.beta;
                child.move = move;
                if (!MinimaxAlphaBeta(child, depth - 1))
                    return false;

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
                return false;

            _gameBoard.PreviousTurn();

            if (AlphaBetaCut(node, child))
                break;
        }

        return true;
    }

    public bool IsBoardValid()
    {
        return AStarDistanceToObjective();
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
        AStarDistanceToNextRow();

        var nextPlayer = _gameBoard.GetNextPlayer();

        var weightA = _weights[0, player];
        var weightB = _weights[1, player];
        var weightC = _weights[2, player];
        var weightD = _weights[3, player];
        var row = GetPlayerRowAbs(nextPlayer);

        var heuristicValueA = weightA * (0.012000f) * (80 - _distanceToObjective[player]);
        var heuristicValueB = weightB * (0.012000f) * (_distanceToObjective[nextPlayer] - 80);
        var heuristicValueC = weightC * (0.111000f) * (8 - _distanceToNextRow[player]);
        var heuristicValueD = weightD * (0.012321f) * (_distanceToNextRow[nextPlayer] - 8) * row;

        return (heuristicValueA + heuristicValueB + heuristicValueC + heuristicValueD) * Random.Range(0.99f, 1.01f);
    }

    bool AStarDistanceToNextRow()
    {
        for (var i = 0; i < _gameBoard.Players.Count; i++)
        {
            var playerPawnTile = _gameBoard.Players[i].Pawn.Tile;
            var localObjectiveRow = playerPawnTile.Row + (playerPawnTile.Row < _gameBoard.Players[i].ObjectiveRow ? 1 : -1);

            _distanceToNextRow[i] = AStar(i, localObjectiveRow, true);
            if (_distanceToNextRow[i] < 0)
                return false;
        }

        return true;
    }

    bool AStarDistanceToObjective()
    {
        for (var i = 0; i < _gameBoard.Players.Count; i++)
        {
            _distanceToObjective[i] = AStar(i, _gameBoard.Players[i].ObjectiveRow, false);
            if (_distanceToNextRow[i] < 0)
                return false;
        }

        return true;
    }

    int AStar(int player, int objective, bool contemplatePawns)
    {
        var openList = new List<Tile>();
        var closedList = new List<Tile>();
        var startingTile = _gameBoard.Players[player].Pawn.Tile;
        startingTile.AStarCostValue = 0;
        startingTile.AStarHeuristicValue = AStarHeuristicValue(startingTile, objective);
        startingTile.AStarPathParent = null;

        openList.Add(startingTile);

        while (openList.Count > 0)
        {
            Tile bestChoice;
            if (!GetAStarBestTile(openList, out bestChoice))
                break;

            // If objective reached
            if ((player == 0 && bestChoice.Row >= objective) || (player == 1 && bestChoice.Row <= objective))
                return AStarReconstruct(startingTile, bestChoice);

            // Remove current from the open list and add it to closed list
            openList.Remove(bestChoice);
            closedList.Add(bestChoice);

            // Add current's neighbors to the open list
            for (var i = 0; i < bestChoice.Edges.Count; i++)
            {
                var neighbor = bestChoice.Edges[i].GetNeighborOf(bestChoice);

                if (!(closedList.Contains(neighbor) || (contemplatePawns && neighbor.Occupied)))
                {
                    var neighborGPlusUne = bestChoice.AStarCostValue + 1;
                    bool inOpenList = openList.Contains(neighbor);

                    if (!inOpenList || neighborGPlusUne < neighbor.AStarCostValue)
                    {
                        neighbor.AStarPathParent = bestChoice;
                        neighbor.AStarCostValue = neighborGPlusUne;
                        neighbor.AStarHeuristicValue = AStarHeuristicValue(neighbor, objective);
                        if (!inOpenList)
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
        }

        return -1;
    }

    int AStarReconstruct(Tile start, Tile current)
    {
        int movesCount = 0;

        while (current != start)
        {
            movesCount++;
            current = current.AStarPathParent;
        }

        return movesCount;
    }

    bool GetAStarBestTile(List<Tile> openList, out Tile bestTile)
    {
        bestTile = null;
        for (int i = 0; i < openList.Count; i++)
        {
            if (bestTile == null || openList[i].AStarFunctionValue < bestTile.AStarFunctionValue)
            {
                bestTile = openList[i];
            }
        }

        return bestTile != null;
    }

    int AStarHeuristicValue(Tile node, int objective)
    {
        return Mathf.Abs(node.Row - objective);
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
