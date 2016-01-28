using UnityEngine;
using System.Collections;

public class Wall : Piece
{
    private bool horizontal;
    private Transform wallTransform;

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
                _tile.SetWall(this);
            }
            else
            {
                transform.position = new Vector3(0f, 0.5f, 0f);
                visible = false;
            }
        }
    }

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    public bool Horizontal
    {
        get
        {
            return horizontal;
        }

        set
        {
            horizontal = value;
            if (!horizontal)
            {
                wallTransform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                wallTransform.rotation = Quaternion.identity;
            }
        }
    }

    public bool Vertical
    {
        get
        {
            return !horizontal;
        }

        set
        {
            Horizontal = !value;
        }
    }

    public bool Free
    {
        get
        {
            return (_tile == null);
        }
    }

    public void Init()
    {
        visible = false;
        horizontal = true;
        rendererComponent = transform.GetChild(0).GetComponent<Renderer>();
        wallTransform = transform.GetChild(0);
    }
}
