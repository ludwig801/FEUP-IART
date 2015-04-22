using UnityEngine;
using System.Collections;

public class MinimaxNode
{
    public bool isMaximizer;
    public float alpha, beta;
    public float heuristicValue;

    public MinimaxNode(bool isMaximizer)
    {
        this.isMaximizer = isMaximizer;
        this.alpha = float.MinValue;
        this.beta = float.MaxValue;
        this.heuristicValue = isMaximizer ? float.MinValue : float.MaxValue;
    }
}
