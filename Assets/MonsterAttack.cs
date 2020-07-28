using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : WarEntity, DealDmgAble
{
    Vector3 launchPoint, targetPoint, launchVelocity;
    float speed;
    public Monster Owner { get; set; }
    public bool IsDone = false;
    public bool SourceExists { get; set; } =  true;
    float distance = 0;
    float MinDmg = 1;
    float MaxDmg = 10;

    public void Initialize(
        Vector3 launchPoint, Vector3 targetPoint, float speed
    )
    {
        transform.Find("Model/Sphere").GetComponent<Renderer>().material = new Material(Owner.SelectedMat);
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.speed = speed;
        distance = Vector3.Distance(launchPoint, targetPoint);
        transform.localPosition = launchPoint;
    }

    float progress = 0;

    public bool GameUpdate()
    {
        if (IsDone) return false;
        progress += Time.deltaTime;
        float move = speed * progress / distance;
        transform.localPosition = Vector3.LerpUnclamped(launchPoint, targetPoint, move);
        if (move >= 1)
        {
            progress = 0;
            IsDone = true;
            return false;
        }
        return true;
    }

    public void Destroy()
    {
        OriginFactory.Reclaim(this);
    }

    public float GetDamage()
    {
        if (Owner.gameObject) return Random.Range(MinDmg, MaxDmg);
        return 0f;
    }
}
