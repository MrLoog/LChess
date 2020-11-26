using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager
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

    public const string EVENT_ROUND_START = "R_START";
    public const string EVENT_ROUND_PREPARE = "R_PRE";
    public const string EVENT_ROUND_END = "R_END";
    public const string EVENT_BONUS_BEFORE = "BONUS_BEFORE";
    public const string EVENT_BONUS_DONE = "BONUS_DONE";

    public Dictionary<string, List<UnityAction>> RoundEvents;

    public RoundPlan Plan;


    List<Round> Rounds;
    public Round Round { get; set; }
    public List<ActionUnit> MainUnits;
    public List<ActionUnit> BattleUnits;

    public UnityAction BuildNextRound;

    public GoldAccount GoldAccount;



    public void Init()
    {
        Rounds = new List<Round>();
        Plan = Game.Instance.Plan;
        PrepareRound(Game.USER_GROUP, null);
    }

    public bool StartNewRound(int mainGroup, List<ActionUnit> battleUnits)
    {
        if (Rounds == null)
        {
            Init();
        }
        Events.InvokeOnAction(EVENT_ROUND_START);
        Round?.Events.RegisterListener(Round.EVENT_RESULT).AddListener(ResultRound);
        Round?.Events.RegisterListener(Round.EVENT_RESTORE_DONE).AddListener(EndRound);
        Round.StartRound(Game.USER_GROUP);
        MainMenuControl.Instance.RoundTime = Round.RoundTime;
        MainMenuControl.Instance.RoundNumber = Rounds.Count();
        MainMenuControl.Instance.RoundLevel = Round.Level;
        Debug.Log("TotalRound " + Rounds.Count());
        return true;
    }

    public bool PrepareRound(int mainGroup, List<ActionUnit> battleUnits)
    {
        Round prevRound = Round;
        if (Plan != null)
        {
            if (BuildNextRound != null)
            {
                BuildNextRound.Invoke();
            }
            else
            {
                Round = Plan.GetNextRound(mainGroup, Round?.Level != null ? Round.Level : 0);
            }
        }
        else
        {
            Round = new Round(mainGroup, battleUnits);
        }
        MoveRoundListener(prevRound, Round);
        Rounds.Add(Round);
        Round.RoundNumber = Rounds.Count();
        Events.InvokeOnAction(EVENT_ROUND_PREPARE);
        return true;
    }


    internal void ResultRound()
    {
        Debug.Log("Round RoundManager Result");
        Debug.Assert(Round.CurPhase == Round.RoundPhase.Result, "Round must be result phase");
        Events.InvokeOnAction(EVENT_ROUND_END);
        CalculateBonus();
        // Round = null;

        Round?.Events.RemoveListener(Round.EVENT_RESULT);
    }

    internal void EndRound()
    {
        Debug.Log("Round RoundManager End");
        Debug.Assert(Round.CurPhase == Round.RoundPhase.End, "Round must be end phase");
        // Round = null;

        Round?.Events.RemoveListener(Round.EVENT_RESTORE_DONE);
        PrepareRound(Game.USER_GROUP, null);
    }

    private void CalculateBonus()
    {
        Events.InvokeOnAction(EVENT_BONUS_BEFORE);
        GoldAccount.ApplyEndRound(Rounds.Count, Round.TimeLeft, Round.EndData.Count);
        int steak = 1;
        for (int i = Rounds.Count - 1; i >= 0; i--)
        {
            if (i - 1 < 0 || Rounds[i].Result != Rounds[i - 1].Result)
            {
                if (Rounds[i].Result == Round.RoundResult.Win)
                {
                    GoldAccount.ApplySteackWin(steak);
                }
                else if (Rounds[i].Result == Round.RoundResult.Lose)
                {
                    GoldAccount.ApplySteackLose(steak);
                }
                break;
            }
            else
            {
                steak++;
            }
        }
        Events.InvokeOnAction(EVENT_BONUS_DONE);
    }

    internal void Reset()
    {
        Rounds?.Clear();
        Init();
    }

    public bool RegisterRoundEvent(string roundEventName, UnityAction e)
    {
        if (RoundEvents == null)
        {
            RoundEvents = new Dictionary<string, List<UnityAction>>();
        }
        if (!RoundEvents.Keys.Contains(roundEventName))
        {
            RoundEvents.Add(roundEventName, new List<UnityAction>());
        }
        RoundEvents[roundEventName].Add(e);
        if (Round != null)
        {
            Round.Events.RegisterListener(roundEventName).AddListener(e);
        }
        return true;
    }

    public bool RemoveRoundEvent(string roundEventName, UnityAction action = null)
    {
        if (RoundEvents != null && RoundEvents.Keys.Contains(roundEventName))
        {
            if (Round != null)
            {
                if (action == null)
                {
                    foreach (UnityAction a in RoundEvents[roundEventName])
                    {
                        Round.Events.RegisterListener(roundEventName).RemoveListener(a);
                    }
                }
                else
                {
                    Round.Events.RegisterListener(roundEventName).RemoveListener(action);
                }
            }
            if (action == null)
                RoundEvents.Remove(roundEventName);
            else
            {
                RoundEvents[roundEventName].Remove(action);
            }
        }
        return true;
    }

    private bool MoveRoundListener(Round removeRound, Round addRound)
    {
        if (RoundEvents != null && RoundEvents.Count > 0)
        {
            foreach (KeyValuePair<string, List<UnityAction>> p in RoundEvents)
            {
                if (removeRound != null)
                {
                    foreach (UnityAction a in RoundEvents[p.Key])
                    {
                        removeRound.Events.RegisterListener(p.Key).RemoveListener(a);
                    }
                }
                if (addRound != null)
                {
                    foreach (UnityAction a in RoundEvents[p.Key])
                    {
                        addRound.Events.RegisterListener(p.Key).AddListener(a);
                    }
                }
            }
        }
        return true;
    }
}
