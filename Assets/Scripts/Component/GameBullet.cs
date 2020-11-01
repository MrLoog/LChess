using System.Collections.Generic;

public class GameBullet
{

    List<MonsterAttack> bullets;
    List<MonsterAttack> bulletsDone;

    public GameBullet()
    {
        bullets = new List<MonsterAttack>();
        bulletsDone = new List<MonsterAttack>();
    }

    public bool Update()
    {
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
}