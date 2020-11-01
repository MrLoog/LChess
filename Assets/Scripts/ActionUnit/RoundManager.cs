using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager
{

    public RoundPlan Plan;
    private static RoundManager _instance = new RoundManager();
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

    }

    private void Init()
    {
        Rounds = new List<Round>();
        GoldAccount = new GoldAccount(1000f);
        Plan = Game.Instance.Plan;
        MainGroup = 0;
        PrepareRound(MainGroup, null);
    }

    public bool StartNewRound(int mainGroup, List<ActionUnit> battleUnits)
    {
        if (Rounds == null)
        {
            Init();
        }
        Round.StartRound(mainGroup);
        MainMenuControl.Instance.RoundTime = Round.RoundTime;
        MainMenuControl.Instance.Round = Rounds.Count();
        Debug.Log("TotalRound " + Rounds.Count());
        return true;
    }

    public bool PrepareRound(int mainGroup, List<ActionUnit> battleUnits)
    {
        if (Plan != null)
        {
            Round = Plan.GetNextRound(mainGroup, Rounds.Count);
        }
        else
        {
            Round = new Round(mainGroup, battleUnits);
        }
        MainGroup = mainGroup;
        Rounds.Add(Round);
        Round.RoundNumber = Rounds.Count();
        Debug.Log("TotalRound1 " + Rounds.Count());
        return true;
    }


    internal void EndRound()
    {
        Debug.Log("Round RoundManager End");
        Debug.Assert(Round.CurPhase == Round.RoundPhase.End, "Round must be end phase");
        CalculateBonus();
        // Round = null;

        Game.Instance.OnGame = false;
        PrepareRound(MainGroup, null);
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
        Rounds?.Clear();
        Init();
    }
}
