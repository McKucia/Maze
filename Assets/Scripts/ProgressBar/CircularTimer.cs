using UnityEngine;

public class CircularTimer : MonoBehaviour
{
    Transform _camera;

    void Start()
    {
        _camera = GameObject.FindWithTag("MainCamera").transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + _camera.forward);
    }
}
