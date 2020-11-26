using UnityEngine;
using UnityEngine.UI;

public class UnitTag : MonoBehaviour
{
    public Text UnitName;
    public GameObject UnitModel;


    public TileUnitData tileUnitData { get; internal set; }
    public Transform characterPrefabParent;
    private CharacterAnimationEventCalls _characterAnimationEventCalls;
    protected Animator _animator;
    private bool _performAttack = false;
    private float _delayAttack = 0f;
    private float _attackRate = 1f;


    public bool OnceMode = true;
    public bool Sold = false;

    public const string TEMPLATE_TAG_NAME = "{0}\r\n{1}g";
    public const string TEMPLATE_SOLD = "Sold out";

    public void SpawnCharacter()
    {
        if (tileUnitData && characterPrefabParent)
        {
            ActionUnitData data = (ActionUnitData)tileUnitData;
            _attackRate = data.baseAttackRate;
            GameObject spawned = Instantiate(tileUnitData.characterPrefab, characterPrefabParent, false);
            // characterPrefabParent.transform.ChangeLayersRecursively(gameObject.layer);
            spawned.transform.localScale = new Vector3(40f, 40f, 40f);
            spawned.transform.localRotation = Quaternion.Euler(0f, spawned.transform.localRotation.eulerAngles.y + 180f, 0f);
            // Text name = gameObject.transform.("Text").GetComponent<Text>();
            // name.text = data.unitName;
            UnitName.text = string.Format(TEMPLATE_TAG_NAME, data.unitName, data.gold);

            _animator = GetComponentInChildren<Animator>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_performAttack)
        {
            _delayAttack += Time.deltaTime;
            if (_delayAttack > _attackRate)
            {
                _delayAttack = 0f;
                AnimateAttack();
            }
        }
    }

    void AnimateIdle()
    {
        if (_animator)
            _animator.Play("Idle", -1);
    }

    void AnimateAttack()
    {
        if (_animator)
            _animator.Play("Attack", -1);
    }


    public void SelectTag()
    {
        if (OnceMode && Sold) return;
        // Debug.Log("Select " + tileUnitData.unitName);
        // Game.Instance.PickupUnit = tileUnitData;
        // UnitShop.Instance.HideShop();
        ActionUnitData data = (ActionUnitData)tileUnitData;
        if (!IsSlotRemain())
        {
            MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_SLOT_FULL, 1f);
            return;
        }
        if (Game.Instance.RoundManager.GoldAccount.Amount >= data.gold)
        {
            Game.Instance.RoundManager.GoldAccount.ApplyDeduct(data.gold);
            Game.Instance.SpawnPrepareUnit(tileUnitData);
            if (OnceMode)
            {
                Sold = true;
                UnitModel.SetActive(false);
                UnitName.text = TEMPLATE_SOLD;
            }

        }
        else
        {
            MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_GOLD_INVALID, 1f);
        }
    }

    private bool IsSlotRemain()
    {
        int totalUnit = ActionUnitManger.Instance.Total;
        Debug.Log("Slot " + (totalUnit + 1) + "|" + Game.Instance.ActiveGameModeCtrl.GetMaxSpawn() + "|" + Game.Instance.board.GetPrepareTiles().Count);
        if (
            Game.Instance.board.GetPrepareTiles(true).Count == 0
            || ((totalUnit + 1) > (Game.Instance.ActiveGameModeCtrl.GetMaxSpawn() + Game.Instance.board.GetPrepareTiles().Count))
        )
        {
            return false;
        }
        return true;
    }

    public void PointerEnter()
    {
        // Debug.Log("PointerEnter " + tileUnitData.unitName);
        _performAttack = true;
    }

    public void PointerExit()
    {
        // Debug.Log("PointerExit " + tileUnitData.unitName);
        _performAttack = false;
        _delayAttack = 0f;
        AnimateIdle();
    }
}
