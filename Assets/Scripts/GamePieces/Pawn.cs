using UnityEngine;
using System.Collections;

public class Pawn : Piece
{
    public Color[] Colors;

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    public Tile Tile
    {
        get
        {
            return _tile;
        }

        set
        {
            _tile = value;
            if (_tile != null)
            {
                visible = true;
                transform.position = _tile.transform.position + new Vector3(0f, 0.5f, 0f);
                _tile.SetPawn(this);
            }
            else
            {
                visible = false;
            }
        }
    }

    void Start()
    {
        visible = (_tile != null);
        rendererComponent = GetComponent<Renderer>();
        rendererComponent.material.color = Colors[player];
    }
}
