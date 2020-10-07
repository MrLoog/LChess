using System;
using System.Collections;
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
    public ActionUnitData CurrentStatus;
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

    private float TimeOfLastAttack;

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

    public void SpawnCharacter()
    {
        if (tileUnitData && characterPrefabParent)
        {
            GameObject spawned = Instantiate(tileUnitData.characterPrefab, characterPrefabParent, false);
            characterPrefabParent.transform.ChangeLayersRecursively(gameObject.layer);

            _animator = GetComponentInChildren<Animator>();
            CurrentStatus = new ActionUnitData();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(tileUnitData), CurrentStatus);
            _characterAnimationEventCalls = GetComponentInChildren<CharacterAnimationEventCalls>();
            _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_ATTACK).AddListener(AttackApplyDamage);
        }
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
        Debug.Log(string.Format("Received {0} damage, remain {1} heath", receivedDamage, CurrentStatus.baseHealth));
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

    void Destroy()
    {
        // yield return new WaitForSeconds(0.5f);
        // yield return new WaitForSeconds(0.5f);
        // _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_ATTACK).RemoveListener(AttackApplyDamage);
        // _characterAnimationEventCalls.RegisterListener(CharacterAnimationEventCalls.K_ACTION_DIE).RemoveListener(Destroy);
        Debug.Log("Die and Destroy");
        Game.Instance.DestroyUnit(this);
    }


    public bool IsEnemyInRangeAttack()
    {
        if (TargetAttack == null) return false;
        return Vector3.Distance(transform.position, TargetAttack.transform.position) <= ((ActionUnitData)tileUnitData).baseAttackRange;
    }

    public bool ChangeState(UnitState newState)
    {
        if (newState.Equals(State)) return true;
        UnitState oldState = State;
        if (oldState.Equals(UnitState.Attack))
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
                AnimateIdle();
                break;
            case UnitState.Attack:
                _currentState = _attackState;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshObstacle>().enabled = true;
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
            else if (Time.time - _host.TimeOfLastAttack > ((ActionUnitData)_host.tileUnitData).baseAttackRate)
            {
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
        ActionUnit _host;

        public MoveState(ActionUnit host)
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

}
