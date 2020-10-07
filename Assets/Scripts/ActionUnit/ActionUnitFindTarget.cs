using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionUnitFindTarget : MonoBehaviour
{

    private ActionUnit Host;
    // Start is called before the first frame update
    void Start()
    {
        Host = gameObject.GetComponent<ActionUnit>();
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
        ActionUnit closest = ActionUnitManger.Instance.GetAll()
                    .Where(x => x.UnitID != Host.UnitID && x.Group != Host.Group && x.Alive)
                    .OrderBy(x => (Host.transform.position - x.transform.position).magnitude)
                    .FirstOrDefault();

        if (closest)
        {
            MarkTargetAttack(closest);
            return true;
        }
        MarkTargetAttack(null);
        return false;
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
