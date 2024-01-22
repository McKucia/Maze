using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    readonly MazeGeneratorManager _manager = MazeGeneratorManager.Instance;
    List<GameObject> tileGameObjects;
    List<Room> _rooms;
    Grid _grid;
    /// The index of the current region being carved.
    int _currentRegion = -1;
    int _level;
    System.Random _random;

    public MazeGenerator(int level)
    {
        _level = level;
        _random = new System.Random();
        _grid = new Grid(new Vector2Int(_manager.gridSizeX, _manager.gridSizeY), _manager.isCircle);

        _rooms = new List<Room>();
        tileGameObjects = new List<GameObject>();

        Generate();
    }

    public async void Generate()
    {
        tileGameObjects.ForEach(t => Destroy(t));

        await Task.Run(() =>
        {
            ResetGenerator();
            _grid.Reset(new Vector2Int(_manager.gridSizeX, _manager.gridSizeY), _manager.isCircle);

            AddRooms();

            for (int y = 1; y < _grid.Size.y; y += 2)
                for (int x = 1; x < _grid.Size.x; x += 2)
                {
                    var tile = _grid.Tiles[x, y];
                    if (tile.Type != Tile.TileType.Wall) continue;
                    GrowMaze(tile);
                }
            
            ConnectRegions();
            AddCarpets();
        });

        foreach (var t in _grid.Tiles)
            SpawnTile(t);
    }

    void ResetGenerator()
    {
        _currentRegion = -1;
        tileGameObjects.Clear();
        _rooms.Clear();
    }

    void AddRooms()
    {
        for (int i = 0; i < _manager.numRoomTries; i++)
        {
            int sizeX = _random.Next(1, 3 + _manager.roomExtraSize) * 2 + 1;
            int sizeY = _random.Next(1, 3 + _manager.roomExtraSize) * 2 + 1;

            int x = (_random.Next(0, _manager.gridSizeX - sizeX) / 2) * 2 + 1;
            int y = (_random.Next(0, _manager.gridSizeY - sizeY) / 2) * 2 + 1;

            Room room = new Room(new Vector2Int(sizeX, sizeY), new Vector2Int(x, y));

            bool overlaps = _rooms.Any(r => r.Overlaps(room, _manager.roomsPadding));

            if (overlaps || !CarveRoom(room)) continue;

            _rooms.Add(room);
        }
    }

    bool CarveRoom(Room room)
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
                newTile = Instantiate(_manager.tileRoomPrefab, new Vector3(tile.Position.x, -_level * 2, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Wall:
                newTile = Instantiate(_manager.tileWallPrefab, new Vector3(tile.Position.x, -_level * 2 + .5f, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Floor:
                newTile = Instantiate(_manager.tileFloorPrefab, new Vector3(tile.Position.x, -_level * 2, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Border:
                newTile = Instantiate(_manager.tileBorderPrefab, new Vector3(tile.Position.x, -_level * 2 + .5f, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Carpet:
                newTile = Instantiate(_manager.tileCarpetPrefab, new Vector3(tile.Position.x, -_level * 2 + .5f, tile.Position.y), Quaternion.identity);
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

            foreach (var direction in _manager.directions)
                if (_grid.CanCarve(tile, direction))
                    unmadeTilesDirections.Add(direction);

            if (unmadeTilesDirections.Count > 0)
            {
                Vector2Int direction;
                if (unmadeTilesDirections.Contains(lastDirection) && _random.Next(0, 100) > _manager.windingPercent)
                    direction = lastDirection;
                else
                    direction = unmadeTilesDirections[_random.Next(0, unmadeTilesDirections.Count)];

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

    void AddCarpets()
    {
        foreach(var tile in _grid.Tiles)
        {
            if(tile.Type != Tile.TileType.Floor) continue;

            if (_grid.GetNeighboursNumber(tile) > 2)
                _grid.SetTileType(tile, Tile.TileType.Carpet);
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
                foreach (var direction in _manager.directions)
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
            var connector = connectors[_random.Next(0, connectors.Count)];
            _grid.Carve(connector, connector.RegionId);

            /*foreach (var direction in _manager.directions)
                if(_grid.CheckNextTileType(connector, direction, 1, Tile.TileType.Floor))
                    _grid.SetNextTileType(connector, direction, 1, Tile.TileType.Carpet);*/
        
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
