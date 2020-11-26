using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationConditionFacade
{
    public FormationCondition Condition;

    private FormationConditionFacade()
    {

    }
    public static FormationConditionFacade CreateFromFormation(FormationCondition f)
    {
        FormationConditionFacade facade = new FormationConditionFacade();
        facade.Condition = f.Clone<FormationCondition>();
        return facade;
    }


    public List<ActionUnit> ValidUnits;

    public List<ActionUnit> FindValidUnit(int group)
    {
        return Game.Instance.board.GetBattleTilesGroup(-1)
        .Where(x => x.ActionUnit != null
        && x.ActionUnit.Group == group
        && x.ActionUnit.Alive
        && x.gameObject.activeInHierarchy
        && x.ActionUnit.CurrentStatus.IsValidType(Condition.Type)
        ).Select(x => x.ActionUnit).ToList();
    }
    public bool ValidCondition(int group)
    {
        ValidUnits = FindValidUnit(group);
        if (ValidUnits.Count >= Condition.MinNumberUnit) return true;

        return false;
    }

    public void ApplyBuff(BuffFacade b, int group)
    {
        ValidUnits = FindValidUnit(group);
        foreach (ActionUnit u in ValidUnits)
        {
            u.AddBuff(b);
        }
    }
}
