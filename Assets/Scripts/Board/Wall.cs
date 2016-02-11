using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class Wall : MonoBehaviour
{
    public Color ColorDefault, ColorInvalid, ColorInactive; 
    [Range(0, 10)]
    public int Speed;
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
        _offset = new Vector3(0.5f * (gameBoard.TileSize + gameBoard.TileSpacing * gameBoard.TileSize), 0.5f,
            -0.5f * (gameBoard.TileSize + gameBoard.TileSpacing * gameBoard.TileSize));

        _material = GetComponent<MeshRenderer>().material;
    }

    public void Update()
    {
        if (HasTile)
        {
            _material.color = Speed > 0 ? Color.Lerp(_material.color, Invalid ? ColorInvalid : ColorDefault, Time.deltaTime * Speed) : Invalid ? ColorInvalid : ColorDefault;
            var objective = Tile.transform.position + _offset;
            transform.position = Speed > 0 ? Vector3.Lerp(transform.position, objective, Speed * Time.deltaTime) : objective;
            var objectiveRotation = Quaternion.Euler(0, Horizontal ? 0 : 90, 0);
            transform.rotation = Speed > 0 ? Quaternion.Lerp(transform.rotation, objectiveRotation, Speed * Time.deltaTime) : objectiveRotation;
        }
        else
        {
            _material.color = Speed > 0 ? Color.Lerp(_material.color, ColorInactive, Time.deltaTime * Speed) : ColorInactive;  
        }
    }
}
