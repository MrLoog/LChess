using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GameMode
{
    public enum GameModeType
    {
        Demo, LTE, Test
    }

    public GameModeType Type;
    public RoundManager RoundManager;

    public GoldAccount GoldAccount { get; internal set; }

    public static GameMode CreateGameMode(GameModeType type)
    {
        GameMode gameMode = null;
        switch (type)
        {
            case GameModeType.Demo:
                gameMode = new GameModeDemo();
                break;
            case GameModeType.LTE:
                gameMode = new GameModeLTE();
                break;
            case GameModeType.Test:
                gameMode = new GameModeTest();
                break;
            default:
                gameMode = new GameModeTest();
                break;
        }
        gameMode.Type = type;
        return gameMode;
    }

    public virtual RoundManager InitRoundManager()
    {
        RoundManager roundManager = null;
        switch (Type)
        {
            case GameModeType.Demo:
                roundManager = new RoundManagerModeDemo();
                break;
            case GameModeType.LTE:
                roundManager = new RoundManegerModeLTE();
                break;
            case GameModeType.Test:
                roundManager = new RoundManagerModeDemo();
                break;
            default:
                roundManager = new RoundManagerModeDemo();
                break;
        }
        roundManager.GoldAccount = GoldAccount;
        roundManager.Init();
        RoundManager = roundManager;
        roundManager.RegisterRoundEvent(Round.EVENT_SPAWN_ENEMY, CheckNumberUnit);
        return roundManager;
    }

    public virtual void CheckNumberUnit()
    {
        List<ActionUnit> UserBattleUnits = Game.Instance.board.GetBattleTilesGroup(-1).Where(x => x.ActionUnit != null).Select(x => x.ActionUnit).ToList();
        if (UserBattleUnits.Count > GetMaxSpawn())
        {
            int overNum = UserBattleUnits.Count - GetMaxSpawn();
            List<ActionUnit> returnUnit = UserBattleUnits.OrderBy(x => x.CurrentStatus.Level).Take(overNum).ToList();
            for (int i = 0; i < returnUnit.Count; i++)
            {
                returnUnit[i].JumpTo(Game.Instance.board.GetPrepareTile());
            }
        }
    }

    public virtual int GetMaxSpawn()
    {
        return 0;
    }

    public virtual float GetSellPriceUnit(ActionUnit sellUnit)
    {
        return 0f;
    }

    public virtual bool SellUnit(ActionUnit focusUnit)
    {
        if (focusUnit != null)
        {
            Game.Instance.DestroyUnit(focusUnit);
            return true;
        }
        return false;
    }

    public virtual float GetShopPrice()
    {
        return 0f;
    }

    public virtual bool DeductGoldForShop()
    {
        return true;
    }
}
