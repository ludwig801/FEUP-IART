using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Minimax : MonoBehaviour
{
//    // >> Heuristics <<
//    //
//    // > Features
//    //    > F1 : Max's distance to goal
//    //    > F2 : Difference between F1(Max) and F1(Min)
//    //    > F3 : Max's minimum moves to next column (closer to Min's border)
//    //    > F4 : Min's minimum moves to next column (closer to Max's border)
//    //
//    // > Evalutation Function
//    //      H(S) = W(Fi)*Fi(S) + Random;
//    //
//
//    [Range(1, 3)]
//    public int Depth;
//    public const int NumEvalFeatures = 4;
//    public Move BestMove;
//	
//    GameManager _gameManager;
//    Stack<Move> minimaxHistory;
//    int[] _distanceToNextRow;
//    int[] _distanceToObjective;
//    float[,] _weights;
//
//    void Start()
//    {
//        _gameManager = GetComponent<GameManager>();
//        minimaxHistory = new Stack<Move>();
//        _distanceToNextRow = new int[GameManager.NumPlayers];
//        _distanceToObjective = new int[GameManager.NumPlayers];
//        _weights = new float[NumEvalFeatures, GameManager.NumPlayers];
//
//        ReadWeights();
//    }
//
//    public bool RunAlgorithm()
//    {
//        minimaxHistory.Clear();
//
//        var root = new MinimaxNode(true);
//
//        return MinimaxAlphaBeta(root, Depth, _gameManager.CurrentPlayer);
//    }
//
//    bool MinimaxAlphaBeta(MinimaxNode node, int depth, int player)
//    {
//        if (depth <= 0 || _gameManager.IsFinalState())
//        {
//            node.heuristicValue = CalcHeuristicValue(_gameManager.CurrentPlayer);
//            return true;
//        }
//
//        // Assign Moves
//        List<Move> moves = _gameManager.GetPossibleMoves(player);
//        MinimaxNode child;
//
//        // Evaluate Moves
//        for (int i = 0; i < moves.Count; i++)
//        {
//            Move move = moves[i];
//
//            child = new MinimaxNode(!node.isMaximizer);
//
//            if (!_gameManager.PlayMove(minimaxHistory, move, player))
//                continue;
//
//            if (IsBoardValid())
//            {
//                child.alpha = node.alpha;
//                child.beta = node.beta;
//                child.move = move;
//
//                if (!MinimaxAlphaBeta(child, depth - 1, _gameManager.GetNextPlayer(player)))
//                    return false;
//
//                if (node.isMaximizer)
//                {
//                    if ((depth == Depth) && (child.heuristicValue > node.heuristicValue))
//                    {
//                        BestMove = move;
//                    }
//                    node.heuristicValue = Mathf.Max(node.heuristicValue, child.heuristicValue);
//                }
//                else
//                {
//                    node.heuristicValue = Mathf.Min(node.heuristicValue, child.heuristicValue);
//                }
//            }
//
//            if (!_gameManager.UndoMove(minimaxHistory.Pop(), player))
//                return false;
//
//            if (AlphaBetaCut(node, child))
//                break;
//        }
//
//        return true;
//    }
//
//    public bool IsBoardValid()
//    {
//        return AStarDistanceToGoal();
//    }
//
//    bool AlphaBetaCut(MinimaxNode node, MinimaxNode child)
//    {
//        if (node.isMaximizer)
//        {
//            node.alpha = Mathf.Max(node.alpha, node.heuristicValue);
//            if (node.alpha > node.beta)
//            {
//                return true;
//            }
//        }
//        else
//        {
//            node.beta = Mathf.Min(node.beta, node.heuristicValue);
//            if (node.alpha > node.beta)
//            {
//                return true;
//            }
//        }
//
//        return false;
//    }
//
//    float CalcHeuristicValue(int player)
//    {
//        // F3 & F4
//        AStarDistanceToNextRow();
//
//        int nextPlayer = _gameManager.GetNextPlayer(player);
//
//        float weightA = _weights[0, player];
//        float weightB = _weights[1, player];
//        float weightC = _weights[2, player];
//        float weightD = _weights[3, player];
//        int row = GetPlayerRowAbs(nextPlayer);
//
//        var heuristicValueA = weightA * (0.012000f) * (80 - _distanceToObjective[player]);
//        var heuristicValueB = weightB * (0.012000f) * (_distanceToObjective[nextPlayer] - 80);
//        var heuristicValueC = weightC * (0.111000f) * (8 - _distanceToNextRow[player]);
//        var heuristicValueD = weightD * (0.012321f) * (_distanceToNextRow[nextPlayer] - 8) * row;
//
//        return (heuristicValueA + heuristicValueB + heuristicValueC + heuristicValueD) * Random.Range(0.99f, 1.01f);
//    }
//
//    bool AStarDistanceToNextRow()
//    {
//        // First Player
//        var objective = _gameManager.GetPlayerPawnTile(0).Row + 1;
//        var retVal = new int[GameManager.NumPlayers];
//
//        retVal[0] = AStar(0, objective, true);
//        if (retVal[0] < 0)
//            return false;
//			
//        // Second Player
//        objective = _gameManager.GetPlayerPawnTile(1).Row - 1;
//        retVal[1] = AStar(1, objective, true);
//        if (retVal[1] < 0)
//            return false;
//
//        _distanceToNextRow[0] = retVal[0];
//        _distanceToNextRow[1] = retVal[1];
//
//        return true;
//    }
//
//    bool AStarDistanceToGoal()
//    {
//        var player = 0;
//        var objective = _gameManager.GameBoard.Border;
//        var retVal = new int[GameManager.NumPlayers];
//
//        retVal[player] = AStar(player, objective, false);
//        if (retVal[player] < 0)
//            return false;
//
//        player = 1;
//        objective = 0;
//        retVal[player] = AStar(player, objective, false);
//        if (retVal[player] < 0)
//            return false;
//
//        _distanceToObjective[0] = retVal[0];
//        _distanceToObjective[1] = retVal[1];
//
//        return true;
//    }
//
//    int AStar(int player, int objective, bool contemplatePawns)
//    {
//        var openList = new List<Tile>();
//        var closedList = new List<Tile>();
//        var start = _gameManager.GetPlayerPawnTile(player);
//        start.AStarCostValue = 0;
//        start.AStarHeuristicValue = AStarHeuristicValue(start, objective);
//        start.AStarPathParent = null;
//
//        openList.Add(start);
//
//        while (openList.Count > 0)
//        {
//            Tile bestChoice;
//            if (!GetAStarBestTile(openList, out bestChoice))
//                break;
//
//            // If objective reached
//            if ((player == 0 && bestChoice.Row >= objective) || (player == 1 && bestChoice.Row <= objective))
//                return AStarReconstruct(start, bestChoice);
//
//            // Remove current from the open list and add it to closed list
//            openList.Remove(bestChoice);
//            closedList.Add(bestChoice);
//
//            // Add current's neighbors to the open list
//            for (var i = 0; i < bestChoice.Edges.Count; i++)
//            {
//                var neighbor = bestChoice.Edges[i].GetNeighborOf(bestChoice);
//
//                if (!(closedList.Contains(neighbor) || (contemplatePawns && neighbor.HasPawn)))
//                {
//                    var neighborGPlusUne = bestChoice.AStarCostValue + 1;
//                    bool inOpenList = openList.Contains(neighbor);
//
//                    if (!inOpenList || neighborGPlusUne < neighbor.AStarCostValue)
//                    {
//                        neighbor.AStarPathParent = bestChoice;
//                        neighbor.AStarCostValue = neighborGPlusUne;
//                        neighbor.AStarHeuristicValue = AStarHeuristicValue(neighbor, objective);
//                        if (!inOpenList)
//                        {
//                            openList.Add(neighbor);
//                        }
//                    }
//                }
//            }
//        }
//
//        return -1;
//    }
//
//    int AStarReconstruct(Tile start, Tile current)
//    {
//        int movesCount = 0;
//
//        while (current != start)
//        {
//            movesCount++;
//            current = current.AStarPathParent;
//        }
//
//        return movesCount;
//    }
//
//    bool GetAStarBestTile(List<Tile> openList, out Tile bestTile)
//    {
//        bestTile = null;
//        for (int i = 0; i < openList.Count; i++)
//        {
//            if (bestTile == null || openList[i].AStarFunctionValue < bestTile.AStarFunctionValue)
//            {
//                bestTile = openList[i];
//            }
//        }
//
//        return bestTile != null;
//    }
//
//    int AStarHeuristicValue(Tile node, int objective)
//    {
//        return Mathf.Abs(node.Row - objective);
//    }
//
//    int GetPlayerRowAbs(int player)
//    {
//        return (player == 0 ? _gameManager.Players[0].Pawn.Tile.Row : _gameManager.GameBoard.Size - 1 - _gameManager.Players[0].Pawn.Tile.Row);
//    }
//
//    public void SetWeights(float[,] param = null)
//    {
//        if (param == null)
//        {
//            _weights[0, 0] = 3f;
//            _weights[1, 0] = 1f;
//            _weights[2, 0] = 0f;
//            _weights[3, 0] = 1f;
//
//            _weights[0, 1] = 3f;
//            _weights[1, 1] = 1f;
//            _weights[2, 1] = 0f;
//            _weights[3, 1] = 0f;
//        }
//        else
//        {
//            _weights = param;
//        }
//    }
//
//    public void SetWeight(int player, int evalFunctionIndex, float newWeight)
//    {
//        _weights[evalFunctionIndex, player] = newWeight;
//    }
//
//    public float GetWeight(int player, int evalFunctionIndex)
//    {
//        return _weights[evalFunctionIndex, player];
//    }
//
//    public void SaveWeights()
//    {
//        StreamWriter stream = new StreamWriter(Names.SaveWeightsPath_ + Names.SaveExt, false);
//        stream.WriteLine(_weights[0, 0] + " " + _weights[1, 0] + " " + _weights[2, 0] + " " + _weights[3, 0]);
//        stream.WriteLine(_weights[0, 1] + " " + _weights[1, 1] + " " + _weights[2, 1] + " " + _weights[3, 1]);
//        stream.Close();
//    }
//
//    void ReadWeights()
//    {
//        if (File.Exists(Application.dataPath + Names.SaveWeightsPath_ + Names.SaveExt))
//        {
//            StreamReader stream = new StreamReader(Names.SaveWeightsPath_ + Names.SaveExt);
//
//            string w = stream.ReadLine();
//            string[] w0 = w.Split(' ');
//            for (int i = 0; i < w0.Length; i++)
//            {
//                _weights[i, 0] = float.Parse(w0[i]);
//            }
//
//            w = stream.ReadLine();
//            string[] w1 = w.Split(' ');
//            for (int i = 0; i < w1.Length; i++)
//            {
//                _weights[i, 1] = float.Parse(w1[i]);
//            }
//
//            stream.Close();
//        }
//        else
//        {
//            SetWeights();
//        }
//    }
}
