using UnityEngine;

[CreateAssetMenu(fileName = "TileUnitData", menuName = "LChess/TileUnitData", order = 1)]
[System.Serializable]
public class TileUnitData : MScriptableObject
{
    public string unitName;
    public GameObject characterPrefab;

    
}
