using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitLevelManager
{
    public static UnitLevelManager Instance { get; private set; } = new UnitLevelManager();
    private UnitLevelManager()
    {

    }

    public bool ValidLevelUpUnit(ActionUnit unit, bool levelUp = true)
    {
        Debug.Log("UnitLevelManager Valid");
        if (unit.CurrentStatus.NextLevel == null)
        {
            return false;
        }
        List<ActionUnit> materials = Game.Instance.board.GetBattleTilesGroup(-1).Where(x => x.ActionUnit != null
        && x.ActionUnit.Group == unit.Group
        && x.ActionUnit.UnitID != unit.UnitID
        && ((ActionUnitData)(x.ActionUnit.CurrentStatus)).unitName == ((ActionUnitData)(unit.CurrentStatus)).unitName
        && ((ActionUnitData)(x.ActionUnit.CurrentStatus)).Level == ((ActionUnitData)(unit.CurrentStatus)).Level)
        .Select(x => x.ActionUnit).ToList();
        Debug.Log("UnitLevelManager " + materials.Count);
        if (materials.Count == 2)
        {
            if (levelUp)
            {
                return PerformLevelUpUnit(unit, materials);
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    private bool PerformLevelUpUnit(ActionUnit unit, List<ActionUnit> materials)
    {
        Debug.Log("UnitLevelManager level up");
        ActionUnitData newStatus = (ActionUnitData)ScriptableObject.CreateInstance(typeof(ActionUnitData));
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(unit.CurrentStatus.NextLevel), newStatus);
        unit.CurrentStatus = newStatus;
        ActionUnitData newData = new ActionUnitData();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(unit.CurrentStatus), newData);
        unit.tileUnitData = newData;
        materials.ForEach(x => Game.Instance.DestroyUnit(x));
        ValidLevelUpUnit(unit, true);
        return true;
    }

}
