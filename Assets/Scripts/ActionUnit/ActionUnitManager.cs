using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionUnitManger
{
    public static ActionUnitManger Instance { get; private set; } = new ActionUnitManger();
    private ActionUnitManger() { }

    List<ActionUnit> actionUnits = new List<ActionUnit>();
    public int Total => actionUnits.Count;

    internal int GetTotalGroup(int group)
    {
        return actionUnits.Where(a => a.Group == group).Count();
    }

    internal void Add(ActionUnit actionUnit)
    {
        actionUnits.Add(actionUnit);
    }

    internal List<ActionUnit> GetAll()
    {
        return actionUnits.Where(a => a.gameObject.activeInHierarchy).ToList();
    }

    internal void Remove(ActionUnit actionUnit)
    {
        actionUnits.Remove(actionUnit);
    }

    public ActionUnit GetUnit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == Game.TAG_MONSTER)
            {
                return FindObjScript.GetObjScriptFromCollider<ActionUnit>(hit.collider);
            }
        }
        return null;
    }
}
