using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseCharacterwNav : MonoBehaviour
{
    public Transform target; // where we're going
    private NavMeshAgent nma; // Unity nav agent
    public float probeRange = 3.0f; // how far the character can "see"
    private bool obstacleAvoid = false; // internal var
    public float turnSpeed = 50f; // how fast to turn

    // create empty game objects and place them appropriately infront, to the left and right of our object
    // This creates a little buffer around the character, and I had some trouble with never raycasting
    // outside the character rigidbody/collider
    public Transform probePoint; // forward probe point
    public Transform leftR; // left probe point
    public Transform rightR; // right probe point

    private Transform obstacleInPath; // we found something!  
    private bool StopAttack = false;
    private Vector3 lastPosition;

    // Use this for initialization
    void Start()
    {
        nma = this.GetComponent<NavMeshAgent>();
        updatePosition();
    }

    private void updatePosition()
    {
        // if (probePoint == null)
        //     probePoint = transform;
        // if (leftR == null)
        // {
        //     leftR = transform;
        // }
        // if (rightR == null)
        //     rightR = transform;
        probePoint = transform;
        leftR = transform;
        rightR = transform;
    }

    void Update()
    {
        // return;
        if (!target || StopAttack) return;
        
        RaycastHit hit;
        Vector3 dir = (target.position - transform.position).normalized;
        //     
        bool previousCastMissed = true; // no need to keep testing if something already hit
        // this is the main forward raycast
        if (Physics.Raycast(probePoint.position, transform.forward, out hit, probeRange))
        {
            if (obstacleInPath != target.transform)
            { // ignore our target
                Debug.Log("Found an object in path! - " + gameObject.name);
                Debug.DrawLine(transform.position, hit.point, Color.green);
                previousCastMissed = false;
                obstacleAvoid = true;
                nma.isStopped = true;
                nma.ResetPath();
                if (hit.transform != transform)
                {
                    obstacleInPath = hit.transform;
                    Debug.Log("I hit: " + hit.transform.gameObject.name);
                    dir += hit.normal * turnSpeed;

                    Debug.Log("moving around an object - " + gameObject.name);

                }
            }
        }
        // if we did see something before, but now the forward raycast is turned out of range, check the sides
        // without this, the character bumps into the object and sort of bounces (usually) until it gets
        // past.  This is a better approach :)
        /*
        if (obstacleAvoid && previousCastMissed && Physics.Raycast(leftR.position, transform.forward, out hit, probeRange))
        {
            if (obstacleInPath != target.transform)
            { // ignore our target
                Debug.DrawLine(leftR.position, hit.point, Color.red);
                obstacleAvoid = true;
                nma.isStopped = true;
                if (hit.transform != transform)
                {
                    obstacleInPath = hit.transform;
                    previousCastMissed = false;
                    //Debug.Log("moving around an object");
                    dir += hit.normal * turnSpeed;
                }
            }
        }
        // check the other side :)
        if (obstacleAvoid && previousCastMissed && Physics.Raycast(rightR.position, transform.forward, out hit, probeRange))
        {
            if (obstacleInPath != target.transform)
            { // ignore our target
                Debug.DrawLine(rightR.position, hit.point, Color.green);
                obstacleAvoid = true;
                nma.isStopped = true;
                if (hit.transform != transform)
                {
                    obstacleInPath = hit.transform;
                    dir += hit.normal * turnSpeed;
                }
            }
        }
*/
        // turn Nav back on when obstacle is behind the character!!
        if (obstacleInPath != null)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = obstacleInPath.position - transform.position;
            if (Vector3.Dot(forward, toOther) < 0)
            {
                //print("The other transform is behind me!");
                Debug.Log("Back on Navigation! unit - " + gameObject.name);
                obstacleAvoid = false; // don't let Unity nav and our avoidance nav fight, character does odd things
                obstacleInPath = null; // Hakuna Matata
                nma.ResetPath();
                nma.SetDestination(target.position);
                nma.isStopped = false; // Unity nav can resume movement control
            }

        }
        //     
        // this is what actually moves the character when under avoidance control
        if (obstacleAvoid)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            lastPosition = transform.position;
            transform.position += transform.forward * nma.speed * Time.deltaTime;
            Debug.Log(string.Format("Move From {0} to {1}", lastPosition.ToString(), transform.position.ToString()));
        }
        else
        {
            nma.SetDestination(target.position);
        }
    }

    public void SetTarget(Transform tIn)
    {
        target = tIn;
        if (tIn != null)
        {
            StopAttack = false;
            nma.isStopped = false;
            // nma.SetDestination(target.position);
        }
        else
        {
            StopAttack = true;
            nma.isStopped = true;
        }
    }
}
