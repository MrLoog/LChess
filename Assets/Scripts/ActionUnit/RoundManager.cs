using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager
{

    public RoundPlan Plan;
    private static RoundManager _instance;
    public static RoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new RoundManager();
            }
            return _instance;
        }
    }

    List<Round> Rounds;
    public Round Round { get; private set; }
    public List<ActionUnit> MainUnits;
    public List<ActionUnit> BattleUnits;

    public GoldAccount GoldAccount;

    public int MainGroup;

    private RoundManager()
    {
        Rounds = new List<Round>();
        GoldAccount = new GoldAccount(190f);
        Plan = Game.Instance.Plan;
    }

    public bool StartNewRound(int mainGroup, List<ActionUnit> battleUnits)
    {
        if (Plan != null)
        {
            Round = Plan.GetNextRound(mainGroup,Rounds.Count);
        }
        else
        {
            Round = new Round(mainGroup, battleUnits);
        }
        MainGroup = mainGroup;
        Rounds.Add(Round);
        Round.RoundNumber = Rounds.Count();
        Round.StartRound(mainGroup);
        MainMenuControl.Instance.RoundTime = Round.RoundTime;
        MainMenuControl.Instance.Round = Rounds.Count;
        return true;
    }

    

    internal void EndRound()
    {
        Debug.Log("Round RoundManager End");
        Debug.Assert(Round.CurPhase == Round.RoundPhase.End, "Round must be end phase");
        CalculateBonus();
        Round = null;
        
        Game.Instance.OnGame = false;
    }

    private void CalculateBonus()
    {
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
    }

    internal void Reset()
    {
        Rounds.Clear();
    }
}
