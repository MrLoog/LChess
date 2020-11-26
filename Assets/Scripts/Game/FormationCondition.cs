using UnityEngine;

[CreateAssetMenu(fileName = "FormationCondition", menuName = "LChess/FormationCondition", order = 6)]
[System.Serializable]
public class FormationCondition : MScriptableObject
{
    public int MinNumberUnit;

    public ActionUnitData.UnitType Type;

    
}

