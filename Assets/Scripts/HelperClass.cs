using System.Collections.Generic;
using UnityEngine;

public static class HelperClass
{
    public static List<Vector2Int> Directions = 
        new() { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left }; 
}
