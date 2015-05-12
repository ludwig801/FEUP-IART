using UnityEngine;
using System.Collections;

public class Pawn : Piece
{
    public Color[] Colors;

    public Tile Tile
    {
        get
        {
            return this.tile;
        }

        set
        {
            tile = value;
            if (tile != null)
            {
                visible = true;
                transform.position = tile.transform.position + new Vector3(0f, 0.5f, 0f);
                tile.SetPawn(this);
            }
            else
            {
                visible = false;
            }
        }
    }

    void Start()
    {
        visible = (tile != null);
        rendererComponent = GetComponent<Renderer>();
        rendererComponent.material.color = Colors[player];
    }
}
