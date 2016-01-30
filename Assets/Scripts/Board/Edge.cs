using UnityEngine;
using System.Collections.Generic;

public class Edge : MonoBehaviour
{
    public Tile A, B;
    public bool Free, Active;

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

    void Update()
    {
        transform.position = Vector3.Lerp(A.transform.position, B.transform.position, 0.5f);
        var delta = (B.transform.position - A.transform.position);
        transform.localScale = new Vector3(0.4f, 0.5f * delta.magnitude, 0.4f);
        transform.localRotation = Quaternion.LookRotation(delta);
        transform.Rotate(new Vector3(1, 0, 0), 90);
        MeshRenderer.material.color = Active ? Color.green : Color.red;
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
