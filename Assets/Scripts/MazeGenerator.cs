using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] bool _generate;
    [SerializeField] bool _isCircle;
    [SerializeField] GameObject tileRoomPrefab;
    [SerializeField] GameObject tileFloorPrefab;
    [SerializeField] GameObject tileWallPrefab;
    [SerializeField] GameObject tileConnectorPrefab;
    [SerializeField] int _numRoomTries = 40;
    [SerializeField] int _roomsPadding = 1;
    [SerializeField] int _roomExtraSize = 0;
    [SerializeField] int windingPercent = 50;
    [SerializeField] Vector2Int _gridSize = new Vector2Int(15, 15);

    List<Object> tileGameObjects;
    List<Room> _rooms;
    List<Vector2Int> _directions;
    Grid _grid;
    bool _generateCheck = false;
    /// The index of the current region being carved.
    int _currentRegion = -1;

    void Start()
    {
        _grid = new Grid(_gridSize, _isCircle);

        _rooms = new List<Room>();
        tileGameObjects = new List<Object>();
        _directions = new List<Vector2Int>()
            {
                new Vector2Int(0, 1),  // up
                new Vector2Int(1, 0),  // right
                new Vector2Int(0, -1), // down
                new Vector2Int(-1, 0)  // left
            };
        Generate();
    }

    private void OnValidate()
    {
        if (_generateCheck != _generate)
        {
            _generate = _generateCheck;
            Generate();
        }
    }

    void Generate()
    {
        _currentRegion = -1;
        tileGameObjects.ForEach(t => Destroy(t));
        tileGameObjects.Clear();
        _rooms.Clear();
        _grid.Reset();

        AddRooms();

        for (int y = 1; y < _grid.Size.y; y += 2)
            for (int x = 1; x < _grid.Size.x; x += 2)
            {
                var tile = _grid.Tiles[x, y];
                if (tile.Type != Tile.TileType.Wall) continue;
                GrowMaze(tile);
            }

        
        ConnectRegions();

        foreach (var t in _grid.Tiles)
            SpawnTile(t);
    }

    void AddRooms()
    {
        for (int i = 0; i < _numRoomTries; i++)
        {
            int sizeX = Random.Range(1, 3 + _roomExtraSize) * 2 + 1;
            int sizeY = Random.Range(1, 3 + _roomExtraSize) * 2 + 1;

            int x = (Random.Range(0, _gridSize.x - sizeX) / 2) * 2 + 1;
            int y = (Random.Range(0, _gridSize.y - sizeY) / 2) * 2 + 1;

            Room room = new Room(new Vector2Int(sizeX, sizeY), new Vector2Int(x, y));

            bool overlaps = _rooms.Any(r => r.Overlaps(room, _roomsPadding));

            if (overlaps || !SpawnRoom(room)) continue;

            _rooms.Add(room);
        }
    }

    bool SpawnRoom(Room room)
    {
        var tilesToCarve = new List<Tile>();

        for (int y = room.Position.y; y < room.Position.y + room.Size.y; y++)
            for(int x = room.Position.x; x < room.Position.x + room.Size.x; x++)
            {
                var newTile = _grid.Tiles[x, y];
                if (newTile.Type != Tile.TileType.Wall) return false;
                tilesToCarve.Add(newTile);
            }

        _currentRegion++;
        tilesToCarve.ForEach(t => _grid.Carve(t, _currentRegion, Tile.TileType.Room));
        return true;
    }

    void SpawnTile(Tile tile)
    {
        GameObject newTile = null;
        
        switch (tile.Type)
        {
            case Tile.TileType.Room:
                newTile = Instantiate(tileRoomPrefab, new Vector3(tile.Position.x, 0, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Wall:
                newTile = Instantiate(tileWallPrefab, new Vector3(tile.Position.x, .5f, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Floor:
                newTile = Instantiate(tileFloorPrefab, new Vector3(tile.Position.x, 0, tile.Position.y), Quaternion.identity);
                break;
        }

        if(newTile != null)
            tileGameObjects.Add(newTile);
    }

    // Growing Tree
    void GrowMaze(Tile startTile)
    {
        Vector2Int lastDirection = new Vector2Int(0, 0);

        _currentRegion++;
        _grid.Carve(startTile, _currentRegion);
        List<Tile> tiles = new List<Tile> { startTile };

        while (tiles.Count > 0)
        {
            Tile tile = tiles.Last();
            var unmadeTilesDirections = new List<Vector2Int>();

            foreach (var direction in _directions)
                if (_grid.CanCarve(tile, direction))
                    unmadeTilesDirections.Add(direction);

            if (unmadeTilesDirections.Count > 0)
            {
                Vector2Int direction;
                if (unmadeTilesDirections.Contains(lastDirection) && Random.Range(0, 100) > windingPercent)
                    direction = lastDirection;
                else
                    direction = unmadeTilesDirections[Random.Range(0, unmadeTilesDirections.Count)];

                _grid.Carve(_grid.GetNextTile(tile, direction, 1), _currentRegion);
                _grid.Carve(_grid.GetNextTile(tile, direction, 2), _currentRegion);

                tiles.Add(_grid.GetNextTile(tile, direction, 2));
                lastDirection = direction;
            }
            else
            {
                // No adjacent uncarved tiles
                tiles.RemoveAt(tiles.Count - 1);
            
                // This path has ended.
                lastDirection = new Vector2Int(0, 0);
            }
        }
    }

    void ConnectRegions()
    {
        var connectors = new List<Tile>();
        var connectorRegions = new List<int>();

        for (int y = 0; y < _grid.Size.y; y++)
            for (int x = 0; x < _grid.Size.x; x++)
            {
                var tile = _grid.GetTile(new Vector2Int(x, y));
                if (tile.Type != Tile.TileType.Wall)
                    continue;

                connectorRegions.Clear();
                foreach (var direction in _directions)
                {
                    if (!_grid.CheckNextTile(tile, direction, 1))
                        continue;
                    Tile nextTile = _grid.GetNextTile(tile, direction, 1);

                    if (nextTile.RegionId != -1 && !connectorRegions.Contains(nextTile.RegionId))
                        connectorRegions.Add(nextTile.RegionId);
                }

                if (connectorRegions.Count < 2) continue;

                tile.ConnectorRegions.AddRange(connectorRegions);
                connectors.Add(tile);
            }

        //foreach(var c in connectors)
        //{
        //    var con = Instantiate(tileConnectorPrefab, new Vector3(c.Position.x, 2f, c.Position.y), Quaternion.identity);
        //    tileGameObjects.Add(con);
        //}

        var merged = new int[_currentRegion + 1];
        var openRegions = new List<int>();
        for (int i = 0; i <= _currentRegion; i++)
        {
            merged[i] = i;
            openRegions.Add(i);
        }

        while (openRegions.Count > 0)
        {
            var connector = connectors[Random.Range(0, connectors.Count)];
            _grid.Carve(connector, connector.RegionId);
        
            var regions = connector.ConnectorRegions.Select(r => merged[r]).ToList();
            var dest = regions.First();
            var sources = regions.Skip(1).ToList();
        
            for (int i = 0; i <= _currentRegion; i++)
                if (sources.Contains(merged[i]))
                    merged[i] = dest;
        
            openRegions.RemoveAll(r => sources.Contains(r));
        
            connectors.RemoveAll(c =>
                (connector.IsClose(c, 3)) ||
                (c.ConnectorRegions.Select(r => merged[r]).Count() <= 1)
            );
        };
    }
}
