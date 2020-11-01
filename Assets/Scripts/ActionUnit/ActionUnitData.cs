using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionUnitData", menuName = "LChess/ActionUnitData", order = 2)]
[System.Serializable]
public class ActionUnitData : TileUnitData
{

    public enum UnitType
    {
        Beast, Human, Fly, Mythical, Special, Melee, Range, Speed, Heavy, Machine
    }

    public UnitType[] Types;
    public float baseHealth;
    public float baseAttack;
    public float baseAttackRange;
    public float baseAttackRate;
    public float gold;

    public string description;

    public Vector3 position { get; set; }

    public int Level;
    public TileUnitData NextLevel;
    public TileUnitData PrevLevel;

    public bool IsValidType(UnitType typeCheck)
    {
        if ((new List<UnitType>(Types)).Contains(typeCheck))
        {
            return true;
        }
        return false;
    }

    internal ActionUnitData GetActualStatus()
    {
        return this;
    }
}
