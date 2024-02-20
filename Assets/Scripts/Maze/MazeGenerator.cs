using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [HideInInspector] public Grid Grid;
    [HideInInspector] public bool IsReady;
    [HideInInspector] public int Level;

    readonly MazeGeneratorManager _manager = MazeGeneratorManager.Instance;

    List<GameObject> tileGameObjects;
    List<Room> _rooms;
    /// The index of the current region being carved.
    int _currentRegion = -1;
    System.Random _random;
    GameObject _playerObject;
    int _numSpawnCoroutines = 0;
    int _numFinishedSpawnCoroutines = 0;

    void Start()
    {
        _random = new System.Random();
        Grid = new Grid(new Vector2Int(_manager.gridSizeX, _manager.gridSizeY), _manager.isCircle, Level);

        _rooms = new List<Room>();
        tileGameObjects = new List<GameObject>();
    }

    public void SetActive(bool active)
    {
        if (active) gameObject.SetActive(true);
        StartCoroutine(FadeUp(active));
    }

    public async void Generate()
    {
        IsReady = false;
        tileGameObjects.ForEach(t => Destroy(t));

        if(_playerObject)
            Destroy(_playerObject);

        await Task.Run(() =>
        {
            ResetGenerator();
            Grid.Reset(new Vector2Int(_manager.gridSizeX, _manager.gridSizeY), _manager.isCircle);

            AddRooms();

            for (int y = 1; y < Grid.Size.y; y += 2)
                for (int x = 1; x < Grid.Size.x; x += 2)
                {
                    var tile = Grid.Tiles[x, y];
                    if (tile.Type != Tile.TileType.Wall) continue;
                    GrowMaze(tile);
                }

            ConnectRegions();
            //AddCarpets();
            Grid.ReverseTiles();
            AddExit();
            Grid.IsReady = true;
        });

        SpawnTiles();
    }

    public void DisplayMinimapTile(Tile tile)
    {
        if (tile.Exposed) return;
        Grid.SetTileMinimapObjectActive(tile, true);
    }

    public void SetFloor(Tile tile)
    {
        Grid.SetTileType(tile, Tile.TileType.Floor);
    }

    void SpawnPlayer()
    {
        foreach (var tile in Grid.Tiles)
            if (tile.Type == Tile.TileType.Floor)
            {
                _playerObject = Instantiate(_manager.playerPrefab, new Vector3(tile.Position.x, 0.5f, tile.Position.y), Quaternion.identity);
                break;
            }
    }

    void SpawnEnemies()
    {
        foreach (var room in _rooms)
        {
            var enemyObject = Instantiate(_manager.enemyPrefab, new Vector3(room.Position.x, 0.5f, room.Position.y), Quaternion.identity);
            enemyObject.GetComponent<Enemy>().SetRoom(room);
            enemyObject.transform.parent = transform;
        }
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
                var newTile = Grid.Tiles[x, y];
                if (newTile.Type != Tile.TileType.Wall) return false;
                tilesToCarve.Add(newTile);
            }

        _currentRegion++;
        tilesToCarve.ForEach(t => Grid.Carve(t, _currentRegion, Tile.TileType.Room));
        return true;
    }

    void SpawnTiles()
    {
        int i = 0, total = 0;

        foreach(var tile in Grid.Tiles)
        {
            total++; i++;
            if (i == 100 || total + 1 >= Grid.Tiles.Length)
            {
                StartCoroutine(SpawnTiles(i, total));
                _numSpawnCoroutines++;
                i = 0;
            }
        }
    }

    IEnumerator SpawnTiles(int howMany, int rightBound)
    {
        for(int i = rightBound - howMany; i < rightBound; i++)
        {
            SpawnTile(Grid.Tiles[i / Grid.Size.x, i % Grid.Size.y]);
            yield return null;
        }

        _numFinishedSpawnCoroutines++;
        if(_numFinishedSpawnCoroutines == _numSpawnCoroutines)
            ResumeGeneration();
    }

    void SpawnTile(Tile tile)
    {
        GameObject newTile = null;
        
        switch (tile.Type)
        {
            case Tile.TileType.Room:
                newTile = Instantiate(_manager.tileRoomPrefab, new Vector3(tile.Position.x, -Level * 2 + 100, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Wall:
                newTile = Instantiate(_manager.tileWallPrefab, new Vector3(tile.Position.x, -Level * 2 + .25f + 100, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Floor:
                newTile = Instantiate(_manager.tileFloorPrefab, new Vector3(tile.Position.x, -Level * 2 + 100, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Border:
                newTile = Instantiate(_manager.tileBorderPrefab, new Vector3(tile.Position.x, -Level * 2 + .5f + 100, tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Carpet:
                newTile = Instantiate(_manager.tileCarpetPrefab, new Vector3(tile.Position.x, -Level * 2 + 100 , tile.Position.y), Quaternion.identity);
                break;
            case Tile.TileType.Exit:
                newTile = Instantiate(_manager.tileExitPrefab, new Vector3(tile.Position.x, -Level * 2 + 100, tile.Position.y), Quaternion.identity);
                break;
        }

        if (newTile != null)
        {
            tileGameObjects.Add(newTile);
            Grid.SetTileObject(tile, newTile);
            newTile.transform.parent = transform;
        }
    }

    void ResumeGeneration()
    {
        transform.position = new Vector3(0, 0, 0);
        if (Level == _manager._currentLevel)
        {
            _manager.surface.BuildNavMesh();
            SpawnPlayer();
            SpawnEnemies();
            _manager.virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = _playerObject.transform;
        }
        else
        {
            transform.Translate(Vector3.down * 10f);
            gameObject.SetActive(false);
        }
        IsReady = true;
    }

    // Growing Tree
    void GrowMaze(Tile startTile)
    {
        Vector2Int lastDirection = Vector2Int.zero;

        _currentRegion++;
        Grid.Carve(startTile, _currentRegion);
        List<Tile> tiles = new List<Tile> { startTile };

        while (tiles.Count > 0)
        {
            Tile tile = tiles.Last();
            var unmadeTilesDirections = new List<Vector2Int>();

            foreach (var direction in HelperClass.Directions)
                if (Grid.CanCarve(tile, direction))
                    unmadeTilesDirections.Add(direction);

            if (unmadeTilesDirections.Count > 0)
            {
                Vector2Int direction;
                if (unmadeTilesDirections.Contains(lastDirection) && _random.Next(0, 100) > _manager.windingPercent)
                    direction = lastDirection;
                else
                    direction = unmadeTilesDirections[_random.Next(0, unmadeTilesDirections.Count)];

                Grid.Carve(Grid.GetNextTile(tile, direction, 1), _currentRegion);
                Grid.Carve(Grid.GetNextTile(tile, direction, 2), _currentRegion);

                tiles.Add(Grid.GetNextTile(tile, direction, 2));
                lastDirection = direction;
            }
            else
            {
                // No adjacent uncarved tiles
                tiles.RemoveAt(tiles.Count - 1);

                // This path has ended.
                lastDirection = Vector2Int.zero;
            }
        }
    }

    void AddCarpets()
    {
        foreach(var tile in Grid.Tiles)
        {
            if(tile.Type != Tile.TileType.Floor) continue;

            if (Grid.GetNeighboursNumber(tile) > 2)
                Grid.SetTileType(tile, Tile.TileType.Carpet);
        }
    }

    void AddExit()
    {
        foreach (var tile in Grid.Tiles) // TilesReversed
        {
            if (tile.Type != Tile.TileType.Floor) continue;

            if (Grid.GetWallNeighboursNumber(tile) > 2)
            {
                Grid.SetTileType(tile, Tile.TileType.Exit);
                Grid.ExitTile = tile;
                MazeGeneratorManager.Instance.SetNextMazeGeneratorFloor(tile);
                return;
            }
        }
    }

    void ConnectRegions()
    {
        var connectors = new List<Tile>();
        var connectorRegions = new List<int>();

        for (int y = 0; y < Grid.Size.y; y++)
            for (int x = 0; x < Grid.Size.x; x++)
            {
                var tile = Grid.GetTile(new Vector2Int(x, y));
                if (tile.Type != Tile.TileType.Wall)
                    continue;

                connectorRegions.Clear();
                foreach (var direction in HelperClass.Directions)
                {
                    if (!Grid.CheckNextTile(tile, direction, 1))
                        continue;
                    Tile nextTile = Grid.GetNextTile(tile, direction, 1);

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

        while (openRegions.Count > 0 && connectors.Count > 0)
        {
            var connector = connectors[_random.Next(0, connectors.Count)];
            Grid.Carve(connector, connector.RegionId);

            /*foreach (var direction in _manager.directions)
                if(Grid.CheckNextTileType(connector, direction, 1, Tile.TileType.Floor))
                    Grid.SetNextTileType(connector, direction, 1, Tile.TileType.Carpet);*/
        
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

    IEnumerator FadeUp(bool active)
    {
        float elapsedTime = 0f;
        float fadeSpeed = 7f;

        float targetPosY = transform.position.y + 10f;

        while (targetPosY > transform.position.y)
        {
            elapsedTime += Time.deltaTime;
            transform.position += new Vector3(0, fadeSpeed * Time.deltaTime, 0);
            yield return null;
        }
        if (!active) gameObject.SetActive(false);
    }
}
