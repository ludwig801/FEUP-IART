using UnityEngine;
using System.Collections;

public class Pawn : MonoBehaviour
{
    public Player Player;
    public Tile Tile;
    [Range(0, 5)]
    public float MoveAnimation;
    [Range(0, 5)]
    public float FloatAnimation;
    [Range(0, 1)]
    public float MoveDeadZone;
    public float OffsetY;
    public float DeltaY;
    public bool Moving;
    public bool Floating;
    public bool OnTile;

    Vector3 _bottom;
    Vector3 _top;

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    void Start()
    {
        _bottom = new Vector3(0, OffsetY, 0);
        _top = new Vector3(0, OffsetY + DeltaY, 0);

        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.materials[1].color = Player.Color;
        meshRenderer.materials[2].color = Player.Color;

        StartCoroutine(UpdatePosition());
        StartCoroutine(UpdateOffsets());
    }

    IEnumerator UpdatePosition()
    {
        Moving = false;
        var target = transform.position;
        var currentTile = Tile;
        var moveDeltaTime = 0f;
        var floatDeltaTime = 0f;
        var movingUp = true;

        while (true)
        {
            if (Tile != null && currentTile != Tile)
            {
                target = Tile.transform.position + _bottom;

                Moving = Vector3.Distance(transform.position, target) > MoveDeadZone;
                if (Moving)
                {
                    Floating = false;
                    moveDeltaTime += Time.deltaTime;
                    var t = Mathf.Clamp01(moveDeltaTime / MoveAnimation);
                    transform.position = MoveAnimation > 0 ? Vector3.Lerp(transform.position, target, t) : target;
                }
                else
                {
                    moveDeltaTime = 0;
                    currentTile = Tile;
                    transform.position = target;
                    OnTile = true;
                }
            }

            if (OnTile && Floating)
            {
                var floatTarget = Tile.transform.position + (movingUp ? _top : _bottom);
                if (Vector3.Distance(transform.position, floatTarget) < MoveDeadZone)
                {
                    movingUp = !movingUp;
                    floatDeltaTime = 0;
                }
                else
                {
                    floatDeltaTime += Time.deltaTime;
                    var t = Mathf.Clamp01(floatDeltaTime / FloatAnimation);
                    transform.position = FloatAnimation > 0 ? Vector3.Lerp(transform.position, floatTarget, SmoothStep(t)) : floatTarget;
                }
            }

            if (!Moving && !Floating && OnTile)
            {
                if (Vector3.Distance(transform.position, _bottom) > MoveDeadZone)
                    transform.position = Tile.transform.position + _bottom;
            }

            yield return null;
        }
    }

    IEnumerator UpdateOffsets()
    {
        var oldOffsetY = OffsetY;
        var oldDeltaY = DeltaY;

        while (true)
        {
            if (oldOffsetY != OffsetY)
            {
                _bottom.Set(0, OffsetY, 0);
                _top.Set(0, OffsetY + DeltaY, 0);
            }
            else if (oldDeltaY != DeltaY)
            {
                _top.Set(0, OffsetY + DeltaY, 0);
            }

            yield return new WaitForSeconds(0.033f);
        }
    }

    float SmoothStep(float t)
    {
        return Mathf.SmoothStep(0, 1, t);
    }
}
