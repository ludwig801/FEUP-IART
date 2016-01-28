using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
	public int player;
	protected Tile _tile;

	public bool visible;
	protected Renderer rendererComponent;

	// Properties

	// Methods
	void Update()
	{
		if(rendererComponent != null)
		{
			rendererComponent.enabled = visible;
		}
	}

}
