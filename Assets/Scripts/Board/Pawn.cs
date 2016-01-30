using UnityEngine;
using System.Collections;

public class Pawn : MonoBehaviour
{
    public Player Player;
    public Tile Tile;
    [Range(0, 10)]
    public int Speed;

    Vector3 _offset;

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    void Start()
    {
        _offset = new Vector3(0, 0.5f, 0);
        GetComponent<MeshRenderer>().material.color = Player.Color;
    }

    void Update()
    {
        if (HasTile)
        {            
            var objective = Tile.transform.position + _offset;
            transform.position = Speed > 0 ? Vector3.Lerp(transform.position, objective, Speed * Time.deltaTime) : objective;
        }
    }
}
