using System;
using System.Collections.Generic;
using UnityEngine;

public struct Tile
{
    public enum TileType
    {
        Floor,
        Room,
        Wall,
        Nothing,
        Border,
        Carpet
    }

    public TileType Type;
    public Vector2Int Size;
    public Vector2Int Position;
    public int RegionId;
    public List<int> ConnectorRegions;
    public bool Exposed;
    public GameObject tileObject;
    public GameObject tileMinimapObject;

    public Tile(Vector2Int? size, Vector2Int position, TileType type = TileType.Wall, int regionId = -1)
    {
        if (size == null)
            Size = new Vector2Int(1, 1);
        else
            Size = size.GetValueOrDefault();

        Type = type;
        Position = position;
        RegionId = regionId;
        Exposed = false;
        tileObject = null;
        tileMinimapObject = null;
        ConnectorRegions = new List<int>();
    }

    public bool IsClose(Tile tile, int precision)
    {
        return Mathf.Abs(tile.Position.x - Position.x) < precision && Mathf.Abs(tile.Position.y - Position.y) < precision;
    }

    public void SetTileObject(GameObject newTileObject)
    {
        if (Type == Tile.TileType.Wall) return;

        tileObject = newTileObject;
        tileMinimapObject = tileObject.transform.GetChild(0).gameObject;
    }

    public void SetTileMinimapObjectActive(bool active)
    {
        tileMinimapObject.SetActive(active);
        Exposed = active;
    }
}
