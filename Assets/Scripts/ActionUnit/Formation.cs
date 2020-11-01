using UnityEngine;
[CreateAssetMenu(fileName = "Formation", menuName = "LChess/Formation", order = 7)]
[System.Serializable]
public class Formation : MScriptableObject
{
    public string Name;
    public enum FormationCheckType
    {
        All, Current, Next, Prev
    }
    public FormationCondition[] Conditions;
    public Buff[] Buffs;

    public Formation NextLevel;
    public Formation PrevLevel;


    
}
