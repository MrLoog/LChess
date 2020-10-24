using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Buff", menuName = "LChess/Buff", order = 5)]
[System.Serializable]
public class Buff : ScriptableObject
{
    public enum TargetType
    {
        Enemy, Ally
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
    public StatAffect ActualStat;

    public float Percent;
    public bool IsUse;

    internal void PerformBuff(int ownerGroup)
    {
        Percent = 5f;
        List<ActionUnit> target = Game.Instance.board.GetBattleTilesGroup(-1)
        .Where(x => x.ActionUnit != null &&
        (
            (Target == TargetType.Ally && x.ActionUnit.Group == ownerGroup)
            ||
            (Target == TargetType.Enemy && x.ActionUnit.Group != ownerGroup)
        )
        ).Select(x => x.ActionUnit).ToList();
        target.ForEach(x =>
        {
            ApplyBuff(x);
        });
    }

    private ActionUnit ApplyBuff(ActionUnit unit)
    {
        ActualStat = Stat;
        if (Stat == StatAffect.Random)
        {
            ActualStat = RandomStatAffect();
        }
        if (RoundManager.Instance.Round.RoundNumber < 5)
        {
            Percent = 20 - 2 * RoundManager.Instance.Round.RoundNumber;
        }
        else
        {
            Percent = 2 * RoundManager.Instance.Round.RoundNumber;
        }
        float rate = (Effect == EffectType.Increase ? (1 + Percent / 100) : (1 - Percent / 100));
        Debug.Log("Apply Buff " + unit.Group + "/" + Effect + "/" + ActualStat + "/" + rate);
        switch (ActualStat)
        {
            case StatAffect.Health:
                unit.CurrentStatus.baseHealth = unit.CurrentStatus.baseHealth * rate;
                unit.OriginStatus.baseHealth = unit.OriginStatus.baseHealth * rate;
                break;
            case StatAffect.Damage:
                unit.CurrentStatus.baseAttack = unit.CurrentStatus.baseAttack * rate;
                unit.OriginStatus.baseAttack = unit.OriginStatus.baseAttack * rate;
                break;
            case StatAffect.AtkRate:
                rate = (Effect == EffectType.Decrease ? (1 + Percent / 100) : (1 - Percent / 100));
                unit.CurrentStatus.baseAttackRate = unit.CurrentStatus.baseAttackRate * rate;
                unit.OriginStatus.baseAttackRate = unit.OriginStatus.baseAttackRate * rate;
                break;
            default:
                break;
        }
        return unit;
    }

    internal static Buff RandomPositiveBuff()
    {
        Buff b = new Buff();
        b.Target = TargetType.Ally;
        b.Effect = EffectType.Increase;
        b.Stat = RandomStatAffect();
        return b;
    }

    private static StatAffect RandomStatAffect()
    {
        Array values = Enum.GetValues(typeof(StatAffect));
        List<int> values2 = new List<int>((IEnumerable<int>)values);
        values2.Remove((int)StatAffect.Random);
        return (StatAffect)values2[(int)UnityEngine.Random.Range(0, values2.Count)];
    }
}
