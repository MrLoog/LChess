using UnityEngine;
using UnityEngine.AI;

public class ActionUnitMove : MonoBehaviour
{
    private ActionUnit Host;
    private Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        Host = gameObject.GetComponent<ActionUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Host.State.Equals(ActionUnit.UnitState.Move)) return;

        NavMeshAgent agent = Host.GetComponent<NavMeshAgent>();
        //agent.destination = target;
        agent.isStopped = false;
        agent.SetDestination(Host.TargetAttack.transform.position);
        // Host.GetComponent<BaseCharacterwNav>().SetTarget(Host.TargetAttack.transform);
        Host.FaceTarget();
        // Host.ChangeState(ActionUnit.UnitState.Move);
        // if (agent.velocity.sqrMagnitude > Mathf.Epsilon)
        // {
        //     FaceTarget(Host.TargetAttack.transform.position);
        //     // Host.transform.rotation = Quaternion.Lerp(Host.transform.rotation, Quaternion.LookRotation(agent.velocity.normalized), 0.1f);
        // }

        if (Host.IsEnemyInRangeAttack())
        {
            //stop moving
            Debug.Log("Inside Range Attack");
            agent.isStopped = true;
            // Host.GetComponent<BaseCharacterwNav>().SetTarget(null);

            Host.ChangeState(ActionUnit.UnitState.Attack);
        }

    }





    private bool IsHasTargetMoving()
    {
        if (Host.TargetAttack != null && !Host.IsEnemyInRangeAttack())
        {
            target = Host.TargetAttack.transform.position;
            return true;
        }
        return false;

    }
}
