using System;
using UnityEngine;

namespace Assets.Scripts.Component
{
    public class MonsterFindEnemy : LComponent
    {
        //logic find enemy
        //scope
        //target type
        Monster Host { get; }
        Action<object> OnTargetDestroy;

        public MonsterFindEnemy(Monster host)
        {
            Host = host;
            OnTargetDestroy = (o) => {
                Debug.Log("remove listen");
                Host.TargetAttack.OnDestroyNotify.Detach(OnTargetDestroy);
                Host.TargetAttack = null;
            };
        }

        public override bool Update()
        {
            if (IsNeedFindTarget())
            {
                AcquireTarget();
            }
            return true;
        }

        private bool IsNeedFindTarget()
        {
            if (!Host.BattleMode) return false;
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
            Collider[] targets = Physics.OverlapSphere(
                Host.transform.localPosition, CalculateSight(Host), Host.MonstersLayerMask
            );
            if (targets.Length > 1)
            {
                //target = targets[0].GetComponent<TargetPoint>();
                //Debug.Assert(target != null, "Targeted non-enemy!", targets[0]);
                float shortestDistance = -1f;
                Monster temTarget = null;

                for (var i = 0; i < targets.Length; i++)
                {
                    Monster monster = targets[i].transform.parent.transform.parent.GetComponent<Monster>();
                    if (monster.MonsterID != Host.MonsterID && monster.Group != Host.Group)
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

        private float CalculateSight(Monster host)
        {
            return Game.Instance.boardSize.magnitude;
        }

        private void MarkTargetAttack(Monster monster)
        {
            if (Host.TargetAttack ==  monster) return;
            if (Host.TargetAttack != null)
            {
                Debug.Log("Change Target");
                Host.TargetAttack.OnDestroyNotify.Detach(OnTargetDestroy);
                Host.ChangeState(Monster.MonsterState.Idle);
            }
            Host.TargetAttack = monster;
            if (monster != null)
            {
                Host.TargetAttack.OnDestroyNotify.Attach(OnTargetDestroy);
                Host.ChangeState(Monster.MonsterState.Move);
            }
        }
    }
}
