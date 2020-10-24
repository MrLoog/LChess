using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitShop : MonoBehaviour
{
    public GameObject UnitTagTemplate;
    public GameObject CanvasShop;

    public bool IsOpen { get; set; } = false;
    public int NumberUnit = 5;

    List<GameObject> AllUnitTags = new List<GameObject>();
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);


        HideShop();
    }
    public static UnitShop Instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        // foreach (ActionUnitData data in TileUnitDatas)
        // {
        //     GameObject tag = Instantiate(UnitTagTemplate, gameObject.transform, false);
        //     UnitTag ScriptTag = tag.transform.GetComponent<UnitTag>();
        //     ScriptTag.tileUnitData = data;
        //     ScriptTag.SpawnCharacter();
        // }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowRandomUnit()
    {
        ClearUnitTag();
        for (int i = 0; i < NumberUnit; i++)
        {
            ShowUnitTag((ActionUnitData)(Game.Instance.TileUnitDatas.PickRandom()));
        }
    }

    private void ShowUnitTag(ActionUnitData data)
    {
        GameObject tag = Instantiate(UnitTagTemplate, gameObject.transform, false);
        AllUnitTags.Add(tag);
        UnitTag ScriptTag = tag.transform.GetComponent<UnitTag>();
        ScriptTag.tileUnitData = data;
        ScriptTag.SpawnCharacter();
    }

    private void ClearUnitTag()
    {
        foreach (GameObject tag in AllUnitTags)
        {
            Destroy(tag);
        }
    }

    public void ShowShop()
    {
        ShowRandomUnit();
        CanvasShop.SetActive(true);
        IsOpen = true;
        InputUtils.GameRayCastEnable = false;
    }

    public void HideShop()
    {
        CanvasShop.SetActive(false);
        IsOpen = false;
        InputUtils.GameRayCastEnable = true;
    }

    public void ToggerShop()
    {
        if (IsOpen) HideShop();
        else ShowShop();
    }
}
