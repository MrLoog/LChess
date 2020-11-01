using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterManager
{
    List<Monster> monsters = new List<Monster>();
    public int Total => monsters.Count;

    internal int GetTotalGroup(int group)
    {
        return monsters.Where(a => a.Group == group).Count();
    }

    internal void Add(Monster monster)
    {
        monsters.Add(monster);
    }

    internal List<Monster> GetAll()
    {
        return monsters;
    }

    internal void Remove(Monster o)
    {
        monsters.Remove(o);
    }

    public Monster GetMonster(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == Game.TAG_MONSTER)
            {
                return FindObjScript.GetObjScriptFromCollider<Monster>(hit.collider);
            }
        }
        return null;
    }
}
