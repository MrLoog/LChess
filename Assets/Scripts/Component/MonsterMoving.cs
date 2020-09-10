using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Component
{
    public class MonsterMoving : LComponent
    {
        Monster Host { get; }

        private Vector3 target;
        //private float speed = 0.5f;

        //public float Speed { get; set; } = 0.5f;

        //private float current;
        //float distance;
        //float progress;
        //private Vector3 moveTo;
        //private Vector3 moveFrom;

        public MonsterMoving(Monster host)
        {
            Host = host;
        }

        public override bool Update()
        {

            if (IsHasTargetMoving())
            {
                //Moving
                Host.Moving = true;
                NavMeshAgent agent = Host.GetComponent<NavMeshAgent>();
                //agent.destination = target;
                agent.isStopped = false;
                agent.SetDestination(target);
            }
            if (Host.Moving && IsNeedStopMoving())
            {
                //stop moving
                Host.Moving = false;

                NavMeshAgent agent = Host.GetComponent<NavMeshAgent>();
                agent.isStopped = true;
            }
            return true;
        }

        private bool IsNeedStopMoving()
        {
            return IsEnemyInRangeAttack();

        }

        private bool IsEnemyInRangeAttack()
        {
            if (Host.TargetAttack == null) return false;
            return Vector3.Distance(Host.transform.position, Host.TargetAttack.transform.position) < Host.RangeAttack;
        }


        private bool IsHasTargetMoving()
        {
            if (Host.TargetAttack != null && !IsEnemyInRangeAttack())
            {
                target = Host.TargetAttack.transform.position;
                return true;
            }
            return false;

        }
    }
}
