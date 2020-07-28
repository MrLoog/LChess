using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMove
{
    private Monster monster;
    private Vector3 target;
    private float speed = 0.5f;
    private float current;
    float distance;
    float progress;
    private Vector3 moveTo;
    private Vector3 moveFrom;

    public MonsterMove(Monster monster, Vector3 target, float speed)
    {
        this.monster = monster;
        this.target = target;
        this.speed = speed;
        moveTo = target;
        moveFrom = monster.transform.position;
        progress = 0;
        distance = Vector3.Distance(moveFrom, moveTo);
        Debug.Log(distance);
    }


    public bool GameUpdate()
    {
        //if (monster.Moving)
        //{
        //    progress += Time.deltaTime;
        //    float move = speed * progress / distance;
        //    monster.transform.localPosition = Vector3.LerpUnclamped(moveFrom, moveTo, move);
        //    if (move >= 1)
        //    {
        //        progress = 0;
        //        monster.transform.localPosition = moveTo;
        //        monster.Moving = false;
        //        monster.MonsterMove = null;
        //    }
        //}
        return true;
    }

    public void StopMoving()
    {
        monster.Moving = false;

        NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
        agent.isStopped = true;
    }

    internal void StartMoving()
    {
        monster.Moving = true;

        NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
        agent.destination = target;
    }
}
