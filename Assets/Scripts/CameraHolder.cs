using System.Collections;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public enum CameraPointings
    {
        Up,
        Right,
        Down,
        Left
    };

    [SerializeField]
    [Header("Rotation time in Seconds")] 
    float _rotationTime = 0.2f;

    [SerializeField]
    MinimapCamera _minimapCamera;

    PlayerMovement _playerMovement;
    MazeGeneratorManager _manager;

    bool isRotating = false;
    bool _init = false;
    CameraPointings CameraPointing = CameraPointings.Up;

    private void Start()
    {
        _manager = MazeGeneratorManager.Instance;
    }

    void Update()
    {
        if (!_manager.IsReady) return;
        if (!_init)
            _playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        if (!isRotating)
        {
            // right
            if (Input.GetKeyDown(KeyCode.E))
            {
                RotateCameraPointing(0);
                StartCoroutine(SmoothRotate(90f, _rotationTime));
                _minimapCamera.UpdateOffset(CameraPointing);
                _playerMovement.UpdateKeys(CameraPointing);
            }
            // left
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateCameraPointing(1);
                StartCoroutine(SmoothRotate(-90f, _rotationTime));
                _minimapCamera.UpdateOffset(CameraPointing);
                _playerMovement.UpdateKeys(CameraPointing);
            }
        }
    }

    IEnumerator SmoothRotate(float angle, float inTime)
    {
        isRotating = true;

        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + Vector3.up * angle);

        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }

        transform.rotation = toAngle;

        isRotating = false;
    }

    void RotateCameraPointing(int direction)
    {
        // right
        if (direction == 0 && CameraPointing == CameraPointings.Left)
        {
            CameraPointing = CameraPointings.Up;
            return;
        }

        // left 
        if (direction == 1 && CameraPointing == CameraPointings.Up)
        {
            CameraPointing = CameraPointings.Left;
            return;
        }

        // right
        if (direction == 0) CameraPointing++;
        // left
        if (direction == 1) CameraPointing--;
    }
}
