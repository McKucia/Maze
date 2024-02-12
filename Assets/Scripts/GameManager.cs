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

    CinemachineVirtualCamera _virtualCamera;
    CinemachineBasicMultiChannelPerlin _virtualCameraNoise;
    MinimapCamera _minimapCamera;
    PlayerMovement _playerMovement;
    bool _mazeInit = false;

    void Update()
    {
        if (_mazeInit || !MazeGeneratorManager.Instance.IsReady) return;
        if (!_mazeInit)
        {
            _playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
            _minimapCamera = GameObject.FindWithTag("MinimapCamera").GetComponent<MinimapCamera>();
            _virtualCamera = GameObject.FindWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
            _virtualCameraNoise = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _mazeInit = true;
        }
    }

    #region
    public void ChangeVirtualCameraNoise(float amplitude, float frequency)
    {
        _virtualCameraNoise.m_AmplitudeGain = amplitude;
        _virtualCameraNoise.m_FrequencyGain = frequency;
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
