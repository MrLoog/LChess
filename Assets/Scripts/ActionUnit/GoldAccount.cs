using UnityEngine;

public class GoldAccount
{
    private float _amount;
    public float Amount
    {
        get
        {
            return _amount;
        }
        private set
        {
            MainMenuControl.Instance.Balance = value;
            _amount = value;
        }
    }
    public float BonusRound = 10f;
    public float BonusTime = 0.5f;
    public float BonusRemain = 1f;
    public float Interest = 0.1f;
    public float InterestSteakWin = 0.01f;
    public int MinSteakWin = 2;
    public int MinSteakLose = 2;
    public float InterestSteakLose = 0.03f;

    public GoldAccount(float startAmount)
    {
        Amount = startAmount;
    }

    public float ApplyEndRound(int steak, float timeLeft, float remain)
    {
        float finalAmount = (1f + Interest) * Amount + BonusRound * steak + BonusTime * timeLeft + BonusRemain * remain;
        Debug.Log(string.Format("GoldAccount-EndRound {0}/ Interest {1}/ Steak Round {2}/Time {3}/ Remain {4}/Final {5}",
        Amount, Interest * Amount, BonusRound * steak, BonusTime * timeLeft, BonusRemain * remain, finalAmount));
        Amount = finalAmount;
        return Amount;
    }

    public float ApplySteackWin(int steak)
    {
        float finalAmount = (1f + InterestSteakWin * (steak > MinSteakWin ? (steak - MinSteakWin) : 0)) * Amount;
        Debug.Log(string.Format("GoldAccount-SteackWin {0}/ Interest {1}/ Steak {2}/Final {3}",
        Amount, InterestSteakWin * (steak > MinSteakWin ? (steak - MinSteakWin) : 0),  (steak > MinSteakWin ? (steak - MinSteakWin) : 0), finalAmount));
        Amount = finalAmount;
        // Amount = (1f + InterestSteakWin * (steak > MinSteakWin ? (steak - MinSteakWin) : 0)) * Amount;
        return Amount;
    }

    public float ApplySteackLose(int steak)
    {
        float finalAmount = (1f + InterestSteakLose * (steak > MinSteakLose ? (steak - MinSteakLose) : 0)) * Amount;
        Debug.Log(string.Format("GoldAccount-SteackLose {0}/ Interest {1}/ Steak {2}/Final {3}",
        Amount, InterestSteakLose * (steak > MinSteakLose ? (steak - MinSteakLose) : 0),  (steak > MinSteakLose ? (steak - MinSteakLose) : 0), finalAmount));
        Amount = finalAmount;
        // Amount = (1f + InterestSteakLose * (steak > MinSteakLose ? (steak - MinSteakLose) : 0)) * Amount;
        return Amount;
    }

    internal float ApplyDeduct(float gold)
    {
        Amount -= gold; return Amount;
    }
}
