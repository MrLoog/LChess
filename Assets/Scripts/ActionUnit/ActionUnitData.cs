using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionUnitData", menuName = "LChess/ActionUnitData", order = 2)]
[System.Serializable]
public class ActionUnitData : TileUnitData
{

    public float baseHealth;
    public float baseAttack;
    public float baseAttackRange;
    public float baseAttackRate;
}
