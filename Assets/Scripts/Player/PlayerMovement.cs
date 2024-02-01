using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] int _speed = 8;
    public Rigidbody Rb => _rb;

    Rigidbody _rb;
    bool _isMoving = false;
    bool _init = false;
    Grid _grid;
    Vector2Int _target;
    Vector2Int _currentDirection;
    Vector2Int _currentPosition;

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

        if (!_isMoving)
        {
            HandleInput();
            if (_currentDirection != Vector2Int.zero)
                _target = GetTarget();
        }
        else
        {
            Move();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) _currentDirection = _vectorUp;
        if (Input.GetKeyDown(KeyCode.D)) _currentDirection = _vectorRight;
        if (Input.GetKeyDown(KeyCode.S)) _currentDirection = _vectorDown;
        if (Input.GetKeyDown(KeyCode.A)) _currentDirection = _vectorLeft;
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

    Vector2Int GetTarget()
    {
        int i = 0;
        Tile nextTile = _grid.GetTile(_currentPosition);

        while(_grid.GetNextTile(nextTile, _currentDirection, 1).Type != Tile.TileType.Wall &&
            _grid.GetNextTile(nextTile, _currentDirection, 1).Type != Tile.TileType.Carpet &&
            i++ < _grid.Size.x)
        {
            nextTile = _grid.GetNextTile(nextTile, _currentDirection, 1);
        }
        if (_grid.GetNextTile(nextTile, _currentDirection, 1).Type == Tile.TileType.Carpet)
        {
            nextTile = _grid.GetNextTile(nextTile, _currentDirection, 1);
        }

        _isMoving = true;
        return new Vector2Int(nextTile.Position.x, nextTile.Position.y);
    }

    void Move()
    {
        int positionX = Mathf.RoundToInt(transform.position.x);
        int positionY = Mathf.RoundToInt(transform.position.z);

        _currentPosition = new Vector2Int(positionX, positionY);

        var targetPosition = new Vector3(_target.x, transform.position.y, _target.y);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime  * _speed / 2.0f);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            _isMoving = false;
            _currentPosition = _target;
            _currentDirection = Vector2Int.zero;
        }

        MazeGeneratorManager.Instance.DisplayMinimapTile(_currentPosition);
    }
}
