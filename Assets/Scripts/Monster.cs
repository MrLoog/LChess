using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour, ReceivedDmgAble
{
    public static int CountMonster = 0;
    public int MonsterID = 0;
    MonsterFactory originFactory;
    public DestroyAbleObj OnDestroyNotify { get; internal set; }
    public bool BattleMode { get; set; } = false;
    public string Test { get; set; }


    public Board board { get; set; }

    public Material mat;
    public Material enemyMat;
    public Monster TargetAttack;

    [SerializeField]
    public Material SelectedMat;

    private GameTile StandTile;

    private int group = 0;

    public int Group
    {
        get => group;
        set
        {
            group = value;
            SelectedMat = (group == 0 ? mat : enemyMat);
            transform.Find("Model/Cube").GetComponent<Renderer>().material = SelectedMat;
        }
    }

    public MonsterFactory OriginFactory
    {
        get => originFactory;
        set
        {
            originFactory = value;
        }
    }
    public void SpawnOn(GameTile tile)
    {
        transform.localPosition = tile.transform.localPosition;
        tile.Monster = this;
        StandTile = tile;
        Health = Random.Range(0.0f, 1.0f) * 100;
        OnDestroyNotify = new DestroyAbleObj(this);
    }

    private float speed;
    public bool Moving = false;
    private Vector3 moveTo;
    private Vector3 moveFrom;
    float progress;
    public MonsterMove MonsterMove;
    List<MonsterAttack> monsterAttacks = new List<MonsterAttack>();
    public MonsterSight MonsterSight { get; set; } = default;

    public void EnterBattleMode(MonsterSight sight)
    {
        MonsterSight = sight;
        BattleMode = true;
        //speed = 0.1f;
        //progress = 0f;
        //moveFrom = this.StandTile.transform.position;
        //moveTo = target.StandTile.transform.position;
        //moving = true;
    }

    float rangeAttack = 2f;
    float attackSpeed = 1f;
    float attackTime = 0;

    void Awake()
    {
        monstersLayerMask = 1 << LayerMask.NameToLayer("Monster");
    }

    bool IsTargetInsideRange()
    {
        if (TargetAttack == null) return false;
        return Vector3.Distance(this.transform.position, TargetAttack.transform.position) < rangeAttack;
    }

    void Update()
    {
        if (Health <= 0)
        {
            Destroy();
            Debug.Log(string.Format("{0} out of health, I'm dead", MonsterID));
            originFactory.Reclaim(this);
            return;
        }
        if (Moving)
        {
            MonsterMove.GameUpdate();
            if (IsTargetInsideRange())
            {
                MonsterMove.StopMoving();
            }
            //progress += Time.deltaTime;
            //transform.localPosition = Vector3.LerpUnclamped(moveFrom, moveTo, progress);
            //if (progress >= 1)
            //{
            //    moving = false;
            //    progress = 0;
            //}
        }
        if (BattleMode)
        {
            if (!Moving && !IsTargetInsideRange())
            {
                TargetAttack = null;
            }
            if (TargetAttack == null)
            {
                AcquireTarget();
            }
        }
        foreach (MonsterAttack attack in monsterAttacks)
        {
            attack.GameUpdate();
        }
        for (var i = (monsterAttacks.Count - 1); i >= 0; i--)
        {
            if (monsterAttacks[i].IsDone)
            {
                MonsterAttack attack = monsterAttacks[i];
                if (TargetAttack != null)
                {
                    float damage = new DmgCalculate(TargetAttack, attack).PerformDamage();
                    Debug.Log(string.Format("Monster {0} Received {1} Damage Current Health {2}", TargetAttack.MonsterID, damage, TargetAttack.Health));
                }
                StopAttack(attack);
                attack.Destroy();
            }
        }
        if ((IsDoneMoving() || !Moving) && TargetAttack != null)
        {
            attackTime += Time.deltaTime;
            float attack = attackSpeed * attackTime / 1;
            if (attack >= 1)
            {
                Attack();
                attackTime = 0;
            }
        }

    }

    private bool IsDoneMoving()
    {
        NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
        return agent.pathStatus == NavMeshPathStatus.PathComplete;
    }

    private void Destroy()
    {
        Debug.Log(string.Format("{0} destroy", MonsterID));
        OnDestroyNotify.NotifyAllObserver();
        foreach (var att in monsterAttacks)
        {
            att.Destroy();
        }
    }

    private void Attack()
    {
        MonsterAttack attack = Game.SpawnMonsterAttack();
        attack.Owner = this;
        attack.Initialize(new Vector3(
            transform.localPosition.x,
            transform.localPosition.y + transform.localScale.y * 0.25f,
            transform.localPosition.z),
            new Vector3(
            TargetAttack.transform.localPosition.x,
            TargetAttack.transform.localPosition.y + TargetAttack.transform.localScale.y * 0.25f,
            TargetAttack.transform.localPosition.z)
            , 0.5f);
        monsterAttacks.Add(attack);
    }

    int monstersLayerMask;

    public void StopAttack(MonsterAttack attack)
    {
        monsterAttacks.Remove(attack);
    }

    bool AcquireTarget()
    {
        Collider[] targets = Physics.OverlapSphere(
            transform.localPosition, MonsterSight.GetSight(), monstersLayerMask
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
                if (monster.MonsterID != this.MonsterID && monster.group != this.group)
                {
                    float distance = Vector3.Distance(transform.position, monster.transform.position);
                    if (shortestDistance < 0f || distance < shortestDistance)
                    {
                        Debug.Log("Check distance");
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
        TargetAttack = null;
        return false;
    }

    private void MarkTargetAttack(Monster monster)
    {
        TargetAttack = monster;
        TargetAttack.OnDestroyNotify.Attach(o =>
        {
            TargetAttack = null;
        });

        MonsterMove = new MonsterMove(this, TargetAttack.transform.position, 3f);
        MonsterMove.StartMoving();
    }

    public float Health = 100;

    public float GetCurrentHealth()
    {
        return Health;
    }

    public void SetCurrentHealth(float health)
    {
        Health = health;
    }


}
