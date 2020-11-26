using UnityEngine;
[CreateAssetMenu(fileName = "Buff", menuName = "LChess/Buff", order = 5)]
[System.Serializable]
public class Buff : MScriptableObject
{

    
    public enum TargetType
    {
        Enemy, Ally, Custom
    }

    public enum EffectType
    {
        Increase, Decrease
    }

    public enum StatAffect
    {
        Random, Health, Damage, AtkRate
    }

    public TargetType Target;
    public EffectType Effect;
    public StatAffect Stat;
    public StatAffect? ActualStat;

    public float Percent;
}
