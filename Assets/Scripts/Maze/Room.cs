using UnityEngine;

public struct Room
{
    public Vector2Int Size;
    public Vector2Int Position;

    public Room(Vector2Int? size, Vector2Int position)
    {
        if (size == null)
            Size = new Vector2Int(2, 2);
        else
            Size = size.GetValueOrDefault();

        Position = position;
    }

    public bool Overlaps(Room room, int margin)
    {
        return (
            Position.x < room.Position.x + room.Size.x + margin && 
            Position.x + Size.x + margin > room.Position.x && 
            Position.y < room.Position.y + room.Size.y + margin && 
            Position.y + Size.y + margin > room.Position.y
        );
    }
}
