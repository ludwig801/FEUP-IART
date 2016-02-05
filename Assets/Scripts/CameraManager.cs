using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public Camera Camera;
    [Range(0, 10)]
    public int RotationSpeed;
    public bool RotateHorizontal, RotateVertical;

    public float EulerHorizontal
    {
        get
        {
            return transform.rotation.eulerAngles.y;
        }
    }

    public float EulerVertical
    {
        get
        {
            return transform.rotation.eulerAngles.x;
        }
    }

    GameBoard _gameBoard;
    Quaternion _rotateTo;

    void Start()
    {
        _rotateTo = transform.rotation;

        SetRotationHorizontal(90);
    }

    void Update()
    {
        HandleKeysInput();

        UpdateVisualElements();
    }

    void HandleKeysInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ApplyRotation(-1, 0);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ApplyRotation(+1, 0);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ApplyRotation(0, -1);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            ApplyRotation(0, +1);
    }

    void UpdateVisualElements()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, _rotateTo, RotationSpeed > 0 ? RotationSpeed * Time.deltaTime : 1);
    }

    void ApplyRotation(int horizontalAmount, int verticalAmount)
    {
        var rotation = _rotateTo.eulerAngles;
        rotation.y -= 90 * horizontalAmount;
        rotation.x += 15 * verticalAmount;
        SetRotation((int)rotation.y, (int)rotation.x);
    }

    void SetRotationHorizontal(int degrees)
    {
        if (RotateHorizontal)
        {
            var rotation = _rotateTo.eulerAngles;
            rotation.y = (degrees / 90) * 90;
            _rotateTo.eulerAngles = rotation;        
        }
    }

    void SetRotationVertical(int degrees)
    {
        if (RotateVertical)
        {
            var rotation = _rotateTo.eulerAngles;
            rotation.x = (degrees / 15) * 15;
            rotation.x = Mathf.Clamp(rotation.x, 305, 355);
            _rotateTo.eulerAngles = rotation;         
        }
    }

    void SetRotation(int horizontalDegrees, int verticalDegrees)
    {
        SetRotationHorizontal(horizontalDegrees);
        SetRotationVertical(verticalDegrees);
    }
}
