using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollision : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");
        Monster m = gameObject.transform.parent.transform.parent.GetComponent<Monster>();
        if ((other.tag == "Board")) { }

        if (other.gameObject.CompareTag("CubeMonster"))
        {
            Monster enemy = other.transform.parent.transform.parent.GetComponent<Monster>();
            if (m.TargetAttack != null && m.TargetAttack.MonsterID == enemy.MonsterID)
            {
                Transform parent = gameObject.transform, foundParent = null;
                do
                {
                    parent = parent.transform.parent;
                    if (parent != null && parent.name == "Monster")
                    {
                        foundParent = parent;
                        Debug.Log("found");
                    }
                }
                while (!(parent == null || foundParent != null));
                if (m.MonsterMove != null)
                {
                    Debug.Log("found monster move");
                    m.MonsterMove.StopMoving();
                }
            }
        }

    }

    void OnCollisionEnter(Collision info)
    {
        info.rigidbody.velocity = Vector3.zero;
        Monster m = gameObject.transform.parent.transform.parent.GetComponent<Monster>();
        if (m.MonsterMove != null)
        {
            Debug.Log("found monster move");
            m.MonsterMove.StopMoving();
        }
    }

    void OnCollisionStay(Collision info)
    {
    }

    void OnCollisionExit(Collision info)
    {
    }
}
