using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] int _speed = 3;

    MazeGeneratorManager _manager;

    bool _isMoving = false;
    Vector2Int _currentDirection;

    void Start()
    {
        _currentDirection = Vector2Int.zero;
        _manager = MazeGeneratorManager.Instance;
    }

    void Update()
    {
        HandleInput();
        Move();
    }

    void HandleInput()
    {
        if (_isMoving) return;

        if (Input.GetKeyDown(KeyCode.W)) _currentDirection = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.D)) _currentDirection = Vector2Int.right;
        if (Input.GetKeyDown(KeyCode.S)) _currentDirection = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) _currentDirection = Vector2Int.left;
    }

    void Move()
    {
        if (_currentDirection == Vector2Int.zero) return;

        var grid = _manager.GetCurrentGrid();
        if (!grid.IsReady) return;

        int positionX = Mathf.RoundToInt(transform.position.x);
        int positionY = Mathf.RoundToInt(transform.position.z);

        Tile currentPlayerTile = new Tile(null, new Vector2Int(positionX, positionY));
        Debug.Log(currentPlayerTile.Position);

        if (grid.CheckNextTileType(currentPlayerTile, _currentDirection, 1, Tile.TileType.Wall))
        {
            _currentDirection = Vector2Int.zero;
            _isMoving = false;
            return;
        }

        _isMoving = true;
        transform.Translate(_speed * Time.deltaTime * new Vector3(_currentDirection.x, 0, _currentDirection.y));
    }
}
