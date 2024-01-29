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
            _mazeInit = true;
        }
    }

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
