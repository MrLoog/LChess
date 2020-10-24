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
    public float gold;

    public string description;

    public Vector3 position { get; set; }

    public int Level;
    public TileUnitData NextLevel;
    public TileUnitData PrevLevel;

    public List<Buff> buffs;
    public List<bool> buffed;

    public bool AddBuff(Buff b)
    {
        if (buffs == null)
        {
            buffs = new List<Buff>();
            buffed = new List<bool>();
        }
        buffs.Add(b);
        buffed.Add(false);
        return true;
    }
}
