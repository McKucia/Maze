using UnityEngine;

public struct Grid
{
    public Vector2Int Size;
    public Tile[,] Tiles;
    public Tile[,] TilesReversed;
    public bool IsReady;
    public Tile ExitTile;
    public int Level;
    bool _isCircle;

    public Grid(Vector2Int size, bool isCircle = false, int level = 0)
    {
        Size = size;
        Tiles = new Tile[Size.x, Size.y];
        TilesReversed = new Tile[Size.x, Size.y];
        _isCircle = isCircle;
        IsReady = false;
        ExitTile = new Tile();
        Level = level;
    }

    void InitSquare()
    {
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                Tiles[x, y] = new Tile(null, new Vector2Int(x, y), Tile.TileType.Wall);
    }

    void InitCircle()
    {
        Vector2Int centerPoint = new Vector2Int(Size.x / 2, Size.y / 2);
        int circleRadius = Mathf.Min(Size.x / 2, Size.y / 2);

        for (int x = centerPoint.x - circleRadius; x <= centerPoint.x + circleRadius; x++)
            for (int y = centerPoint.y - circleRadius; y <= centerPoint.y + circleRadius; y++)
            {
                int distance = (x - circleRadius) * (x - circleRadius) + (y - circleRadius) * (y - circleRadius);

                if(distance >= circleRadius * circleRadius && distance <= (circleRadius * circleRadius + circleRadius * 2))
                    Tiles[x, y] = new Tile(null, new Vector2Int(x, y), Tile.TileType.Border);
                else if (distance < circleRadius * circleRadius)
                    Tiles[x, y] = new Tile(null, new Vector2Int(x, y), Tile.TileType.Wall);
                else
                    Tiles[x, y] = new Tile(null, new Vector2Int(x, y), Tile.TileType.Nothing);
            }
    }

    public void Reset(Vector2Int size, bool isCircle)
    {
        IsReady = false;
        _isCircle = isCircle;
        Size = size;
        Tiles = new Tile[Size.x, Size.y];
        if (_isCircle) InitCircle();
        else InitSquare();
    }

    public void ReverseTiles()
    {
        int rows = Tiles.GetLength(0);
        int cols = Tiles.GetLength(1);

        Tile[,] reversedArray = new Tile[rows, cols];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                reversedArray[i, j] = Tiles[rows - 1 - i, cols - 1 - j];

        TilesReversed = reversedArray;
    }

    public void SetTileRegion(Tile tile, int region)
    {
        Tiles[tile.Position.x, tile.Position.y].RegionId = region;
    }

    public void SetTileObject(Tile tile, GameObject tileObject)
    {
        Tiles[tile.Position.x, tile.Position.y].SetTileObject(tileObject);
    }

    public void SetTileMinimapObjectActive(Tile tile, bool active)
    {
        Tiles[tile.Position.x, tile.Position.y].SetTileMinimapObjectActive(active);
    }

    public bool CanCarve(Tile fromTile, Vector2Int direction)
    {
        if (!CheckNextTile(fromTile, direction, 2))
            return false;

        return GetNextTile(fromTile, direction, 2).Type == Tile.TileType.Wall;
    }

    public void Carve(Tile tile, int region, Tile.TileType type = Tile.TileType.Floor)
    {
        SetTileType(tile, type);
        SetTileRegion(tile, region);
    }

    public Tile GetTile(Vector2Int position)
    {
        return Tiles[position.x, position.y];
    }

    public Tile GetExitTile()
    {
        return Tiles[ExitTile.Position.x, ExitTile.Position.y];
    }

    public Tile GetNextTile(Tile fromTile, Vector2Int direction, int multiplier)
    {
        var bounds = GetBounds(fromTile, direction, multiplier);
        return Tiles[bounds.x, bounds.y];
    }

    public void SetTileType(Tile tile, Tile.TileType type)
    {
        Tiles[tile.Position.x, tile.Position.y].Type = type;
    }

    public void SetNextTileType(Tile fromTile, Vector2Int direction, int multiplier, Tile.TileType type)
    {
        var bounds = GetBounds(fromTile, direction, multiplier);
        Tiles[bounds.x, bounds.y].Type = type;
    }

    public bool CheckNextTile(Tile fromTile, Vector2Int direction, int multiplier)
    {
        var bounds = GetBounds(fromTile, direction, multiplier);

        if (bounds.x >= Size.x || bounds.x <= 0 || bounds.y >= Size.y || bounds.y <= 0)
            return false;

        return true;
    }

    public bool CheckNextTileType(Tile fromTile, Vector2Int direction, int multiplier, Tile.TileType type)
    {
        var bounds = GetBounds(fromTile, direction, multiplier);

        if (bounds.x > Size.x || bounds.x < 0 || bounds.y > Size.y || bounds.y < 0)
            return false;

        if (Tiles[bounds.x, bounds.y].Type == type)
            return true;

        return false;
    }

    public int GetNeighboursNumber(Tile tile)
    {
        int total = 0;

        foreach (var direction in HelperClass.Directions)
        {
            if (!CheckNextTileType(tile, direction, 1, Tile.TileType.Floor))
                continue;
            total++;
        }

        return total;
    }

    public int GetWallNeighboursNumber(Tile tile)
    {
        int total = 0;

        foreach (var direction in HelperClass.Directions)
        {
            if (!CheckNextTileType(tile, direction, 1, Tile.TileType.Wall))
                continue;
            total++;
        }

        return total;
    }

    Vector2Int GetBounds(Tile fromTile, Vector2Int direction, int multiplier)
    {
        return new Vector2Int(fromTile.Position.x + direction.x * multiplier, fromTile.Position.y + direction.y * multiplier);
    }
}
