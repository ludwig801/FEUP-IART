using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    public Color ColorDefault, ColorFocused, ColorHighlighted, ColorOccupied, ColorSelected, ColorObjective;
    [Range(0, 10)]
    public float AnimDuration;
    [Range(0f, 1.5f)]
    public float Height;
    public int Row, Col;
    public bool Occupied, Focused, Selected, Highlighted, Objective;
    public int AStarCostValue, AStarHeuristicValue;
    public int AStarFunctionValue { get { return AStarCostValue + AStarHeuristicValue; } }
    public Tile AStarPathParent;
    public List<Edge> Edges;

    private Material _material;

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;

        StartCoroutine(UpdateColor());
    }

    IEnumerator UpdateColor()
    {
        var colorDeltaTime = 0f;

        while (true)
        {
            var targetColor = Focused ? ColorFocused :
                Selected ? ColorSelected :
                Occupied ? ColorOccupied :
                Highlighted ? ColorHighlighted :
                Objective ? ColorObjective :
                ColorDefault;

            if (!Utils.IsColorLike(_material.color, targetColor))
            {
                colorDeltaTime += Time.deltaTime;
                _material.color = Color.Lerp(_material.color, targetColor, AnimDuration > 0 ? Mathf.Clamp01(colorDeltaTime / AnimDuration) : 1);
            }
            else
            {
                colorDeltaTime = 0;
            }

            yield return null;
        }
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

    public bool IsValidMoveTarget(Tile src)
    {
        return !Occupied && IsNeighborOf(src);
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
