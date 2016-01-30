using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public List<Edge> Edges;
    public int Row, Col;
    public int AStarCostValue;
    public int AStarHeuristicValue;
    public int AStarFunctionValue { get { return AStarCostValue + AStarHeuristicValue; } }
    public Tile AStarPathParent;
    public bool Occupied;

    public bool AboveTo(Tile other)
    {
        return Col == other.Col && Row == other.Row + 1;
    }

    public bool BelowTo(Tile other)
    {
        return Col == other.Col && Row == other.Row - 1;
    }

    public bool RightTo(Tile other)
    {
        return Row == other.Row && Col == other.Col + 1;
    }

    public bool LeftTo(Tile other)
    {
        return Row == other.Row && Col == other.Col - 1;
    }

    public bool IsNeighborOf(Tile other)
    {
        foreach (var edge in Edges)
        {
            if (edge.Active && edge.Connects(this, other))
                return true;
        }

        return false;
    }

    public bool CanMoveTo(Tile src)
    {
        if (Occupied)
            return false;

        foreach (Edge edge in Edges)
        {
            if (edge.Connects(this, src) && edge.Active)
                return true; 
        }

        return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameBoard.Instance.OnAction(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameBoard.Instance.OnTileEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameBoard.Instance.OnTileExit(this);
    }

    public override string ToString()
    {
        return "[" + Row + " " + Col + "]";
    }

    // Class Methods
    public static bool SameRow(Tile a, Tile b)
    {
        return (a.Row == b.Row);
    }

    public static bool SameCol(Tile a, Tile b)
    {
        return (a.Col == b.Col);
    }

    public static float Distance(Tile a, Tile b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.Row - b.Row, 2) + Mathf.Pow(a.Col - b.Col, 2));
    }
}
