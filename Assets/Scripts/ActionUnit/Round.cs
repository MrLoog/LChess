using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Round", menuName = "LChess/Round", order = 3)]
[System.Serializable]
public class Round : ScriptableObject
{
    public enum RoundPhase
    {
        NotStart, Battle, End
    }

    public enum RoundEnemy
    {
        Mirror, Player, Boss
    }

    public enum RoundResult
    {
        NoInfo, Win, Lose, DrawTimeout, DrawBattle
    }

    public string ExpressionNumberBuff;

    public int RoundNumber;
    public Buff[] Buffs = default;
    // public List<ActionUnit> BattleUnit { get; private set; }
    public List<TileUnitData> StartData;
    public List<TileUnitData> EndData;

    public RoundEnemy EnemySource;

    public int UserGroup;
    private int _winGroup;
    public int WinGroup
    {
        get
        {
            return _winGroup;
        }
        private set
        {
            _winGroup = value;
        }
    }

    public RoundPhase CurPhase;
    public RoundResult Result;
    public DateTime StartTime;
    public float RoundTime = 30f;
    public float TimeLeft = 30f;

    public Round(int mainGroup, List<ActionUnit> battleUnit)
    {
        UserGroup = mainGroup;
        BattleUnits = battleUnit;
        CurPhase = RoundPhase.NotStart;
        StartData = battleUnit.Select(a => a.tileUnitData).ToList();
    }

    public Round()
    {
        CurPhase = RoundPhase.NotStart;
    }
    public List<ActionUnit> MainUnits;
    public List<ActionUnit> BattleUnits;

    private void ApplyBuff()
    {
        List<Buff> buffs=new List<Buff>();
        buffs.AddRange(Buffs);
        if (ExpressionNumberBuff.Length > 0)
        {
            string parseExp = ExpressionNumberBuff.Replace("R", RoundNumber.ToString());
            int rs = 0;
            if (ExpressionEvaluator.Evaluate<int>(parseExp, out rs))
            {
                for (int i = 0; i < rs; i++)
                {
                    buffs.Add(Buff.RandomPositiveBuff());
                }
            }
        }
        foreach (Buff b in buffs)
        {
            b.PerformBuff(EnemyGroup);
        }
    }

    public bool StartRound(int mainGroup)
    {
        UserGroup = mainGroup;

        if (EnemySource == RoundEnemy.Mirror)
        {
            Game.Instance.MirrorSpawn();
        }
        BattleUnits = ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile).ToList();
        Debug.Log("Battle Unit "+BattleUnits.Count() );
        StartData = BattleUnits.Select(a => a.tileUnitData).ToList();
        MainUnits = BattleUnits.Where(a => a.Group == mainGroup).ToList();
        ApplyBuff();
        MainUnits.ForEach(a =>
        {
            a.IsRealDestroy = false;
            ActionUnitData newData = new ActionUnitData();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(a.CurrentStatus), newData);
            a.SavedStatus = newData;
            a.SavedStatus.position = a.transform.localPosition;
        });

        return StartRound();
    }


    public bool StartRound()
    {
        CurPhase = RoundPhase.Battle;
        Result = RoundResult.NoInfo;
        StartTime = System.DateTime.Now;
        BattleUnits.ForEach(a => a.BattleMode = true);
        return true;
    }

    private bool OnlyGroup => BattleUnits.Where(x => x.Alive).Select(a => a.Group).Distinct().Count() == 1;
    private int EnemyGroup => BattleUnits.Where(x => x.Alive && x.Group != UserGroup).Select(a => a.Group).FirstOrDefault();

    public bool ValidEndGame(bool autoEnd)
    {
        if (CurPhase != RoundPhase.Battle) return false;
        bool isEnd =
        System.DateTime.Now.Subtract(StartTime).TotalSeconds > RoundTime || (BattleUnits.Where(x => x.Alive).ToList().Count == 0) || OnlyGroup;
        if (isEnd && autoEnd)
        {
            EndRound();
        }
        return isEnd;
    }

    public bool EndRound()
    {
        EndData = BattleUnits.Where(x => x.Alive).Select(a => a.tileUnitData).ToList();
        TimeLeft = (float)System.DateTime.Now.Subtract(StartTime).TotalSeconds;
        if (OnlyGroup)
        {
            WinGroup = BattleUnits.Where(x => x.Alive).FirstOrDefault().Group;
            if (UserGroup == WinGroup)
            {
                Result = RoundResult.Win;
                MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_WINNER, 1f);
            }
            else
            {
                Result = RoundResult.Lose;
                MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_LOSE, 1f);
            }
        }
        else
        {
            MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_DRAW, 1f);
            if (EndData.Count == 0)
            {
                Result = RoundResult.DrawBattle;
            }
            else
            {
                Result = RoundResult.DrawTimeout;
            }
        }
        CurPhase = RoundPhase.End;
        Debug.Log("Battle Unit "+BattleUnits.Count() );

        BattleUnits.Where(x => x.Alive).ToList().ForEach(x => x.BattleMode = false);
        Game.Instance.StartCoroutine(Restore(5f));

        return true;
    }
    public IEnumerator Restore(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Battle Unit "+BattleUnits.Count() );
        MainMenuControl.Instance.RoundTime = 0;
        BattleUnits.Where(a => a.Group != UserGroup).ToList().ForEach(x => x.Destroy());
        MainUnits.ForEach(a =>
        {
            a.CurrentStatus = a.SavedStatus;
            a.transform.localPosition = a.SavedStatus.position;
            a.BattleMode = false;
            a.gameObject.SetActive(true);
            a.Revive();
        });
        MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_PHASE_PREPARE, 1f);
        BattleUnits = null;
        MainUnits = null;
        yield return null;
    }

}
