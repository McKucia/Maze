using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get {  return _instance; }
    }

    void Awake()
    {
        if (_instance)
            Destroy(gameObject);
        else
            _instance = this;

        DontDestroyOnLoad(this);
    }
    #endregion

    public bool Initialized
    {
        get { return _mazeInit; } 
    }

    MinimapCamera _minimapCamera;
    PlayerMovement _playerMovement;
    bool _mazeInit = false;

    private void Start()
    {
        Debug.Log(_noiseShake);
    }

    void Update()
    {
        if (!MazeGeneratorManager.Instance.IsReady) return;
        if (!_mazeInit)
        {
            _playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
            _minimapCamera = GameObject.FindWithTag("MinimapCamera").GetComponent<MinimapCamera>();
            _virtualCamera = GameObject.FindWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
            _virtualCameraNoise = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _mazeInit = true;
        }
        ChangeVirtualCameraOrthoSize();
        ChangeVirtualCameraNoise();
    }

    #region Camera Noise & Size
    CinemachineVirtualCamera _virtualCamera;
    CinemachineBasicMultiChannelPerlin _virtualCameraNoise;
    [SerializeField] NoiseSettings _noiseShake;
    [SerializeField] NoiseSettings _noiseHandheld;

    private const float _cameraOrthoSpeed = 10f;
    private const float _cameraAmplitudeSpeed = 15f;
    private const float _cameraFrequencySpeed = 15f;

    public float CameraOrthoSize = 3f;
    public float CameraNoiseFrequency;
    public float CameraNoiseAmplitude;

    public void ChangeVirtualCameraNoiseProfile(string type)
    {
        if(type == "shake") _virtualCameraNoise.m_NoiseProfile = _noiseShake;
        else _virtualCameraNoise.m_NoiseProfile = _noiseHandheld;
    }

    private void ChangeVirtualCameraOrthoSize()
    {
        _virtualCamera.m_Lens.OrthographicSize = 
            Mathf.Lerp(_virtualCamera.m_Lens.OrthographicSize, CameraOrthoSize, Time.deltaTime * _cameraOrthoSpeed);
    }

    private void ChangeVirtualCameraNoise()
    {
        _virtualCameraNoise.m_AmplitudeGain =
            Mathf.Lerp(_virtualCameraNoise.m_AmplitudeGain, CameraNoiseAmplitude, Time.deltaTime * _cameraAmplitudeSpeed);
        _virtualCameraNoise.m_FrequencyGain =
            Mathf.Lerp(_virtualCameraNoise.m_FrequencyGain, CameraNoiseFrequency, Time.deltaTime * _cameraFrequencySpeed);
    }
    #endregion

    #region CameraRotation
    public enum CameraPointings
    {
        Up,
        Right,
        Down,
        Left
    };

    [HideInInspector]
    public CameraPointings CameraPointing = CameraPointings.Up;

    public void RotateCameraPointing(int direction)
    {
        // right
        if (direction == 0)
        {
            if(CameraPointing == CameraPointings.Left)
                CameraPointing = CameraPointings.Up;
            else
                CameraPointing++;
        }

        // left 
        if (direction == 1)
        {
            if(CameraPointing == CameraPointings.Up)
                CameraPointing = CameraPointings.Left;
            else
                CameraPointing--;
        }

        _playerMovement.UpdateKeys();
        _minimapCamera.UpdateOffset();
    }
    #endregion
}
