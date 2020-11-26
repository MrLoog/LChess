using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Round", menuName = "LChess/Round", order = 3)]
[System.Serializable]
public class Round : MScriptableObject
{
    private EventDict _events;
    public EventDict Events
    {
        get
        {
            if (_events == null) _events = new EventDict();
            return _events;
        }
    }

    public const string EVENT_SPAWN_ENEMY = "SPAWN";
    public const string EVENT_APPLY_BUFF = "APPLY";
    public const string EVENT_RESULT = "RESULT";
    public const string EVENT_RESTORE_DONE = "RESTORE";

    public UnityAction<Round> SpawnEnemy;
    public enum RoundPhase
    {
        NotStart, Battle, Result, End
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
    public int Level;
    public Buff[] Buffs = default;
    // public List<ActionUnit> BattleUnit { get; private set; }
    public List<ActionUnitData> StartData;
    public List<ActionUnitData> EndData;

    public List<BuffFacade> ApplyBuffs;

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
        StartData = battleUnit.Select(a => a.CurrentStatus).ToList();
    }

    public Round()
    {
        CurPhase = RoundPhase.NotStart;
    }
    public List<ActionUnit> MainUnits;
    public List<ActionUnit> BattleUnits;

    private void ApplyBuff()
    {
        ApplyBuffs = new List<BuffFacade>();
        Buffs.ToList().ForEach(x =>
        {
            ApplyBuffs.Add(BuffFacade.CreateFromBuff(x));
        });
        // if (ExpressionNumberBuff.Length > 0)
        // {
        //     string parseExp = ExpressionNumberBuff.Replace("R", RoundNumber.ToString());
        //     int rs = 0;
        //     rs = 1;
        //     // if (ExpressionEvaluator.Evaluate<int>(parseExp, out rs))
        //     // {
        //     for (int i = 0; i < rs; i++)
        //     {
        //         ApplyBuffs.Add(BuffFacade.RandomPositiveBuff());
        //     }
        //     // }
        // }
        Events.InvokeOnAction(EVENT_APPLY_BUFF);
        foreach (BuffFacade b in ApplyBuffs)
        {
            b.PerformBuff(EnemyGroup);
        }
    }

    public bool StartRound(int mainGroup)
    {
        UserGroup = mainGroup;
        Events.InvokeOnAction(EVENT_SPAWN_ENEMY);
        if (SpawnEnemy != null)
        {
            SpawnEnemy.Invoke(this);
        }
        else
        {
            if (EnemySource == RoundEnemy.Mirror)
            {
                Game.Instance.MirrorSpawn();
            }
        }
        BattleUnits = ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile).ToList();
        ApplyBuff();
        StartData = BattleUnits.Select(a => a.CurrentStatus).ToList();
        MainUnits = BattleUnits.Where(a => a.Group == mainGroup).ToList();
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


    private bool StartRound()
    {
        CurPhase = RoundPhase.Battle;
        Result = RoundResult.NoInfo;
        StartTime = System.DateTime.Now;
        TimeLeft = RoundTime;
        BattleUnits.ForEach(a => a.BattleMode = true);
        return true;
    }

    private bool OnlyGroup => BattleUnits.Where(x => x.Alive).Select(a => a.Group).Distinct().Count() == 1;
    private int EnemyGroup => BattleUnits.Where(x => x.Alive && x.Group != UserGroup).Select(a => a.Group).FirstOrDefault();

    public bool ValidEndGame(bool autoEnd)
    {
        if (CurPhase != RoundPhase.Battle) return false;

        TimeLeft -= Time.deltaTime;
        bool isEnd =
        TimeLeft <= 0 || (BattleUnits.Where(x => x.Alive).ToList().Count == 0) || OnlyGroup;
        if (isEnd && autoEnd)
        {
            EndRound();
        }
        return isEnd;
    }

    public bool EndRound()
    {
        EndData = BattleUnits.Where(x => x.Alive).Select(a => a.CurrentStatus).ToList();
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
        CurPhase = RoundPhase.Result;
        Events.InvokeOnAction(EVENT_RESULT);
        Debug.Log("Battle Unit " + BattleUnits.Count());

        BattleUnits.Where(x => x.Alive).ToList().ForEach(x => x.BattleMode = false);
        Game.Instance.StartCoroutine(Restore(3f));

        return true;
    }
    public IEnumerator Restore(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Battle Unit " + BattleUnits.Count());
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
        CurPhase = RoundPhase.End;
        Events.InvokeOnAction(EVENT_RESTORE_DONE);
        yield return null;
    }

    internal int GetMaxSpawn()
    {
        return Mathf.CeilToInt(Level / 2f);
        // return 10;
    }

}
