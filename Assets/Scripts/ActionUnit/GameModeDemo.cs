using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static Buff;

public class GameModeDemo : GameMode
{
    public override RoundManager InitRoundManager()
    {
        RoundManager roundManager = base.InitRoundManager();

        roundManager.Events.RegisterListener(RoundManager.EVENT_BONUS_DONE).AddListener(RefreshShopFree);
        roundManager.Events.RegisterListener(RoundManager.EVENT_ROUND_PREPARE).AddListener(RefreshButtonState);

        roundManager.RegisterRoundEvent(Round.EVENT_APPLY_BUFF, AddRoundBuff);
        roundManager.RegisterRoundEvent(Round.EVENT_SPAWN_ENEMY, SpawnEnemy);
        roundManager.BuildNextRound = DesignateNextRound;
        FormationManager.Instance.Events.RegisterListener(FormationManager.EVENT_CHECK_BEFORE).AddListener(MakeFormationUserApplyOnly);


        return roundManager;
    }

    private void DesignateNextRound()
    {
        //only call for level 1+
        if (RoundManager.Round.Result == Round.RoundResult.Win)
        {
            RoundManager.Round = RoundManager.Plan.GetNextRound(Game.USER_GROUP, RoundManager.Round.Level);
        }
        else
        {
            RoundManager.Round = RoundManager.Plan.GetNextRound(Game.USER_GROUP, RoundManager.Round.Level - 1);
        }

    }

    private void AddRoundBuff()
    {
        // if (RoundManager.Round.ApplyBuffs != null)
        // {
        //     RoundManager.Round.ApplyBuffs.Clear();
        // }
        if (RoundManager.Round.ApplyBuffs != null)
        {
            RoundManager.Round.ApplyBuffs.AddRange(BuildBuffForRound());
        }
        else
        {
            RoundManager.Round.ApplyBuffs = BuildBuffForRound();
        }
    }

    private void SpawnEnemy()
    {
        RoundManager.Round.SpawnEnemy = SpawnEnemyMirror;
    }

    private void SpawnEnemyMirror(Round r)
    {
        GameTile spawnTile = null;
        int group = 1;
        List<ActionUnit> units = ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile).ToList();
        int maxLevel = -1;
        int existsMaxLevel = 1;
        maxLevel = 1 + (Game.Instance.RoundManager.Round.Level <= 10 ? 0 : +Mathf.CeilToInt((Game.Instance.RoundManager.Round.Level - 10f) / 10f));
        TileUnitData dataSpawn = null;
        int spawnCount = Game.Instance.Profile.GameModeCtrl.GetMaxSpawn();
        foreach (ActionUnit unit in units)
        {
            spawnTile = Game.Instance.board.GetRandomEmptyTileGroup(group);
            if (spawnTile != null)
            {
                dataSpawn = unit.tileUnitData;
                int level = ((ActionUnitData)unit.tileUnitData).Level;
                if (maxLevel != -1 &&
                level > maxLevel)
                {
                    existsMaxLevel = units.Select(x => x.tileUnitData).Where(x => ((ActionUnitData)x).Level <= maxLevel).OrderByDescending(x => ((ActionUnitData)x).Level).Select(x => ((ActionUnitData)x).Level).FirstOrDefault();
                    dataSpawn = Game.Instance.RandomFromList(units.Select(x => x.tileUnitData).Cast<ActionUnitData>().ToList(), existsMaxLevel);

                }
                Game.Instance.SpawnMonster(spawnTile, group, dataSpawn);
                spawnCount--;
            }
        }
        if (spawnCount > 0)
        {
            for (int i = spawnCount; i > 0; i--)
            {
                spawnTile = Game.Instance.board.GetRandomEmptyTileGroup(group);
                if (spawnTile != null)
                {
                    dataSpawn = Game.Instance.RandomFromList(units.Select(x => x.tileUnitData).Cast<ActionUnitData>().ToList());
                    Game.Instance.SpawnMonster(spawnTile, group, dataSpawn);
                }
            }
        }
        MainMenuControl.Instance.ScanAndShow(true);
    }

    private void MakeFormationUserApplyOnly()
    {
        FormationManager.Instance.GroupChecks.RemoveAll(x => x != Game.USER_GROUP);
    }

    public List<BuffFacade> BuildBuffForRound()
    {
        List<BuffFacade> buffs = new List<BuffFacade>();
        int nR = RoundManager.Round.Level;
        if (nR <= 10)
        {
            BuffFacade facade = BuffFacade.CreateFromBuff((Buff)ScriptableObject.CreateInstance(typeof(Buff)));
            facade.Buff.Target = Buff.TargetType.Ally;
            facade.Buff.Effect = Buff.EffectType.Decrease;
            facade.Buff.Stat = Buff.StatAffect.Random;
            facade.Buff.Percent = 30f - ((nR - 1) * 3f);
            if (facade.Buff.Percent > 0) buffs.Add(facade);
        }
        else
        {
            List<StatAffect> stats = new List<StatAffect>();
            float maxPercent = (nR - 10f) * 1f;
            while (maxPercent > 0)
            {
                StatAffect randomStat = BuffFacade.RandomStatAffect(null, stats);
                BuffFacade facade = BuffFacade.CreateFromBuff((Buff)ScriptableObject.CreateInstance(typeof(Buff)));
                facade.Buff.Target = Buff.TargetType.Ally;
                facade.Buff.Effect = Buff.EffectType.Increase;
                if (randomStat == Buff.StatAffect.Random)
                {
                    buffs[buffs.Count - 1].Buff.Percent += maxPercent;
                    maxPercent = 0;
                    break;
                }
                else
                {
                    stats.Add(randomStat);
                    facade.Buff.Stat = randomStat;
                }
                facade.Buff.Percent = (maxPercent <= 1 ? maxPercent : (int)Random.Range(1f, maxPercent));
                maxPercent -= facade.Buff.Percent;
                buffs.Add(facade);
            }

        }
        return buffs;
    }

    public void RefreshShopFree()
    {
        UnitShop.Instance.ShowShop();
    }

    public void RefreshButtonState()
    {
        MainMenuControl.Instance.ShowGameState(false);
    }

    public override int GetMaxSpawn()
    {
        // return 10;
        int cnr = RoundManager.Round.Level;
        int slot = 1 +
        (cnr <= 10
        ? Mathf.RoundToInt((cnr - 1) / 3f)
        : (3 + Mathf.RoundToInt((cnr - 10) / 5f)));
        return slot < 10 ? slot : 10;
    }

    public override float GetSellPriceUnit(ActionUnit sellUnit)
    {
        ActionUnitData data = (ActionUnitData)sellUnit.tileUnitData;
        int level = data.Level;
        while (data.PrevLevel != null)
        {
            data = (ActionUnitData)data.PrevLevel;
        }
        float price = (level == 1 ? 0.8f : 1f) * (level == 2 ? 2.5f : 1f) * (level == 3 ? 7.5f : 1f) * data.gold;
        return price;
    }

    public override bool SellUnit(ActionUnit sellUnit)
    {
        float price = GetSellPriceUnit(sellUnit);
        if (base.SellUnit(sellUnit))
        {
            GoldAccount.ApplyAdd(price);
            return true;
        }
        return false;
    }


    public override float GetShopPrice()
    {
        return 50f;
    }

    public override bool DeductGoldForShop()
    {
        if (GoldAccount.ApplyDeduct(GetShopPrice()) >= 0)
        {
            return true;
        }
        else
        {
            MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_GOLD_INVALID, 1f);
            GoldAccount.ApplyAdd(GetShopPrice());
            return false;
        }
    }
}
