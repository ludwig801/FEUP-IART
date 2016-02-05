using UnityEngine;
using System.Collections.Generic;

public class Edge : MonoBehaviour
{
    public Color ColorActive, ColorInactive;
    [Range(0, 10)]
    public int Speed;
    [Range(0.1f, 1)]
    public float Thickness;
    [Range(0, 2)]
    public float Height;
    public Tile A, B;
    public bool Free, Active;

    Material _material;
    Vector3 _offset;

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(A.transform.position, B.transform.position, 0.5f) + new Vector3(0, Height, 0);
        var delta = (B.transform.position - A.transform.position);
        transform.localScale = new Vector3(Thickness, 0.45f * delta.magnitude * 0.4f, Thickness);
        transform.localRotation = Quaternion.LookRotation(delta);
        transform.Rotate(new Vector3(1, 0, 0), 90);
        _material.color = Speed > 0 ? Color.Lerp(_material.color, Active ? ColorActive : ColorInactive, Speed * Time.deltaTime) : Active ? ColorActive : ColorInactive;
    }

    public bool Connects(Tile a, Tile b)
    {
        return (a == A && b == B) || (a == B && b == A);
    }

    public Tile GetNeighborOf(Tile tile)
    {
        if (A == tile)
        {
            return B;
        }
        else if(B == tile)
        {
            return A;
        }

        Debug.LogWarning("Requested neighbor of tile which does NOT belong to this edge. Returning null...");
        return null;
    }
}
