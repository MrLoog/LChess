using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuControl : MonoBehaviour
{
    public static InGameMenuControl Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);


        if (OnSelectedTarget == null) OnSelectedTarget = new ActionUnitEvent();
        OnSelectedTarget.AddListener(SelectTargetForMonster);
    }

    private const string TEMPLATE_DISPLAY_DATA = "<b>{0}</b> : <color=lime>{1}</color>";
    public GameObject CanvasMenu;
    [SerializeField]
    private ActionUnit _focusUnit;
    public ActionUnit FocusUnit
    {
        get
        {
            return _focusUnit;
        }
        set
        {
            CanvasMenu.SetActive(value == null ? false : true);
            _focusUnit = value;
            if (value == null)
            {

            }
            else
            {
                ShowUnitInfo();
            }
        }
    }

    public Text UnitName;
    public Text Health;
    public Text Damage;
    public Text AttackRange;
    public Text AttackRate;

    public Texture2D TextureSword;

    private bool _cursorSwordEnable;
    private ActionUnitEvent _backupEvent;
    private ActionUnitEvent OnSelectedTarget;
    private bool CursorSwordEnable
    {
        get
        {
            return _cursorSwordEnable;
        }
        set
        {
            if (value)
            {
                Cursor.SetCursor(TextureSword, Vector2.zero, CursorMode.Auto);
                _backupEvent = Game.Instance.OnUnitSelected;
                Game.Instance.OnUnitSelected = OnSelectedTarget;
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Game.Instance.OnUnitSelected = _backupEvent;
            }
            _cursorSwordEnable = value;
        }
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (CanControlUnit)
        {
            ShowUnitInfo();
        }
        else if (FocusUnit != null && !FocusUnit.Alive)
        {
            FocusUnit = null;
        }
    }

    private bool CanControlUnit => !(FocusUnit == null || !FocusUnit.Alive);
    public void ShowUnitInfo()
    {
        if (CanControlUnit)
        {
            ActionUnitData originData = (ActionUnitData)FocusUnit.tileUnitData;
            ActionUnitData curData = (ActionUnitData)FocusUnit.CurrentStatus;
            UnitName.text = string.Format(TEMPLATE_DISPLAY_DATA, "Name",
                originData.unitName);
            Health.text = string.Format(TEMPLATE_DISPLAY_DATA, "Health",
                string.Format("{0}/{1}", curData.baseHealth, originData.baseHealth));
            Damage.text = string.Format(TEMPLATE_DISPLAY_DATA, "Damage",
                string.Format("{0}/{1}", curData.baseAttack, originData.baseAttack));
            AttackRange.text = string.Format(TEMPLATE_DISPLAY_DATA, "Range",
                string.Format("{0}/{1}", curData.baseAttackRange, originData.baseAttackRange));
            AttackRate.text = string.Format(TEMPLATE_DISPLAY_DATA, "Rate",
                string.Format("{0}/{1}", curData.baseAttackRate, originData.baseAttackRate));
        }
    }



    public void PressBattle()
    {
        if (CanControlUnit)
        {
            FocusUnit.BattleMode = true;
        }
    }

    public void PressIdle()
    {
        if (CanControlUnit)
        {
            FocusUnit.BattleMode = false;
        }
    }

    public void PressDeath()
    {
        if (CanControlUnit)
        {
            Game.Instance.DestroyUnit(FocusUnit);
            FocusUnit = null;
        }
    }

    public void PressTarget()
    {
        CursorSwordEnable = !CursorSwordEnable;
    }


    private void SelectTargetForMonster(ActionUnit target)
    {
        if (CanControlUnit)
        {
            if (FocusUnit.ForceSelectTarget(target))
            {
                CursorSwordEnable = !CursorSwordEnable;
            }
            else
            {
                MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_INVALID_TARGET, 3f);
            }
        }
    }
}
