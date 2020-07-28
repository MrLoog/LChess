using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North, NorthEast, East, EastSouth, South, SouthWest, West, WestNorth
}
public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    public Monster Monster;

    public GameTile NTile, NETile, ETile, ESTile, STile, SWTile, WTile, WNTile;

    public List<GameTile> FindNearBy(Direction directions)
    {
        List<GameTile> result = new List<GameTile>();
        if ((directions & Direction.North) == Direction.North)
        {
            // result.Add()
        }
        return result;
    }

    public int FindEnemyTileDirection(Direction direction, int group, int distance)
    {
        Debug.Log(String.Format("Direction {0};{1},{2},{3}", direction, transform.position.x, transform.position.y, transform.position.z));
        ++distance;
        if (Monster != null && Monster.Group != group) return distance;
        GameTile tile = GetTileDirection(direction);
        if (tile == null) return -1;
        Debug.DrawLine(transform.position, tile.transform.position, Color.blue, 30000);
        return tile.FindEnemyTileDirection(direction, group, distance);
    }

    private GameTile GetTileDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return NTile;
            case Direction.NorthEast: return NETile;
            case Direction.East: return ETile;
            case Direction.EastSouth: return ESTile;
            case Direction.South: return STile;
            case Direction.SouthWest: return SWTile;
            case Direction.West: return WTile;
            case Direction.WestNorth: return WNTile;
            default: return null;
        }
    }
}
