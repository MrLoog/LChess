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
            AcquireTarget();
        }
    }

    private bool IsNeedFindTarget()
    {
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
        if (!CheckPathEnable && LockTarget && Host.TargetAttack != null && Host.TargetAttack.Alive) return true;
        ActionUnit closest = null;
        IOrderedEnumerable<ActionUnit> potentialTarget = ActionUnitManger.Instance.GetAll()
                        .Where(x => x.UnitID != Host.UnitID && x.Group != Host.Group && x.Alive)
                        .OrderBy(x => (Host.transform.position - x.transform.position).magnitude);
        NavMeshAgent nma = Host.GetComponent<NavMeshAgent>();
        NavMeshObstacle nmo = Host.GetComponent<NavMeshObstacle>();
        if (!CheckPathEnable)
        {
            closest = potentialTarget.FirstOrDefault();

        }
        else
        {
            bool stateNma = nma.enabled;
            if (!stateNma)
            {
                nmo.enabled = false;
                nma.enabled = true;
            }
            NavMeshPath path = new NavMeshPath();
            Debug.Log(string.Format("find target check path {0}", potentialTarget.Count()));
            foreach (ActionUnit checkTarget in potentialTarget)
            {
                nma.ResetPath();
                nma.CalculatePath(checkTarget.transform.position, path);
                if (path.status.Equals(NavMeshPathStatus.PathComplete))
                {
                    closest = checkTarget;
                    LockTarget = true;
                    CheckPathEnable = false;
                    Debug.Log("Found Target have path");
                    for (int i = 0; i < path.corners.Length - 1; i++)
                    {
                        Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.yellow, 1f);
                    }
                    break;
                }
                else
                {
                    Debug.Log("Found Target cant reach");
                }
            }
            if (!stateNma)
            {
                nma.enabled = false;
                nmo.enabled = true;
            }
        }

        if (closest)
        {
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

    private void MarkTargetAttack(ActionUnit monster)
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
}
