using UnityEngine;

public class OnCollision : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Monster m = gameObject.transform.parent.transform.parent.GetComponent<Monster>();
        if (other.gameObject.CompareTag(Game.TAG_BOARD)) { }

        //else if (other.gameObject.CompareTag("CubeMonster"))
        else if (other.gameObject.CompareTag(Game.TAG_MONSTER))
        {
            //Monster enemy = other.transform.parent.transform.parent.GetComponent<Monster>();
            Monster enemy = FindObjScript.GetObjScriptFromCollider<Monster>(other);
        }
        else if (other.gameObject.CompareTag(Game.TAG_BULLET))
        {
            //MonsterAttack bullet = other.transform.parent.transform.parent.GetComponent<MonsterAttack>();
            MonsterAttack bullet = FindObjScript.GetObjScriptFromCollider<MonsterAttack>(other);
            //if (!m.Equals(bullet.Owner))
            if (bullet.Owner != null && !m.Group.Equals(bullet.Owner.Group))
            {
                bullet.ApplyDamage(m);
            }
        }

    }

    void OnCollisionEnter(Collision info)
    {
    }

    void OnCollisionStay(Collision info)
    {
    }

    void OnCollisionExit(Collision info)
    {
    }
}
