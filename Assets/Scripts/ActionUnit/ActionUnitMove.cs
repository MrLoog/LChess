using UnityEngine;
using UnityEngine.AI;

public class ActionUnitMove : MonoBehaviour
{
    private ActionUnit Host;
    private Vector3 target;

    Vector3 prevPos;
    float _processCount;
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
        // TestCalculateSpeed();
        agent.SetDestination(Host.TargetAttack.transform.position);
        // Host.GetComponent<BaseCharacterwNav>().SetTarget(Host.TargetAttack.transform);
        // Host.FaceTarget();
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

    private void TestCalculateSpeed()
    {
        Transform model = FindChild(Host.transform, "Spider_Armature");
        _processCount += Time.deltaTime;
        if (prevPos == null)
        {
            prevPos = model.position;
            _processCount = 0;
        }
        Debug.Log(string.Format("Speed {0}/{1}", _processCount, Host.GetComponent<ActionUnit>().walkTimeClip));
        if (_processCount > Host.GetComponent<ActionUnit>().walkTimeClip)
        {
            if (prevPos != null)
            {
                Debug.Log(string.Format("Speed {0} : {1} : {2} : {3}", Host.tileUnitData.unitName, Vector3.Distance(prevPos, model.position) / _processCount, prevPos, model.position));
            }
            _processCount = 0;
            prevPos = model.position;
        }
    }
    private Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = FindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
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
