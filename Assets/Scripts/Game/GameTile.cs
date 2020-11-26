using System;
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

    public ActionUnit ActionUnit;
    public bool PrepareTile;

    public bool Empty => ActionUnit == null;

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
