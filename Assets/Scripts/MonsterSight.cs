using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSight
{
    private Monster Monster;

    private Game Game;

    public MonsterSight(Monster monster, Game game)
    {
        this.Monster = monster;
        this.Game = game;
    }

    public Monster AcquireTarget()
    {
        Monster foundEnemy = null;
        float distance = -1f;
        for (var i = 0; i < Game.monsters.Count; i++)
        {
            Monster mCheck = Game.monsters[i];
            if (mCheck.Group != Monster.Group && !mCheck.Equals(Monster))
            {
                float newDistance = Vector3.Distance(Monster.transform.position, mCheck.transform.position);
                if (distance == -1 || newDistance < distance)
                {
                    foundEnemy = mCheck;
                    distance = newDistance;
                }
            }
        }
        return foundEnemy;
    }

    internal float GetSight()
    {
        return Game.boardSize.magnitude;
    }
}
