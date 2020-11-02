using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProfile
{
    public GameMode GameModeCtrl;
    public GoldAccount GoldAccount;
    public PlayerProfile()
    {

    }

    public void InitNewGame(GameMode.GameModeType type)
    {
        GameModeCtrl = GameMode.CreateGameMode(type);
        switch (type)
        {
            case GameMode.GameModeType.Demo:
                GoldAccount = new GoldAccountModeDemo(190f);
                break;
            case GameMode.GameModeType.LTE:
                GoldAccount = new GoldAccountModeLTE(190f);
                break;
            case GameMode.GameModeType.Test:
                GoldAccount = new GoldAccountModeDemo(10000f);
                break;
            default:
                GoldAccount = new GoldAccountModeDemo(190f);
                break;
        }
    }

    public void LoadGame(string name)
    {

    }

    public string[] AllSavedGame()
    {
        return null;
    }
}
