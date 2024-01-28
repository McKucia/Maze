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

    public void UpdateOffset(CameraHolder.CameraPointings cameraPointing)
    {
        switch(cameraPointing) 
        {
            case CameraHolder.CameraPointings.Up:
                _offset.x = Mathf.Abs(_offset.x);
                _offset.z = Mathf.Abs(_offset.z);
                break;
            case CameraHolder.CameraPointings.Right:
                _offset.x = Mathf.Abs(_offset.x);
                _offset.z = Mathf.Abs(_offset.z) * -1;
                break;
            case CameraHolder.CameraPointings.Down:
                _offset.x = Mathf.Abs(_offset.x) * -1;
                _offset.z = Mathf.Abs(_offset.z) * -1;
                break;
            case CameraHolder.CameraPointings.Left:
                _offset.x = Mathf.Abs(_offset.x) * -1;
                _offset.z = Mathf.Abs(_offset.z);
                break;
        }
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
