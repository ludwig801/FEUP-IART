﻿using UnityEngine;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
    public int[] LastCalculatedResults
    {
        get;

        private set;
    }

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
        LastCalculatedResults = new int[_gameBoard.PlayersCount];

        for (var i = 0; i < _gameBoard.PlayersCount; i++)
        {
            var player = _gameBoard.GetPlayer(i);
            _startingTile =  player.Pawn.Tile;
            _objectiveRow = _startingTile.Row + (_startingTile.Row < player.ObjectiveRow ? 1 : -1);
            _contemplatePawns = true;

            if (RunAlgorithm())
                LastCalculatedResults[i] = _output;
            else
                return false;
        }

        return true;
    }

    public bool CalculateDistancesToObjective()
    {
        LastCalculatedResults = new int[_gameBoard.PlayersCount];

        for (var i = 0; i < _gameBoard.PlayersCount; i++)
        {
            var player = _gameBoard.GetPlayer(i);
            _startingTile = player.Pawn.Tile;
            _objectiveRow = player.ObjectiveRow;
            if (_startingTile.Row == _objectiveRow)
            {
                LastCalculatedResults[i] = 0;
            }
            else
            {
                _contemplatePawns = false;

                if (RunAlgorithm())
                    LastCalculatedResults[i] = _output;
                else
                    return false;
            }
        }

        return true;
    }

    bool RunAlgorithm()
    {
        var openList = new List<Tile>();
        var closedList = new List<Tile>();
        _startingTile.AStarCostValue = 0;
        _startingTile.AStarHeuristicValue = AStarHeuristicValue(_startingTile, _objectiveRow);
        _startingTile.AStarPathParent = null;

        openList.Add(_startingTile);

        while (openList.Count > 0)
        {
            Tile bestChoice;

            // Some error occurred
            if (!GetAStarBestTile(openList, out bestChoice))
                break;

            // If objective reached
            if (bestChoice.Row == _objectiveRow)
            {
                _output = AStarPathCost(_startingTile, bestChoice);
                return (_output > 0);
            }

            openList.Remove(bestChoice);
            closedList.Add(bestChoice);

            // Add current's neighbors to the open list
            foreach (var edge in bestChoice.Edges)
            {
                if (edge.Active)
                {
                    var neighbor = edge.GetNeighborOf(bestChoice);

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
