using UnityEngine;
using System.Collections;

public class Node
{
	public bool isMaximizer;
	public float alpha, beta;
	public float heuristicValue;

	public Node(bool isMaximizer)
	{
		this.isMaximizer = isMaximizer;
		this.alpha = float.MinValue;
		this.beta = float.MaxValue;
		this.heuristicValue = isMaximizer ? float.MinValue : float.MaxValue;
	}
}
