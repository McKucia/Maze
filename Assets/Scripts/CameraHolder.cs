using System.Collections;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [SerializeField]
    [Header("Rotation time in Seconds")] 
    float _rotationTime = 0.2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(SmoothRotate(Vector3.up * 90, 0.2f));
        if (Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(SmoothRotate(Vector3.up * -90, 0.2f));
    }

    IEnumerator SmoothRotate(Vector3 angle, float inTime)
    {
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + angle);

        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
    }
}
