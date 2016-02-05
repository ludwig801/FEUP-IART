using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    public Color ColorDefault, ColorHighlighted, ColorOccupied, ColorSelected, ColorObjective;
    [Range(0, 10f)]
    public float TransitionsSpeed;
    public List<Edge> Edges;
    public int Row, Col;
    [Range(0.5f, 1.5f)]
    public float Height;
    public bool Occupied, Selected, Highlighted, Objective;
    public int AStarCostValue, AStarHeuristicValue;
    public int AStarFunctionValue { get { return AStarCostValue + AStarHeuristicValue; } }
    public Tile AStarPathParent;

    Material _material;

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        _material.color = Color.Lerp(_material.color,
            Selected ? ColorSelected :
            Occupied ? ColorOccupied :
            Highlighted ? ColorHighlighted :
            Objective ? ColorObjective :
            ColorDefault, TransitionsSpeed > 0 ? Time.deltaTime * TransitionsSpeed : 1);
    }

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
