using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] Vector3 _offset;
    [SerializeField] float _smoothTime = 0.3f;

    MazeGeneratorManager _manager;

    Vector3 velocity = Vector3.zero;
    float _storedShadowDistance;
    Transform _target;
    bool _init = false;

    private void Start()
    {
        _manager = MazeGeneratorManager.Instance;
    }

    private void LateUpdate()
    {
        if (!_manager.IsReady) return;
        if (!_init) 
        { 
            _target = GameObject.FindWithTag("Player").transform;
            _offset = transform.position - _target.position;
            _init = true;
        }

        Vector3 targetPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, _smoothTime);

        transform.LookAt(_target);
    }

    void OnPreRender()
    {
        _storedShadowDistance = QualitySettings.shadowDistance;
        QualitySettings.shadowDistance = 0;
    }

    void OnPostRender()
    {
        QualitySettings.shadowDistance = _storedShadowDistance;
    }
}
