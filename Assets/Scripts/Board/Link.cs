using UnityEngine;
using System.Collections;

public class Link : MonoBehaviour
{
    public Tile tileA;
    public Tile tileB;

    private Renderer meshRenderer;
    private bool visible;

    public bool Visible
    {
        get
        {
            return visible;
        }

        set
        {
            visible = value;
            meshRenderer.enabled = value;
        }
    }

    public Tile TileA
    {
        get
        {
            return tileA;
        }
    }

    public Tile TileB
    {
        get
        {
            return tileB;
        }
    }

    public bool Free
    {
        get
        {
            return (tileA == null && tileB == null);
        }
    }

    public void Init()
    {
        meshRenderer = GetComponent<Renderer>();
        Visible = false;
    }

    public void SetTiles(Tile a, Tile b)
    {
        this.tileA = a;
        this.tileB = b;
        this.transform.position = Vector3.Lerp(a.transform.position, b.transform.position, 0.5f);

        Vector3 director = (b.transform.position - a.transform.position).normalized;
        float angleY = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(director, Vector3.right));
        if (a.row < b.row)
        {
            angleY = -angleY;
        }
        this.transform.rotation = Quaternion.Euler(new Vector3(0, angleY, 0));
    }

    public void RemoveTiles()
    {
        tileA = null;
        tileB = null;
    }
}
