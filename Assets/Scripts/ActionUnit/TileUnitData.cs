using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileUnitData", menuName = "LChess/TileUnitData", order = 1)]
[System.Serializable]
public class TileUnitData : ScriptableObject
{
    public string unitName;
    public GameObject characterPrefab;
}
