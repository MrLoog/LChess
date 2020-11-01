using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ActionUnit : MonoBehaviour
{
    public enum UnitState
    {
        Idle, Move, Attack
    }

    public UnitState State = UnitState.Idle;

    private IActionUnitState _currentState;
    private IActionUnitState _idleState;
    private IActionUnitState _moveState;
    private IActionUnitState _attackState;
    public static int TotalUnit { get; internal set; } = 0;
    public ActionUnitFactory OriginFactory { get; internal set; }
    public int Group = 0;
    public int UnitID;
    public TileUnitData tileUnitData { get; internal set; }

    public ActionUnitData OriginStatus = default;

    [SerializeField]
    private ActionUnitData _currentStatus;
    public ActionUnitData CurrentStatus
    {
        get
        {
            return _currentStatus;
        }
        set
        {
            _currentStatus = value;
        }
    }
    public ActionUnitData SavedStatus;
    public Transform characterPrefabParent;
    protected Animator _animator;
    private CharacterAnimationEventCalls _characterAnimationEventCalls;
    public ActionUnit TargetAttack;

    private bool _battleMode = false;
    public bool BattleMode
    {
        get
        {
            return _battleMode;
        }
        set
        {
            _battleMode = value;
            if (!value) ChangeState(UnitState.Idle);
        }
    }

    public int MonstersLayerMask { get; private set; }
    public bool Alive => CurrentStatus.baseHealth > 0;

    public bool IsRealDestroy { get; set; } = true;

    public GameTile TilePos;

    public ActionUnit MirrorEnemy;

    private float TimeOfLastAttack;

    public List<BuffFacade> _buffs;
    public List<BuffFacade> Buffs
    {
        get
        {
            if (_buffs == null) _buffs = new List<BuffFacade>();
            return _buffs;
        }
        private set
        {
            _buffs = value;
        }
    }
    public List<bool> _buffed;
    public List<bool> Buffed
    {
        get
        {
            if (_buffed == null) _buffed = new List<bool>();
            return _buffed;
        }
        private set
        {
            _buffed = value;
        }
    }

    public bool AddBuff(BuffFacade b, bool IsApply = true)
    {
        Buffs.Add(b);
        Buffed.Add(false);
        if (IsApply)
        {
            return CalculateBuff(Buffed.Count - 1);
        }
        return true;
    }

    public bool RemoveBuff(BuffFacade b, bool IsApply = true)
    {
        int index = Buffs.IndexOf(b);
        Debug.Log("Buff unit remove");
        if (index == -1) return false;
        else
        {
            if (IsApply)
            {
                Debuff(index);
            }
            Buffs.RemoveAt(index);
            Buffed.RemoveAt(index);
        }
        Debug.Log("Buff unit remove " + index);
        return true;
    }

    public bool Debuff(int index = -1)
    {
        if (index == -1)
        {
            for (int i = 0; i < Buffed.Count; i++)
            {
                Debuff(i);
            }
        }
        else
        {
            if (Buffed.Count < (index + 1))
            {
                return false;
            }
            else
            {
                if (Buffed[index])
                {
                    if (Buffs[index].RemoveBuff(this))
                    {
                        Buffed[index] = false;
                    }
                }
            }
        }
        return true;
    }

    public bool CalculateBuff(int index = -1)
    {
        if (index == -1)
        {
            for (int i = 0; i < Buffed.Count; i++)
            {
                CalculateBuff(i);
            }
        }
        else
        {
            if (Buffed.Count < (index + 1))
            {
                return false;
            }
            else
            {
                if (!Buffed[index])
                {
                    if (Buffs[index].ApplyBuff(this))
                    {
                        Buffed[index] = true;
                    }
                }
            }
        }
        return true;
    }

    private void Awake()
    {
        MonstersLayerMask = 1 << LayerMask.NameToLayer("Monster");
        _idleState = new IdleState(this);
        _attackState = new AttackState(this);
        _moveState = new MoveState(this);
        _currentState = _idleState;
    }

    public void SpawnOn(GameTile tile)
    {
        tile.ActionUnit = this;
        this.TilePos = tile;
        transform.localPosition = tile.transform.localPosition;
    }


    // Start is called before the first frame update
    void Start()
    {
        AnimateIdle();
    }


    // Update is called once per frame
    void Update()
    {
        Debug.Assert(_currentState != null, "State null");
        _currentState.Update();
    }

    private void DrawCircle(Vector3 position, float radius)
    {
        var increment = 10;
        for (int angle = 0; angle < 360; angle = angle + increment)
        {
            var heading = Vector3.forward - position;
            var direction = heading / heading.magnitude;
            var point = position + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
            var point2 = position + Quaternion.Euler(0, angle + increment, 0) * Vector3.forward * radius;
            Debug.DrawLine(point, point2, Color.red);
        }
    }

    public void SpawnCharacter()
    {
        if (tileUnitData && characterPrefabParent)
        {
            GameObject spawned = Instantiate(tileUnitData.characterPrefab, characterPrefabParent, false);
            characterPrefabParent.transform.ChangeLayersRecursively(gameObject.layer);

            _animator = GetComponentInChildren<Animator>();
            CalculateStatusStat();
            _characterAnimationEventCalls = GetComponentInChildren<CharacterAnimationEventCalls>();
            _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_ATTACK).AddListener(AttackApplyDamage);
        }
    }

    public void CalculateStatusStat()
    {
        ActionUnitData newData = (ActionUnitData)ScriptableObject.CreateInstance(typeof(ActionUnitData));
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(((ActionUnitData)tileUnitData).GetActualStatus()), newData);
        OriginStatus = newData;
        newData = (ActionUnitData)ScriptableObject.CreateInstance(typeof(ActionUnitData));
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(((ActionUnitData)tileUnitData).GetActualStatus()), newData);
        CurrentStatus = newData;
    }

    private void AttackApplyDamage()
    {
        // if (CurrentStatus.baseHealth > 0 && TargetAttack != null)
        if (TargetAttack != null)
        {
            TargetAttack.ApplyDamage(CurrentStatus.baseAttack);
        }
    }

    private void ApplyDamage(float receivedDamage)
    {
        if (CurrentStatus.baseHealth <= 0) return;
        CurrentStatus.baseHealth -= receivedDamage;
        if (CurrentStatus.baseHealth <= 0)
        {
            _currentState.Disable();
            Die();
        }
    }

    public void Die()
    {
        _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_DIE).AddListener(Destroy);
        Debug.Log(UnitID + " Die ");
        GetComponent<ActionUnitFindTarget>().enabled = false;
        GetComponent<ActionUnitMove>().enabled = false;
        GetComponent<ActionUnitAttack>().enabled = false;

        AnimateDeath();
        // StartCoroutine(Destroy());
    }

    public void Revive()
    {
        Debug.Log(UnitID + " Revive ");
        GetComponent<ActionUnitFindTarget>().enabled = true;
        GetComponent<ActionUnitMove>().enabled = true;
        GetComponent<ActionUnitAttack>().enabled = true;

        // StartCoroutine(Destroy());
    }

    public void Destroy()
    {
        // yield return new WaitForSeconds(0.5f);
        // yield return new WaitForSeconds(0.5f);
        // _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_ATTACK).RemoveListener(AttackApplyDamage);
        // _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_DIE).RemoveListener(Destroy);
        Debug.Log("Die and Destroy");
        if (IsRealDestroy)
            Game.Instance.DestroyUnit(this);
        else
            gameObject.SetActive(false);
    }


    public bool IsEnemyInRangeAttack()
    {
        if (TargetAttack == null) return false;
        return Vector3.Distance(transform.position, TargetAttack.transform.position) <= ((ActionUnitData)CurrentStatus).baseAttackRange;
    }

    internal void EnterGrabMode(bool v)
    {
        GetComponent<NavMeshObstacle>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = !v;
    }

    public bool ChangeState(UnitState newState)
    {
        if (newState.Equals(State)) return true;

        UnitState oldState = State;
        if (oldState.Equals(UnitState.Attack) || oldState.Equals(UnitState.Idle))
        {
            GetComponent<NavMeshObstacle>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = true;
        }
        State = newState;
        Debug.Log(UnitID.ToString() + " Change State " + oldState.ToString() + " - " + State.ToString());
        _currentState.Disable();
        switch (State)
        {
            case UnitState.Idle:
                _currentState = _idleState;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshObstacle>().enabled = true;
                AnimateIdle();
                break;
            case UnitState.Attack:
                _currentState = _attackState;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshObstacle>().enabled = true;
                _characterAnimationEventCalls.InvokeOnAction(CharacterAnimationEventCalls.K_STATE_ATTACK_IN);
                AnimateAttack();
                break;
            case UnitState.Move:
                _currentState = _moveState;
                AnimateMove();
                break;
            default:
                _currentState = _moveState;
                AnimateIdle();
                break;
        }
        _currentState.Enable();
        return true;
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

    void AnimateMove()
    {
        if (_animator)
            _animator.Play("Walk", -1);
    }
    void AnimateDeath()
    {
        if (_animator)
            _animator.Play("Death", -1);
    }

    public void FaceTarget()
    {
        if (TargetAttack == null) return;
        FaceTarget(TargetAttack.transform.position);
    }


    public void FaceTarget(Vector3 destination)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 1f);
    }

    public bool ForceSelectTarget(ActionUnit target)
    {
        if (target == null
        || !target.Alive
        || target.Group == Group) return false;
        gameObject.GetComponent<ActionUnitFindTarget>().LockToDeath = true;
        gameObject.GetComponent<ActionUnitFindTarget>().MarkTargetAttack(target);
        return true;
    }

    interface IActionUnitState
    {
        void Update();
        void Enable();
        void Disable();
    }
    class IdleState : IActionUnitState
    {
        ActionUnit _host;

        public IdleState(ActionUnit host)
        {
            _host = host;
        }
        public void Update()
        {
        }
        public void Enable()
        {
        }

        public void Disable()
        {

        }
    }

    class AttackState : IActionUnitState
    {
        ActionUnit _host;

        public AttackState(ActionUnit host)
        {
            _host = host;
        }
        public void Update()
        {
            ActionUnit target = _host.TargetAttack;
            if (!target.Alive || !target || !_host.IsEnemyInRangeAttack())
            {
                // _host._navMeshAgent.enabled = false;
                _host.TargetAttack = null;
                _host.ChangeState(ActionUnit.UnitState.Idle);
                return;
            }
            else if (Time.time - _host.TimeOfLastAttack > ((ActionUnitData)_host.CurrentStatus).baseAttackRate)
            {
                Debug.DrawLine(_host.transform.position, _host.TargetAttack.transform.position, Color.red, 0.5f);
                _host.AnimateAttack();
                // _host._characterAnimationEventCalls.OnAttack.AddListener(_attackUnit.DamageTarget);
                _host.TimeOfLastAttack = Time.time;
            }
        }
        public void Enable()
        {
        }

        public void Disable()
        {
            _host.ChangeState(ActionUnit.UnitState.Idle);
        }
    }

    class MoveState : IActionUnitState
    {
        private Vector3 _lastPosition;
        private float _timeStuck = 0;
        ActionUnit _host;

        public MoveState(ActionUnit host)
        {
            _host = host;
        }
        public void Update()
        {
            _timeStuck += Time.deltaTime;
            if (_timeStuck >= 1f)
            {
                if (_lastPosition == null)
                {
                    _lastPosition = _host.transform.position;
                    _timeStuck = 0;
                }
                else
                {
                    if ((_host.transform.position - _lastPosition).sqrMagnitude <= 1f)
                    {
                        Debug.Log("Stuck, change Target");
                        _host.GetComponent<ActionUnitFindTarget>().MarkCantReachTarget(_host.TargetAttack);
                    }
                    _lastPosition = _host.transform.position;
                    _timeStuck = 0;
                }
            }
            Debug.DrawLine(_host.transform.position, _host.TargetAttack.transform.position, Color.green);
        }

        public void Enable()
        {
        }

        public void Disable()
        {

        }
    }

}
