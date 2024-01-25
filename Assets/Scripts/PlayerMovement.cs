using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] int _speed = 8;

    MazeGeneratorManager _manager;

    bool _isMoving = false;
    bool init = false;
    Grid _grid;
    Vector2Int _target;
    Vector2Int _currentDirection;
    Vector2Int _currentPosition;

    void Start()
    {
        _currentDirection = Vector2Int.zero;
        _manager = MazeGeneratorManager.Instance;

        int positionX = Mathf.FloorToInt(transform.position.x);
        int positionY = Mathf.FloorToInt(transform.position.z);

        _currentPosition = new Vector2Int(positionX, positionY);
    }

    void Update()
    {
        if (!_manager.IsReady) return;
        if (!init)
        {
            _grid = _manager.GetCurrentGrid();
            init = true;
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
        if (Input.GetKeyDown(KeyCode.W)) _currentDirection = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.D)) _currentDirection = Vector2Int.right;
        if (Input.GetKeyDown(KeyCode.S)) _currentDirection = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) _currentDirection = Vector2Int.left;
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

        var targetPosition = new Vector3(_target.x, 0f, _target.y);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime  * _speed / 2.0f);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            _isMoving = false;
            _currentPosition = _target;
            _currentDirection = Vector2Int.zero;
        }

        _manager.DisplayMinimapTile(_currentPosition);
    }
}
