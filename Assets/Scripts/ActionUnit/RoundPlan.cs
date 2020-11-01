
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RoundPlan", menuName = "LChess/RoundPlan", order = 4)]
[System.Serializable]
public class RoundPlan : MScriptableObject
{
    public enum RoundPlanType
    {
        Limit, Infinity
    }

    public RoundPlanType PlanType;
    public Round[] Rounds;



    internal Round GetNextRound(int mainGroup, int curNumberRound)
    {
        Round newRound = (Round)ScriptableObject.CreateInstance(typeof(Round));
        if (Rounds.Count() > curNumberRound)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Rounds[curNumberRound]), newRound);
            return newRound;
        }
        else
        {
            if (PlanType == RoundPlanType.Infinity)
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Rounds[Rounds.Count() - 1]), newRound);
                return newRound;
            }
            else
            {
                return null;
            }
        }
    }
}
