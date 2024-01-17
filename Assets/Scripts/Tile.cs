using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Floor,
        Room,
        Wall,
        Nothing
    }

    public TileType Type;
    public Vector2Int Size;
    public Vector2Int Position;
    public int RegionId;
    public List<int> ConnectorRegions;

    public Tile(Vector2Int? size, Vector2Int position, TileType type = TileType.Wall, int regionId = -1)
    {
        if (size == null)
            Size = new Vector2Int(1, 1);
        else
            Size = size.GetValueOrDefault();

        Type = type;
        Position = position;
        RegionId = regionId;
        ConnectorRegions = new List<int>();
    }

    public bool IsClose(Tile tile, int precision)
    {
        return Mathf.Abs(tile.Position.x - Position.x) < precision && Mathf.Abs(tile.Position.y - Position.y) < precision;
    }
}
