using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionUnitManger
{
    private EventDict _events;
    public EventDict Events
    {
        get
        {
            if (_events == null) _events = new EventDict();
            return _events;
        }
    }

    public const string EVENT_UNIT_ADD = "UNIT_ADD";
    public const string EVENT_UNIT_SPAWN = "UNIT_SPAWN";
    public const string EVENT_UNIT_REMOVE = "UNIT_REMOVE";
    public const string EVENT_UNIT_DEATH = "UNIT_DEATH";

    public static ActionUnitManger Instance { get; private set; } = new ActionUnitManger();
    public List<int> Groups = new List<int>();
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
        actionUnit.Events.RegisterListener(ActionUnit.EVENT_UNIT_LIVE).AddListener(OnUnitSpawnBoard);
        actionUnit.Events.RegisterListener(ActionUnit.EVENT_UNIT_DEATH).AddListener(OnUnitDeath);
        if (Groups.IndexOf(actionUnit.Group) < 0)
        {
            Groups.Add(actionUnit.Group);
        }
        Events.InvokeOnAction(EVENT_UNIT_ADD);
    }

    private void OnUnitSpawnBoard()
    {
        Events.InvokeOnAction(EVENT_UNIT_SPAWN);
    }

    private void OnUnitDeath()
    {
        Events.InvokeOnAction(EVENT_UNIT_DEATH);
    }

    internal List<ActionUnit> GetAll()
    {
        return actionUnits.Where(a => a.gameObject.activeInHierarchy).ToList();
    }

    internal void Remove(ActionUnit actionUnit)
    {
        actionUnits.Remove(actionUnit);
        actionUnit.Events.RegisterListener(ActionUnit.EVENT_UNIT_LIVE).RemoveListener(OnUnitSpawnBoard);
        actionUnit.Events.RegisterListener(ActionUnit.EVENT_UNIT_DEATH).RemoveListener(OnUnitDeath);
        if (actionUnits.Where(x => x.Group == actionUnit.Group).Count() == 0)
        {
            Groups.Remove(actionUnit.Group);
        }
        Events.InvokeOnAction(EVENT_UNIT_REMOVE);
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
