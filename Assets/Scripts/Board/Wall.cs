using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class Wall : MonoBehaviour
{
    public Color ColorDefault, ColorInvalid, ColorInactive; 
    [Range(0, 10)]
    public int TransitionSpeed;
    public Tile Tile;
    public bool Horizontal, Free, Invalid;

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    Vector3 _offset;
    Material _material;

    void Start()
    {
        var gameBoard = GameBoard.Instance;
        _offset = new Vector3(0.5f * (gameBoard.TileSize + gameBoard.TileSpacingFactor * gameBoard.TileSize), 0,
            -0.5f * (gameBoard.TileSize + gameBoard.TileSpacingFactor * gameBoard.TileSize));

        _material = GetComponent<MeshRenderer>().material;
    }

    public void Update()
    {
        if (HasTile)
        {
            _material.color = TransitionSpeed > 0 ? Color.Lerp(_material.color, Invalid ? ColorInvalid : ColorDefault, Time.deltaTime * TransitionSpeed) : Invalid ? ColorInvalid : ColorDefault;
            transform.position = Tile.transform.position + _offset;
            transform.rotation = Quaternion.Euler(0, Horizontal ? 0 : 90, 0);
        }
        else
        {
            _material.color = TransitionSpeed > 0 ? Color.Lerp(_material.color, ColorInactive, Time.deltaTime * TransitionSpeed) : ColorInactive;  
        }
    }
}
