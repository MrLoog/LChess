using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class ActionUnitFindTarget : MonoBehaviour
{

    private ActionUnit Host;

    public bool CheckPathEnable = false;

    public bool LockTarget = false;
    public bool LockToDeath = false;
    List<int> _unreachable = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        Host = gameObject.GetComponent<ActionUnit>();
        CharacterAnimationEventCalls eventUnit = gameObject.GetComponentInChildren<CharacterAnimationEventCalls>();
        eventUnit.RegisterListener(CharacterAnimationEventCalls.K_STATE_ATTACK_IN).AddListener(UnlockTarget);

    }

    // Update is called once per frame
    void Update()
    {
        if (IsNeedFindTarget())
        {
            Debug.Assert(!LockToDeath,"Lock to death true, still find target");
            AcquireTarget();
        }else{

        }
    }

    private bool IsNeedFindTarget()
    {
        if (LockToDeath && Host.TargetAttack != null && Host.TargetAttack.Alive) return false;
        else if (LockToDeath)
        {
            LockToDeath = false;
        }
        if (!Host.BattleMode || Host.State.Equals(ActionUnit.UnitState.Attack)) return false;
        /*
        if (Host.TargetAttack != null)
        {
            //outside range
            return Vector3.Distance(Host.transform.position, Host.TargetAttack.transform.position) > Host.RangeAttack;
        }
        */
        return true;
    }


    bool AcquireTarget()
    {
        ActionUnit closest = null;
        IOrderedEnumerable<ActionUnit> potentialTarget = ActionUnitManger.Instance.GetAll()
                        .Where(x => x.UnitID != Host.UnitID && x.Group != Host.Group && x.Alive && x.BattleMode)
                        .OrderBy(x => (Host.transform.position - x.transform.position).magnitude);
        NavMeshAgent nma = Host.GetComponent<NavMeshAgent>();
        NavMeshObstacle nmo = Host.GetComponent<NavMeshObstacle>();
        closest = potentialTarget.Where(x => _unreachable.IndexOf(x.UnitID) == -1).FirstOrDefault();
        if (closest)
        {
            MarkTargetAttack(closest);
            return true;
        }
        closest = potentialTarget.FirstOrDefault();
        if (closest)
        {
            _unreachable.Clear(); //k tìm thấy mục tiêu thay thế thì giữ nguyên
            MarkTargetAttack(closest);
            return true;
        }
        MarkTargetAttack(null);
        return false;
    }

    public void UnlockTarget()
    {
        Debug.Log("Unlock target");
        CheckPathEnable = false;
        LockTarget = false;
        _unreachable.Clear();
        // CharacterAnimationEventCalls eventUnit = GetComponentInChildren<CharacterAnimationEventCalls>();
        // eventUnit.RegisterListener(CharacterAnimationEventCalls.K_STATE_ATTACK_IN).RemoveListener(UnlockTarget);
    }

    bool AcquireTargetOld()
    {
        Collider[] targets = Physics.OverlapSphere(
            gameObject.transform.localPosition, CalculateSight(Host), Host.MonstersLayerMask
        );
        if (targets.Length > 1)
        {
            //target = targets[0].GetComponent<TargetPoint>();
            //Debug.Assert(target != null, "Targeted non-enemy!", targets[0]);
            float shortestDistance = -1f;
            ActionUnit temTarget = null;

            for (var i = 0; i < targets.Length; i++)
            {
                ActionUnit monster = targets[i].transform.parent.transform.parent.GetComponent<ActionUnit>();
                if (monster.UnitID != Host.UnitID && monster.Group != Host.Group)
                {
                    float distance = Vector3.Distance(Host.transform.position, monster.transform.position);
                    if (shortestDistance < 0f || distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        temTarget = monster;
                    }
                }
            }
            if (temTarget != null)
            {
                MarkTargetAttack(temTarget);
            }
            return true;
        }
        MarkTargetAttack(null);
        return false;
    }

    private float CalculateSight(ActionUnit host)
    {
        return Game.Instance.boardSize.magnitude;
    }

    public void MarkTargetAttack(ActionUnit monster)
    {
        if (Host.TargetAttack == monster) return;
        if (Host.TargetAttack != null)
        {
            Debug.Log("Change Target");
            // Host.TargetAttack.OnDestroyNotify.Detach(OnTargetDestroy);
            Host.ChangeState(ActionUnit.UnitState.Idle);
        }
        Host.TargetAttack = monster;
        if (monster != null)
        {
            // Host.TargetAttack.OnDestroyNotify.Attach(OnTargetDestroy);
            if (Host.IsEnemyInRangeAttack())
            {
                Host.ChangeState(ActionUnit.UnitState.Attack);
            }
            else
            {
                Host.ChangeState(ActionUnit.UnitState.Move);

            }
        }
    }

    internal void MarkCantReachTarget(ActionUnit targetAttack)
    {
        _unreachable.Add(targetAttack.UnitID);
    }
}
