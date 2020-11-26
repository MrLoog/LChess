
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

    public enum RoundType
    {
        Limit, Infinity
    }

    public RoundPlanType PlanType;
    public Round[] Rounds;



    internal Round GetNextRound(int mainGroup, int prevLevel)
    {
        Round newRound = (Round)ScriptableObject.CreateInstance(typeof(Round));
        if (Rounds.Count() > prevLevel)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Rounds[prevLevel]), newRound);
        }
        else
        {
            if (PlanType == RoundPlanType.Infinity)
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(Rounds[Rounds.Count() - 1]), newRound);
            }
            else
            {
                return null;
            }
        }
        newRound.Level = prevLevel + 1;
        return newRound;
    }
}
