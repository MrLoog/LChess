using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Component
{
    public class MonsterAttacking : LComponent
    {
        Monster Host { get; }
        List<MonsterAttack> bullets;
        List<MonsterAttack> bulletsDone;
        public float CoolDown = 0;
        public float AttackSpeed => Host.AttackSpeed;

        public MonsterAttacking(Monster host)
        {
            Host = host;
            bullets = new List<MonsterAttack>();
            bulletsDone = new List<MonsterAttack>();
        }

        public override bool Update()
        {
            if (!Host.State.Equals(Monster.MonsterState.Attack)) return true;
            if (IsCanAttack())
            {
                CoolDown += Time.deltaTime;
                if (CoolDown >= AttackSpeed)
                {
                    //spawn attack
                    PerformAttack();
                    CoolDown = 0;
                }
            }
            //for (var i = 0; i < bullets.Count; i++)
            //{
            //    if (bullets[i].IsDone)
            //    {
            //        bulletsDone.Add(bullets[i]);
            //        continue;
            //    }
            //    bullets[i].GameUpdate();
            //}
            //bullets.RemoveAll(a => a.IsDone);
            //RecycleBullet();
            return true;
        }

        private void RecycleBullet()
        {
            for (var i = 0; i < bulletsDone.Count; i++)
            {
                BulletPooler.Instance.RePooledObject(bulletsDone[i].gameObject);
            }
            bulletsDone.Clear();
        }

        private void PerformAttack()
        {
            MonsterAttack newAttack = BulletPooler.Instance.GetPooledObject().GetComponent<MonsterAttack>();
            bullets.Add(newAttack);
            newAttack.Owner = Host;
            newAttack.Initialize(new Vector3(
                Host.transform.localPosition.x,
                Host.transform.localPosition.y + Host.transform.localScale.y * 0.25f,
                Host.transform.localPosition.z),
                new Vector3(
                Host.TargetAttack.transform.localPosition.x,
                Host.TargetAttack.transform.localPosition.y + Host.TargetAttack.transform.localScale.y * 0.25f,
                Host.TargetAttack.transform.localPosition.z)
                , 0.5f);

            Host.OnDestroyNotify.Attach(o =>
            {
                newAttack.Owner = null;
            });
        }

        private bool IsCanAttack()
        {
            //không có mục tiêu không attack
            if (Host.TargetAttack == null) return false;
            return true;
        }
    }
}
