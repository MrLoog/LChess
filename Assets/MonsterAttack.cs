using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : WarEntity, DealDmgAble
{
    public bool Active { get; set; }
    public MonsterAttack NextAvaiable { get; set; }
    Vector3 launchPoint, targetPoint, launchVelocity;
    float speed;
    public Monster Owner { get; set; }
    public bool IsDone = false;
    public bool SourceExists { get; set; } = true;
    float distance = 0;
    public float MinDmg { get; set; } = 1;
    public float MaxDmg { get; set; } = 10;
    float ExistsTime = 10;

    public void Initialize(
        Vector3 launchPoint, Vector3 targetPoint, float speed
    )
    {
        transform.Find("Model/Sphere").GetComponent<Renderer>().material = new Material(Owner.SelectedMat);
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.speed = speed;
        IsDone = false;
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
            MarkDone();
            return false;
        }

        return true;
    }


    void Update()
    {
        if (IsDone) return;
        progress += Time.deltaTime;
        float move = speed * progress / distance;
        transform.localPosition = Vector3.LerpUnclamped(launchPoint, targetPoint, move);
        if (move >= 1)
        {
            progress = 0;
            IsDone = true;
            MarkDone();
            return;
        }

    }

    public void Destroy()
    {
        //OriginFactory.Reclaim(this);
        IsDone = true;
    }

    public void MarkDone()
    {
        //OriginFactory.Reclaim(this);
        IsDone = true;
        BulletPooler.Instance.RePooledObject(this.gameObject);
    }

    public float GetDamage()
    {
        return Random.Range(MinDmg, MaxDmg);
    }

    public bool ApplyDamage(ReceivedDmgAble receivedDmgAble)
    {
        receivedDmgAble.SetCurrentHealth(receivedDmgAble.GetCurrentHealth() - GetDamage());
        MarkDone();
        return true;
    }
}
