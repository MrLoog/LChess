using System;
using System.Collections;
using System.Linq;
using TMPro;
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

    public float RoundNumber;
    public float RoundLevel;
    public float RoundTime;
    public float Balance;
    public float MaxUnit;
    public float CurrentUnit;
    private const string TEMPLATE_GROUP_HEALTH = "{0}/{1}";
    private const string TEMPLATE_AVERAGE_DMG = "G{0} Dmg(/s): {1}";
    private const string TEMPLATE_REMAIN = "G{0} Remain: {1}";
    private const string TEMPLATE_ROUND = "Round: {0}";
    private const string TEMPLATE_ROUND_LEVEL = "Round Level: {0}";
    private const string TEMPLATE_ROUND_TIME = "Time Remain(s): {0}";
    private const string TEMPLATE_GOLD = "Balance: {0}";
    private const string TEMPLATE_UNIT_SLOT = "Battle Unit: {0}/{1}";

    public Slider SliderTotalHealth1;
    public Slider SliderTotalHealth2;
    public Text TextGroupHealth1;
    public Text TextGroupHealth2;
    public Text TextAvgDmg1;
    public Text TextAvgDmg2;
    public Text TextRemain1;
    public Text TextRemain2;
    public Text TextRound;
    public Text TextRoundTime;
    public Text TextGold;
    public Text OtherInfo;

    public TMP_Text UserMessage;

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
        Game.Instance.Events.RegisterListener(Game.EVENT_GAME_READY).AddListener(OnGameReady);
    }

    public void OnGameReady()
    {
        Text btnRefresh = ButtonRefresh.GetComponentInChildren<Text>();
        btnRefresh.text = string.Format(btnRefresh.text, Game.Instance.Profile.GameModeCtrl.GetShopPrice());
    }

    // Update is called once per frame
    void Update()
    {
        _process += Time.deltaTime;
        RoundTime = Game.Instance.RoundManager?.Round != null ? Game.Instance.RoundManager.Round.TimeLeft : 0;
        if (_process > _updateTime)
        {
            _process = 0;
            ScanAndShow(false);
        }
    }

    internal void ShowUserMessage(object mES_PHASE_PREPARE, float v)
    {
        throw new NotImplementedException();
    }

    public void ScanAndShow(bool isFull)
    {
        if (!Game.Instance.OnGame) return;
        ScanInfo(isFull);
        ShowInfo();
    }

    internal void ShowGameState(bool active)
    {
        if (active)
        {
            ButtonStart.gameObject.SetActive(false);
            ButtonPause.gameObject.SetActive(true);
        }
        else
        {
            ButtonPause.gameObject.SetActive(false);
            ButtonStart.gameObject.SetActive(true);
        }
        ScanAndShow(true);
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
        CurrentUnit = 0f;
        MaxUnit = Game.Instance.Profile.GameModeCtrl.GetMaxSpawn();
        // List<ActionUnit> units = ActionUnitManger.Instance.GetAll();
        ActionUnitData data = null;
        ActionUnitData curData = null;
        foreach (ActionUnit unit in ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile))
        {
            if (isFull) data = (ActionUnitData)unit.OriginStatus;
            curData = (ActionUnitData)unit.CurrentStatus;
            if (unit.Group == 0)
            {
                if (isFull) OriginTotalHealth1 += data.baseHealth;
                CurTotalHealth1 += curData.baseHealth;
                AvgDmg1 = (AvgDmg1 * Remain1 + curData.baseAttack / curData.baseAttackRate) / (Remain1 + 1);
                Remain1 += 1;
                CurrentUnit++;
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
        string textInfo = "";
        textInfo = string.Format(TEMPLATE_AVERAGE_DMG, 1, AvgDmg1)
        + "\r\n"
        + string.Format(TEMPLATE_AVERAGE_DMG, 2, AvgDmg2)
        + "\r\n"
        + string.Format(TEMPLATE_REMAIN, 1, Remain1)
        + "\r\n"
        + string.Format(TEMPLATE_REMAIN, 2, Remain2)
        + "\r\n"
        + string.Format(TEMPLATE_ROUND, RoundNumber)
        + "\r\n"
        + string.Format(TEMPLATE_ROUND_LEVEL, RoundLevel)
        + "\r\n"
        + string.Format(TEMPLATE_ROUND_TIME, (int)RoundTime)
        + "\r\n"
        + string.Format(TEMPLATE_GOLD, Balance)
        + "\r\n"
        + string.Format(TEMPLATE_UNIT_SLOT, CurrentUnit, MaxUnit);
        foreach (FormationFacade f in FormationManager.Instance.ActiveFormations)
        {
            textInfo += ""
        + "\r\n"
        + f.Formation.Name;
        }
        OtherInfo.text = textInfo;
        // TextAvgDmg1.text = string.Format(TEMPLATE_AVERAGE_DMG, 1, AvgDmg1);
        // TextAvgDmg2.text = string.Format(TEMPLATE_AVERAGE_DMG, 2, AvgDmg2);
        // TextRemain1.text = string.Format(TEMPLATE_REMAIN, 1, Remain1);
        // TextRemain2.text = string.Format(TEMPLATE_REMAIN, 2, Remain2);
        // TextRound.text = string.Format(TEMPLATE_ROUND, Round);
        // TextRoundTime.text = string.Format(TEMPLATE_ROUND_TIME, (int)RoundTime);
        // TextGold.text = string.Format(TEMPLATE_GOLD, Balance);
    }

    public Button ButtonStart;
    public Button ButtonPause;
    public Button ButtonRefresh;

    public void PressStart()
    {
        ShowGameState(true);
        if (Game.Instance.Profile.GameModeCtrl.RoundManager.Round != null
        && Game.Instance.Profile.GameModeCtrl.RoundManager.Round.CurPhase != global::Round.RoundPhase.NotStart)
        {
            Time.timeScale = 1;
        }
        else
        {
            Game.Instance.StartGame();
        }
        MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_START_GAME, 1f);
    }

    public void PressPause()
    {
        ShowGameState(false);
        Time.timeScale = 0;
        MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_PAUSE_GAME, 1f);
    }


    public void PressSpawn()
    {
        UnitShop.Instance.ToggerShop(false);
    }

    public void PressRefresh()
    {
        if (Game.Instance.Profile.GameModeCtrl.GetShopPrice() == 0 || Game.Instance.Profile.GameModeCtrl.DeductGoldForShop())
        {
            UnitShop.Instance.Refresh();
        }
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

    public void ShowUserMessage(string message, float time)
    {
        UserMessage.text = message;
        UserMessage.gameObject.SetActive(true);
        StartCoroutine(HideUserMessage(time));
    }

    private IEnumerator HideUserMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        UserMessage.gameObject.SetActive(false);
        yield return null;
    }

    IEnumerator ActiveSlider(Slider slider)
    {
        //Tells Unity to wait 1 second
        yield return new WaitForSeconds(0.1f);
        slider.gameObject.SetActive(true);
        yield return null;
    }
}
