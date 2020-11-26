using System;
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

#if UNITY_EDITOR
                GoldAccount = new GoldAccountModeDemo(1000000f);
#else
                GoldAccount = new GoldAccountModeDemo(190f);
#endif
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
        GameModeCtrl.GoldAccount = GoldAccount;
    }

    internal RoundManager InitRoundManager()
    {
        return GameModeCtrl.InitRoundManager();
    }

    public bool LoadProfile(string name)
    {
        return true;
    }

    public bool SaveProfile(string name)
    {
        return true;
    }

    public string[] AllSavedGame()
    {
        return null;
    }
}
