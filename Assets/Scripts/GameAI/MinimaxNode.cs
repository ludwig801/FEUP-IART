using UnityEngine;
using System.Collections;

public class MinimaxNode
{
    public bool IsMaximizer;
    public float Alpha, Beta;
    public float HeuristicValue;
    public Move AssociatedMove;

    public MinimaxNode(bool isMaximizer)
    {
        IsMaximizer = isMaximizer;
        Alpha = float.MinValue;
        Beta = float.MaxValue;
        HeuristicValue = isMaximizer ? float.MinValue : float.MaxValue;
    }

    public override string ToString()
    {
        return string.Format("H: " + HeuristicValue + " (" + Alpha + ", " + Beta + ")");
    } 
}
