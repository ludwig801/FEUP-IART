using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{
    public const int COLOR_BASE = 1;
    public const int COLOR_CRAVINGS = 0;

    public Color ColorDefault, ColorInvalid, ColorCravings;
    [Range(0, 2)]
    public float AnimDuration;
    public Tile Tile;
    public bool Horizontal, Free, IsInvalid;

    public bool tinting;
    public float colorDeltaTime;
    public Color cravingsTargetColor;

    public bool HasTile
    {
        get
        {
            return Tile != null;
        }
    }

    Material[] Materials
    {
        get
        {
            if (_materials == null)
                _materials = GetComponentInChildren<MeshRenderer>().materials;
            return _materials;
        }
    }

    Vector3 _offset;
    Material[] _materials;


    void Start()
    {
        CalculateOffset();
    }

    void OnEnable()
    {
        CalculateOffset();

        StartCoroutine(UpdateColor());
        StartCoroutine(UpdatePosition());
        StartCoroutine(UpdateRotation());
    }

    void CalculateOffset()
    {
        var gameBoard = GameBoard.Instance;
        _offset = new Vector3(0.5f * (gameBoard.TileSize + gameBoard.TileSpacing * gameBoard.TileSize), 0.5f * gameBoard.TileSize,
            -0.5f * (gameBoard.TileSize + gameBoard.TileSpacing * gameBoard.TileSize));
    }

    IEnumerator UpdateColor()
    {
        //var colorDeltaTime = 0f;
        //var tinting = true;

        colorDeltaTime = 0f;
        tinting = true;
        cravingsTargetColor = IsInvalid ? ColorInvalid : ColorCravings;
        var _oldCravingsColor = Color.clear;
        var _oldBaseColor = Color.clear;
        
        while (true)
        {
            var cravingsTargetColor = IsInvalid ? ColorInvalid : ColorCravings;
            var baseTargetColor = IsInvalid ? ColorInvalid : ColorDefault;

            if (!Utils.IsColorLike(_oldCravingsColor, cravingsTargetColor) && !Utils.IsColorLike(_oldBaseColor, baseTargetColor))
            {
                _oldCravingsColor = cravingsTargetColor;
                _oldBaseColor = baseTargetColor;
                colorDeltaTime = 0;
                tinting = true;
            }
            else if (tinting)
            {
                var baseMaterial = Materials[COLOR_BASE];
                var cravingsMaterial = Materials[COLOR_CRAVINGS];
                colorDeltaTime += Time.deltaTime;
                var t = Mathf.Clamp01(colorDeltaTime / AnimDuration);

                baseMaterial.color = AnimDuration > 0 ? Color.Lerp(baseMaterial.color, baseTargetColor, t) : baseTargetColor;
                cravingsMaterial.color = AnimDuration > 0 ? Color.Lerp(cravingsMaterial.color, cravingsTargetColor, t) : cravingsTargetColor;

                tinting = (t < 1);
            }

            yield return null;
        }
    }

    IEnumerator UpdatePosition()
    {
        var posDeltaTime = 0f;
        var oldTile = Tile;
        oldTile = null;
        var targetPosition = _offset;
        var moving = true;

        while (true)
        {
            if (HasTile && oldTile != Tile)
            {
                oldTile = Tile;
                posDeltaTime = 0;
                moving = true;
                targetPosition = (Tile.transform.position + _offset);
            }
            else if (moving)
            {
                posDeltaTime += Time.deltaTime;
                var t = Mathf.Clamp01(posDeltaTime / AnimDuration);
                transform.position = AnimDuration > 0 ? Vector3.Lerp(transform.position, targetPosition, t) : targetPosition;

                moving = (t < 1);
            }

            yield return null;
        }
    }

    IEnumerator UpdateRotation()
    {
        var rotDeltaTime = 0f;
        var currentHorizontal = !Horizontal;
        var targetRotation = Quaternion.Euler(0, Horizontal ? 0 : 90, 0);
        var rotating = true;

        while (true)
        {
            if (currentHorizontal != Horizontal)
            {
                currentHorizontal = Horizontal;
                rotDeltaTime = 0;
                targetRotation = Quaternion.Euler(0, currentHorizontal ? 0 : 90, 0);
                rotating = true;
            }
            else if (rotating)
            {
                rotDeltaTime += Time.deltaTime;
                var t = Mathf.Clamp01(rotDeltaTime / AnimDuration);
                transform.rotation = AnimDuration > 0 ? Quaternion.Lerp(transform.rotation, targetRotation, t) : targetRotation;

                rotating = (t < 1);
            }

            yield return null;
        }
    }
}
