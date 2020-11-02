using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode
{
    public enum GameModeType
    {
        Demo, LTE, Test
    }

    public static GameMode CreateGameMode(GameModeType type)
    {
        switch (type)
        {
            case GameModeType.Demo:
                return new GameModeDemo();
            case GameModeType.LTE:
                return new GameModeLTE();
            case GameModeType.Test:
                return new GameModeTest();
            default:
                return new GameModeTest();
        }
    }
}
