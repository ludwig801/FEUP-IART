using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsCpu;
    public Color Color;
    public int Walls;
    public Pawn Pawn;
    public int ObjectiveRow;

    void Start()
    {
        Pawn = GetComponentInChildren<Pawn>();
    }
}
