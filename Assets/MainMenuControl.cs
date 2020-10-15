using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{

    public static MainMenuControl Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    private float OriginTotalHealth1;
    private float OriginTotalHealth2;
    private float CurTotalHealth1;
    private float CurTotalHealth2;
    private float AvgDmg1;
    private float AvgDmg2;
    private float Remain1;
    private float Remain2;
    private const string TEMPLATE_GROUP_HEALTH = "{0}/{1}";
    private const string TEMPLATE_AVERAGE_DMG = "G{0} Dmg(/s): {1}";
    private const string TEMPLATE_REMAIN = "G{0} Remain: {1}";

    public Slider SliderTotalHealth1;
    public Slider SliderTotalHealth2;
    public Text TextGroupHealth1;
    public Text TextGroupHealth2;
    public Text TextAvgDmg1;
    public Text TextAvgDmg2;
    public Text TextRemain1;
    public Text TextRemain2;

    private float _process = 0;
    private float _updateTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // OriginTotalHealth1 = 100f;
        // OriginTotalHealth2 = 100f;
        // CurTotalHealth1 = 50f;
        // CurTotalHealth2 = 50f;
        // AvgDmg1 = 100f;
        // AvgDmg2 = 200f;
        // Remain1 = 1f;
        // Remain2 = 2f;
        ScanAndShow(true);
    }

    // Update is called once per frame
    void Update()
    {
        _process += Time.deltaTime;
        if (_process > _updateTime)
        {
            _process = 0;
            ScanAndShow(false);
        }
    }

    public void ScanAndShow(bool isFull)
    {
        ScanInfo(isFull);
        ShowInfo();
    }

    private void ScanInfo(bool isFull)
    {
        if (isFull)
        {
            OriginTotalHealth1 = 0f;
            OriginTotalHealth2 = 0f;
        }
        CurTotalHealth1 = 0f;
        CurTotalHealth2 = 0f;
        AvgDmg1 = 0f;
        AvgDmg2 = 0f;
        Remain1 = 0f;
        Remain2 = 0f;
        // List<ActionUnit> units = ActionUnitManger.Instance.GetAll();
        ActionUnitData data = null;
        ActionUnitData curData = null;
        foreach (ActionUnit unit in ActionUnitManger.Instance.GetAll())
        {
            if (isFull) data = (ActionUnitData)unit.tileUnitData;
            curData = (ActionUnitData)unit.CurrentStatus;
            if (unit.Group == 0)
            {
                if (isFull) OriginTotalHealth1 += data.baseHealth;
                CurTotalHealth1 += curData.baseHealth;
                AvgDmg1 = (AvgDmg1 * Remain1 + curData.baseAttack / curData.baseAttackRate) / (Remain1 + 1);
                Remain1 += 1;
            }
            else
            {
                if (isFull) OriginTotalHealth2 += data.baseHealth;
                CurTotalHealth2 += curData.baseHealth;
                AvgDmg2 = (AvgDmg2 * Remain2 + curData.baseAttack / curData.baseAttackRate) / (Remain2 + 1);
                Remain2 += 1;
            }
        }
    }


    private void ShowInfo()
    {
        SliderTotalHealth1.value = SliderTotalHealth1.maxValue * CurTotalHealth1 / OriginTotalHealth1;
        SliderTotalHealth2.value = SliderTotalHealth2.maxValue * CurTotalHealth2 / OriginTotalHealth2;
        TextGroupHealth1.text = string.Format(TEMPLATE_GROUP_HEALTH, CurTotalHealth1, OriginTotalHealth1);
        TextGroupHealth2.text = string.Format(TEMPLATE_GROUP_HEALTH, CurTotalHealth2, OriginTotalHealth2);
        TextAvgDmg1.text = string.Format(TEMPLATE_AVERAGE_DMG, 1, AvgDmg1);
        TextAvgDmg2.text = string.Format(TEMPLATE_AVERAGE_DMG, 2, AvgDmg2);
        TextRemain1.text = string.Format(TEMPLATE_REMAIN, 1, Remain1);
        TextRemain2.text = string.Format(TEMPLATE_REMAIN, 2, Remain2);
    }

    public Button ButtonStart;
    public Button ButtonPause;

    public void PressStart()
    {
        ButtonStart.gameObject.SetActive(false);
        ButtonPause.gameObject.SetActive(true);
    }

    public void PressPause()
    {
        ButtonPause.gameObject.SetActive(false);
        ButtonStart.gameObject.SetActive(true);
    }

    public void PressRandom()
    {
        Game.Instance.RandomSpawn2GroupEqual();
    }

    public void PressSpawn()
    {
    }

    public void PressBattle()
    {
        Game.Instance.AllBattleMode();
    }

    public void PressPeace()
    {
        Game.Instance.AllBattleMode(false);
    }

    public void PressClear()
    {
        Game.Instance.ClearBoard();
    }
}
