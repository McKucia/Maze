using System.Collections;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [SerializeField]
    [Header("Rotation time in Seconds")] 
    float _rotationTime = 0.2f;

    bool isRotating = false;

    void Update()
    {
        if (!GameManager.Instance.Initialized) return;

        if (!isRotating)
        {
            // right
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameManager.Instance.RotateCameraPointing(0);
                StartCoroutine(SmoothRotate(90f, _rotationTime));
            }
            // left
            if (Input.GetKeyDown(KeyCode.Q))
            {
                GameManager.Instance.RotateCameraPointing(1);
                StartCoroutine(SmoothRotate(-90f, _rotationTime));
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
}
