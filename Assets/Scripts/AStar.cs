using UnityEngine;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
    public int[] LastResult;

    GameBoard _gameBoard;
    Tile _startingTile;
    int _objectiveRow, _output;
    bool _contemplatePawns;

    void Start()
    {
        _gameBoard = GameBoard.Instance;
    }

    public bool CalculateDistancesToNextRow()
    {
        LastResult = new int[_gameBoard.Players.Count];

        for (var i = 0; i < _gameBoard.Players.Count; i++)
        {
            _startingTile =  _gameBoard.Players[i].Pawn.Tile;
            _objectiveRow = _startingTile.Row + (_startingTile.Row < _gameBoard.Players[i].ObjectiveRow ? 1 : -1);
            _contemplatePawns = true;

            if (RunAlgorithm())
                LastResult[i] = _output;
            else
                return false;
        }

        return true;
    }

    public bool CalculateDistancesToObjective()
    {
        LastResult = new int[_gameBoard.Players.Count];

        for (var i = 0; i < _gameBoard.Players.Count; i++)
        {
            _startingTile = _gameBoard.Players[i].Pawn.Tile;
            _objectiveRow = _gameBoard.Players[i].ObjectiveRow;
            _contemplatePawns = false;

            if (RunAlgorithm())
                LastResult[i] = _output;
            else
                return false;
        }

        return true;
    }

    bool RunAlgorithm()
    {
        var openList = new List<Tile>();
        var closedList = new List<Tile>();
        var startingTile = _startingTile;
        startingTile.AStarCostValue = 0;
        startingTile.AStarHeuristicValue = AStarHeuristicValue(startingTile, _objectiveRow);
        startingTile.AStarPathParent = null;

        openList.Add(startingTile);

        while (openList.Count > 0)
        {
            Tile bestChoice;

            // Some error occurred
            if (!GetAStarBestTile(openList, out bestChoice))
                break;

            // If objective reached
            if (bestChoice.Row == _objectiveRow)
            {
                _output = AStarPathCost(startingTile, bestChoice);
                return (_output >= 0);
            }

            openList.Remove(bestChoice);
            closedList.Add(bestChoice);

            // Add current's neighbors to the open list
            for (var i = 0; i < bestChoice.Edges.Count; i++)
            {
                var neighbor = bestChoice.Edges[i].GetNeighborOf(bestChoice);

                if (!(closedList.Contains(neighbor) || (_contemplatePawns && neighbor.Occupied)))
                {
                    var neighborCostPlusOne = bestChoice.AStarCostValue + 1;
                    bool inOpenList = openList.Contains(neighbor);

                    if (!inOpenList || neighborCostPlusOne < neighbor.AStarCostValue)
                    {
                        neighbor.AStarPathParent = bestChoice;
                        neighbor.AStarCostValue = neighborCostPlusOne;
                        neighbor.AStarHeuristicValue = AStarHeuristicValue(neighbor, _objectiveRow);
                        if (!inOpenList)
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
        }

        return false;
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

        return (bestTile != null);
    }

    int AStarHeuristicValue(Tile node, int objective)
    {
        return Mathf.Abs(node.Row - objective);
    }

    int AStarPathCost(Tile start, Tile current)
    {
        int movesCount = 0;

        while (current != start)
        {
            movesCount++;
            current = current.AStarPathParent;
        }

        return movesCount;
    }
}
