using UnityEngine;
using System.Collections;

public class Pawn : MonoBehaviour
{
    public Player Player;
    public Tile Tile;
    [Range(0, 10)]
    public int Speed;

    public MeshRenderer MeshRenderer
    {
        get
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }
            return _meshRenderer;
        }
    }

    MeshRenderer _meshRenderer;

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    void Update()
    {
        MeshRenderer.enabled = HasTile;
        if (HasTile)
        {
            MeshRenderer.material.color = Player.Color;
            if (Speed == 0)
            {
                transform.position = Tile.transform.position + new Vector3(0, 0.5f, 0); 
            }
            else
            {
                var objective = Tile.transform.position + new Vector3(0, 0.5f, 0);
                transform.position = Vector3.Lerp(transform.position, objective, Speed * Time.deltaTime);
            }
        }
    }
}
