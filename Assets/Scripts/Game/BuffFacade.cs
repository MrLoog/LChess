using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Buff;

public class BuffFacade
{
    public Buff Buff;

    private BuffFacade()
    {

    }
    public static BuffFacade CreateFromBuff(Buff b)
    {
        BuffFacade facade = new BuffFacade();
        facade.Buff = b.Clone<Buff>();
        return facade;
    }



    List<ActionUnit> _affectUnit;

    List<ActionUnit> AffectUnit
    {
        get
        {
            if (_affectUnit == null)
            {
                _affectUnit = new List<ActionUnit>();
            }
            return _affectUnit;
        }
        set
        {
            _affectUnit = value;
        }
    }
    List<ActionUnit> targetUnit;

    Dictionary<string, object> _infos;
    Dictionary<string, object> Infos
    {
        get
        {
            if (_infos == null)
            {
                _infos = new Dictionary<string, object>();
            }
            return _infos;
        }
        set
        {
            _infos = value;
        }
    }


    internal void PerformBuff(int ownerGroup = -1)
    {
        List<ActionUnit> target = null;
        if (Buff.Target == TargetType.Custom)
        {
            target = targetUnit;
        }
        else
        {
            target = Game.Instance.board.GetBattleTilesGroup(-1)
            .Where(x => x.ActionUnit != null &&
            (
                (Buff.Target == TargetType.Ally && x.ActionUnit.Group == ownerGroup)
                ||
                (Buff.Target == TargetType.Enemy && x.ActionUnit.Group != ownerGroup)
            )
            ).Select(x => x.ActionUnit).ToList();
        }
        if (target.Count > 0)
        {
            target.ForEach(x => x.AddBuff(this));
        }
    }

    public bool ApplyBuff(ActionUnit unit)
    {
        //this method usually cann by Action Unit to self buff
        // if (AffectUnit.Count > 0) return false;
        Buff.ActualStat = Buff.Stat;
        if (Buff.Stat == StatAffect.Random)
        {
            Buff.ActualStat = RandomStatAffect();
        }
        // if (Buff.Target != TargetType.Custom)
        // {
        //     if (Game.Instance.RoundManager.Round.RoundNumber < 5)
        //     {
        //         Buff.Percent = 20 - 2 * Game.Instance.RoundManager.Round.RoundNumber;
        //     }
        //     else
        //     {
        //         Buff.Percent = 2 * (Game.Instance.RoundManager.Round.RoundNumber - 5);
        //     }
        // }
        Debug.Log("Apply Buff " + unit.Group + "/" + Buff.Effect + "/" + Buff.ActualStat + "/" + Buff.Percent);
        float amount = 0;
        ActionUnitData baseStat = (ActionUnitData)unit.tileUnitData;
        switch (Buff.ActualStat)
        {
            case StatAffect.Health:
                amount = (Buff.Effect == EffectType.Increase ? 1 : -1) * baseStat.baseHealth * Buff.Percent / 100;
                unit.OriginStatus.baseHealth += amount;
                if (Buff.Effect == EffectType.Increase)
                {
                    if (unit.CurrentStatus.baseHealth < unit.OriginStatus.baseHealth)
                    {
                        unit.CurrentStatus.baseHealth =
                            ((unit.CurrentStatus.baseHealth + amount) > unit.OriginStatus.baseHealth) ?
                                unit.OriginStatus.baseHealth :
                                (unit.CurrentStatus.baseHealth + amount);
                    }
                }
                else
                {
                    if (unit.CurrentStatus.baseHealth > 0)
                    {
                        unit.CurrentStatus.baseHealth =
                            ((unit.CurrentStatus.baseHealth + amount) <= 0) ?
                                1 :
                                (unit.CurrentStatus.baseHealth + amount);
                    }
                }
                break;
            case StatAffect.Damage:
                amount = (Buff.Effect == EffectType.Increase ? 1 : -1) * baseStat.baseAttack * Buff.Percent / 100;
                unit.OriginStatus.baseAttack += amount;
                unit.CurrentStatus.baseAttack += amount;
                break;
            case StatAffect.AtkRate:
                amount = (Buff.Effect == EffectType.Increase ? -1 : 1) * baseStat.baseAttackRate * Buff.Percent / 100;
                unit.OriginStatus.baseAttackRate += amount;
                unit.CurrentStatus.baseAttackRate += amount;
                break;
            default:
                break;
        }
        AffectUnit.Add(unit);
        return true;
    }

    public bool RemoveBuff(ActionUnit unit)
    {
        if (Buff.ActualStat == null) return false;
        Debug.Log("Remove Buff " + unit.Group + "/" + Buff.Effect + "/" + Buff.ActualStat + "/" + Buff.Percent);
        float amount = 0;
        ActionUnitData baseStat = (ActionUnitData)unit.tileUnitData;
        switch (Buff.ActualStat)
        {
            case StatAffect.Health:
                amount = (Buff.Effect == EffectType.Increase ? 1 : -1) * baseStat.baseHealth * Buff.Percent / 100;
                unit.OriginStatus.baseHealth -= amount;
                if (Buff.Effect == EffectType.Increase)
                {
                    if (unit.CurrentStatus.baseHealth > 0)
                    {
                        unit.CurrentStatus.baseHealth =
                            ((unit.CurrentStatus.baseHealth - amount) <= 0) ?
                                1 :
                                (unit.CurrentStatus.baseHealth - amount);
                    }
                }
                else
                {
                    if (unit.CurrentStatus.baseHealth < unit.OriginStatus.baseHealth)
                    {
                        unit.CurrentStatus.baseHealth =
                            ((unit.CurrentStatus.baseHealth - amount) > unit.OriginStatus.baseHealth) ?
                                unit.OriginStatus.baseHealth :
                                (unit.CurrentStatus.baseHealth - amount);
                    }
                }
                break;
            case StatAffect.Damage:
                amount = (Buff.Effect == EffectType.Increase ? 1 : -1) * baseStat.baseAttack * Buff.Percent / 100;
                unit.OriginStatus.baseAttack -= amount;
                unit.CurrentStatus.baseAttack -= amount;
                break;
            case StatAffect.AtkRate:
                amount = (Buff.Effect == EffectType.Increase ? -1 : 1) * baseStat.baseAttackRate * Buff.Percent / 100;
                unit.OriginStatus.baseAttackRate -= amount;
                unit.CurrentStatus.baseAttackRate -= amount;
                break;
            default:
                break;
        }
        AffectUnit.Remove(unit);
        return true;
    }

    public bool RemoveBuffAffectUnit()
    {
        Debug.Log("Buff unit remove" + AffectUnit.Count);
        for (int i = AffectUnit.Count; i > 0; i--)
        {
            AffectUnit[i - 1].RemoveBuff(this);
        }
        return true;
    }

    internal static BuffFacade RandomPositiveBuff()
    {
        Buff b = new Buff();
        b.Target = TargetType.Ally;
        b.Effect = EffectType.Increase;
        b.Stat = RandomStatAffect();
        return CreateFromBuff(b);
    }

    public static StatAffect RandomStatAffect(List<StatAffect> limit = null, List<StatAffect> exclude = null)
    {
        Array values = Enum.GetValues(typeof(StatAffect));
        List<int> values2 = new List<int>((IEnumerable<int>)values);
        values2.Remove((int)StatAffect.Random);
        if (limit != null)
        {
            values2.RemoveAll(x => limit.Cast<int>().ToList().IndexOf(x) <= -1);
        }
        if (exclude != null)
        {
            values2.RemoveAll(x => exclude.Cast<int>().ToList().IndexOf(x) > -1);
        }
        if (values2.Count == 0) return StatAffect.Random;
        return (StatAffect)values2[(int)UnityEngine.Random.Range(0, values2.Count)];
    }
}
