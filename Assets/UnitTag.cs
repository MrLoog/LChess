using UnityEngine;
using UnityEngine.UI;

public class UnitTag : MonoBehaviour
{
    public Text UnitName;

    public TileUnitData tileUnitData { get; internal set; }
    public Transform characterPrefabParent;
    private CharacterAnimationEventCalls _characterAnimationEventCalls;
    protected Animator _animator;
    private bool _performAttack = false;
    private float _delayAttack = 0f;
    private float _attackRate = 1f;

    public const string TEMPLATE_TAG_NAME = "{0}\r\n{1}g";

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
        // Debug.Log("Select " + tileUnitData.unitName);
        // Game.Instance.PickupUnit = tileUnitData;
        // UnitShop.Instance.HideShop();
        ActionUnitData data = (ActionUnitData)tileUnitData;
        if (RoundManager.Instance.GoldAccount.Amount >= data.gold)
        {
            RoundManager.Instance.GoldAccount.ApplyDeduct(data.gold);
            Game.Instance.SpawnPrepareUnit(tileUnitData);
        }
        else
        {
            MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_GOLD_INVALID, 1f);
        }
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
