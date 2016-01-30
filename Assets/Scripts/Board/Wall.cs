using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class Wall : MonoBehaviour
{
    public Tile Tile;
    public bool Horizontal, Free;

    Vector3 _offset;

    void Start()
    {
        var gameBoard = GameBoard.Instance;
        _offset = new Vector3(0.5f * (gameBoard.TileSize + gameBoard.TileSpacingFactor * gameBoard.TileSize), 0,
            -0.5f * (gameBoard.TileSize + gameBoard.TileSpacingFactor * gameBoard.TileSize));
    }

    public void Update()
    {
        transform.position = Tile.transform.position + _offset;
        transform.rotation = Quaternion.Euler(0, Horizontal ? 0 : 90, 0);
    }
}
