using System.Collections.Generic;
using UnityEngine;
using static Formation;

public class FormationFacade
{
    public Formation Formation;

    private FormationFacade()
    {

    }
    public static FormationFacade CreateFromFormation(Formation f)
    {
        if (f == null) return null;
        FormationFacade facade = new FormationFacade();
        facade.Formation = f.Clone<Formation>();
        facade.Conditions = new FormationConditionFacade[facade.Formation.Conditions.Length];
        for (int i = 0; i < facade.Conditions.Length; i++)
        {
            facade.Conditions[i] = FormationConditionFacade.CreateFromFormation(facade.Formation.Conditions[i]);
        }
        facade.NextLevel = FormationFacade.CreateFromFormation(facade.Formation.NextLevel);
        if (facade.NextLevel != null)
        {
            facade.NextLevel.PrevLevel = facade;
        }
        // facade.PrevLevel = FormationFacade.CreateFromFormation(facade.Formation.PrevLevel);
        return facade;
    }

    List<BuffFacade> _activeBuffs;
    List<BuffFacade> ActiveBuffs
    {
        get
        {
            if (_activeBuffs == null)
            {
                _activeBuffs = new List<BuffFacade>();
            }
            return _activeBuffs;
        }
        set
        {
            _activeBuffs = value;
        }
    }


    public FormationFacade NextLevel;
    public FormationFacade PrevLevel;
    public FormationConditionFacade[] Conditions;

    public FormationFacade Check(int group, FormationCheckType checkType = FormationCheckType.All)
    {
        foreach (FormationConditionFacade c in Conditions)
        {
            if (!c.ValidCondition(group))
            {
                if ((checkType == FormationCheckType.All || checkType == FormationCheckType.Prev) && PrevLevel != null)
                {
                    FormationFacade result = PrevLevel.Check(group, FormationCheckType.Prev);
                    if (result != null) return result;
                }
                return null;
            }
        }
        if ((checkType == FormationCheckType.All || checkType == FormationCheckType.Next) && NextLevel != null)
        {
            FormationFacade result = NextLevel.Check(group, FormationCheckType.Next);
            if (result != null) return result;
        }

        return this;
    }

    public bool ApplyBuff(int group)
    {
        if (Formation.Buffs == null || Formation.Buffs.Length <= 0) return false;
        foreach (Buff b in Formation.Buffs)
        {
            BuffFacade actualBuff = BuffFacade.CreateFromBuff(b);
            ActiveBuffs.Add(actualBuff);
            foreach (FormationConditionFacade c in Conditions)
            {
                c.ApplyBuff(actualBuff, group);
            }
        }
        return true;
    }

    internal void RemoveBuff()
    {
        Debug.Log("Buff unit Formation.RemoveBuff " + ActiveBuffs.Count);
        foreach (BuffFacade b in ActiveBuffs)
        {
            b.RemoveBuffAffectUnit();
        }
    }
}
