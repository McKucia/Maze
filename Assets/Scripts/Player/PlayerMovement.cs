using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] int _moveSpeed = 8;
    [SerializeField] int _chargeSpeed = 14;
    [SerializeField] int _rotationSpeed = 14;
    [SerializeField] int _fallingSpeed = 6;
    [SerializeField] float _chargeNoiseAmplitude = 0.1f;
    [SerializeField] float _chargeNoiseFrequency = 50f;
    [SerializeField] float _chargeLensOrthoSize = 2f;
    [SerializeField] float _moveNoiseAmplitude = 1f;
    [SerializeField] float _moveNoiseFrequency = 0.2f;
    [SerializeField] float _moveLensOrthoSize = 3f;
    [SerializeField] Material _playerMaterial;
    [SerializeField] Transform _playerModel;
    public Rigidbody Rb => _rb;

    Rigidbody _rb;
    int _speed;
    bool _isMoving = false;
    bool _isMovingToTarget = false;
    bool _isCharging = false;
    bool _chargeChange = false;
    bool _isChangingLevel = false;
    bool _init = false;
    Grid _grid;
    Vector2Int _target;
    Vector2Int _currentDirection;
    Vector2Int _nextDirection;
    Vector2Int _currentPosition;
    Tile _currentTile;

    Vector2Int _vectorUp = Vector2Int.up;
    Vector2Int _vectorRight = Vector2Int.right;
    Vector2Int _vectorDown = Vector2Int.down;
    Vector2Int _vectorLeft = Vector2Int.left;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentDirection = Vector2Int.zero;

        int positionX = Mathf.FloorToInt(transform.position.x);
        int positionY = Mathf.FloorToInt(transform.position.z);

        _currentPosition = new Vector2Int(positionX, positionY);
    }

    void Update()
    {
        if (!GameManager.Instance.Initialized) return;
        if (!_init)
        {
            _grid = MazeGeneratorManager.Instance.GetCurrentGrid();
            _init = true;
        }

        HandleInput();
        RotatePlayer();
        CheckExit();

        if (_isChangingLevel)
        {
            if(!_isMovingToTarget)
            {
                ChangeLevel();
                return;
            }
        }
        if (!_isMoving)
        {
            if (_currentDirection != Vector2Int.zero)
                _isMoving = true;
        }
        else
        {
            GetCurrentTile();
            Move();
        }
        if(_isMovingToTarget)
        {
            GetCurrentTile();
            MoveToTarget();
        }

        _speed = _isCharging ? _chargeSpeed : _moveSpeed;

        if (_chargeChange)
            SetVirtualCameraNoise();
    }

    void HandleInput()
    {
        _chargeChange = false;
        var direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = _vectorUp;
        if (Input.GetKeyDown(KeyCode.D)) direction = _vectorRight;
        if (Input.GetKeyDown(KeyCode.S)) direction = _vectorDown;
        if (Input.GetKeyDown(KeyCode.A)) direction = _vectorLeft;

        var prevIsCharging = _isCharging;
        _isCharging = Input.GetKey(KeyCode.LeftShift) && _isMoving;
        if (prevIsCharging != _isCharging) _chargeChange = true;

        if (!_isMoving) _currentDirection = direction;
        if (_isMoving && _currentDirection != direction && direction != Vector2Int.zero) _nextDirection = direction;
    }

    void RotatePlayer()
    {
        Vector3 targetPosition = new Vector3(_currentDirection.x + transform.position.x, transform.position.y, _currentDirection.y + transform.position.z);
        var targetDirection = targetPosition - transform.position;
        float singleStep = _rotationSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(_playerModel.forward, targetDirection, singleStep, 0.0f);
        _playerModel.rotation = Quaternion.LookRotation(newDirection);
    }


    public void UpdateKeys()
    {
        switch (GameManager.Instance.CameraPointing)
        {
            case GameManager.CameraPointings.Up:
                _vectorUp = Vector2Int.up;
                _vectorRight = Vector2Int.right;
                _vectorDown = Vector2Int.down;
                _vectorLeft = Vector2Int.left;
                break;
            case GameManager.CameraPointings.Right:
                _vectorUp = Vector2Int.right;
                _vectorRight = Vector2Int.down;
                _vectorDown = Vector2Int.left;
                _vectorLeft = Vector2Int.up;
                break;
            case GameManager.CameraPointings.Down:
                _vectorUp = Vector2Int.down;
                _vectorRight = Vector2Int.left;
                _vectorDown = Vector2Int.up;
                _vectorLeft = Vector2Int.right;
                break;
            case GameManager.CameraPointings.Left:
                _vectorUp = Vector2Int.left;
                _vectorRight = Vector2Int.up;
                _vectorDown = Vector2Int.right;
                _vectorLeft = Vector2Int.down;
                break;
        }
    }

    void ChangeLevel()
    {
        float targetLevelY = _grid.Level * -2f + 0.5f;

        transform.position -= new Vector3(0, Time.deltaTime * _fallingSpeed, 0);

        if (targetLevelY >= transform.position.y)
        {
            var pos = transform.position;
            pos.y = targetLevelY;
            transform.position = pos;
            _isChangingLevel = false;
        }
    }

    void CheckExit()
    {
        bool isClose;
        if (Distance2D(_grid.ExitTile.Position) <= 1f &&
            Angle2D(_grid.ExitTile.Position) <= 20f)
        {
            isClose = true;
            if (Input.GetKeyDown(KeyCode.R))
            {
                _target = _grid.GetExitTile().Position;
                _isChangingLevel = true;
                _isMovingToTarget = true;
            }
        }
        else
            isClose = false;

        _grid.GetExitTile()
            .tileObject
            .GetComponent<ExitTile>()
            .ShowHideInteractionHint(isClose);
    }

    void Move()
    {
        var nextTile = _grid.GetNextTile(_currentTile, _currentDirection, 1);

        if (_nextDirection != Vector2Int.zero)
        {
            var nextDirectionTile = _grid.GetNextTile(_currentTile, _nextDirection, 1);

            if (nextDirectionTile.Type != Tile.TileType.Wall)
            {
                if (Distance2D(nextDirectionTile.Position) == 1f)
                {
                    _currentDirection = _nextDirection;
                    _nextDirection = Vector2Int.zero;
                    nextTile = _grid.GetNextTile(_currentTile, _currentDirection, 1);
                }
                else if(Distance2D(_currentTile.Position) < 0.05f)
                {
                    _target = _currentTile.Position;
                    _isMovingToTarget = true;
                    _isMoving = false;
                    return;
                }
            }
        }

        if ((nextTile.Type == Tile.TileType.Wall || nextTile.Type == Tile.TileType.Exit) &&
            Distance2D(nextTile.Position) <= 1f)
        {
            _currentDirection = Vector2Int.zero;
            _nextDirection = Vector2Int.zero;
            transform.position = new Vector3(_currentTile.Position.x, transform.position.y, _currentTile.Position.y);
            _isMoving = false;
            return;
        }

        transform.Translate(new Vector3(_currentDirection.x, 0, _currentDirection.y) * Time.deltaTime * _speed / 2.0f);
    }

    float Distance2D(Vector2Int to)
    {
        return Vector2.Distance(new Vector2(transform.position.x, transform.position.z), to);
    }

    float Angle2D(Vector2Int to)
    {
        Vector2 toTarget = to - new Vector2(_playerModel.position.x, _playerModel.position.z);
        return Vector2.Angle(new Vector2(_playerModel.forward.x, _playerModel.forward.z), toTarget);
    }

    void MoveToTarget()
    {
        var targetPosition = new Vector3(_target.x, transform.position.y, _target.y);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * _speed / 2.0f);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            _isMovingToTarget = false;
            _isMoving = true;
            _currentPosition = _target;
            _currentDirection = Vector2Int.zero;
            transform.position = new Vector3(_currentTile.Position.x, transform.position.y, _currentTile.Position.y);

            if (_isChangingLevel)
            {
                MazeGeneratorManager.Instance.IncrementLevel();
                _grid = MazeGeneratorManager.Instance.GetCurrentGrid();
            }
        }
    }

    public void Hit()
    {
        StartCoroutine(ChangeColor());
    }

    void GetCurrentTile()
    {
        int positionX = Mathf.RoundToInt(transform.position.x);
        int positionY = Mathf.RoundToInt(transform.position.z);

        _currentPosition = new Vector2Int(positionX, positionY);
        _currentTile = _grid.GetTile(_currentPosition);

        if (!_currentTile.Exposed && _currentTile.Type != Tile.TileType.Wall)
            MazeGeneratorManager.Instance.DisplayMinimapTile(_currentTile);
    }

    void SetVirtualCameraNoise()
    {
        if (_isCharging)
        {
            GameManager.Instance.CameraOrthoSize = _chargeLensOrthoSize;
            GameManager.Instance.CameraNoiseAmplitude = _chargeNoiseAmplitude;
            GameManager.Instance.CameraNoiseFrequency = _chargeNoiseFrequency;
            GameManager.Instance.ChangeVirtualCameraNoiseProfile("shake");
        }
        else
        {
            GameManager.Instance.CameraOrthoSize = _moveLensOrthoSize;
            GameManager.Instance.CameraNoiseAmplitude = _moveNoiseAmplitude;
            GameManager.Instance.CameraNoiseFrequency = _moveNoiseFrequency;
            GameManager.Instance.ChangeVirtualCameraNoiseProfile("handheld");
        }
    }

    IEnumerator ChangeColor()
    {
        _playerMaterial.SetColor("_Color", Color.red);
        yield return new WaitForSeconds(0.2f);
        _playerMaterial.SetColor("_Color", Color.green);
    }
}
