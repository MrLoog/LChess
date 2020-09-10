using Assets.Scripts;
using Assets.Scripts.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour, ReceivedDmgAble
{
    //common prop
    [SerializeField]
    public Material SelectedMat;
    public int MonstersLayerMask { get; private set; }
    public int group = 0;
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
    public int MonsterID = 0;
    public DestroyAbleObj OnDestroyNotify { get; internal set; } = default;
    public bool BattleMode { get; set; } = false;
    public Monster TargetAttack;
    private GameTile StandTile;
    public MonsterFactory OriginFactory { get; set; }
    #region attribute monster statistic
    public float RangeAttack { get; private set; } = 2f;
    #endregion



    #region logic spawn
    public void SpawnOn(GameTile tile)
    {
        transform.localPosition = tile.transform.localPosition;
        tile.Monster = this;
        StandTile = tile;
        Health = Random.Range(0.0f, 1.0f) * 100;
        OnDestroyNotify = new DestroyAbleObj(this);
        InitComponet();
    }

    LComponent[] components = new LComponent[MAX_SIZE_COMPONENT];
    private static int MAX_SIZE_COMPONENT = 10;

    private void InitComponet()
    {
        components[0] = new MonsterFindEnemy(this);
        components[1] = new MonsterMoving(this);
        components[2] = new MonsterAttacking(this);
    }

    internal void ApplyDamage(float damage)
    {
        Health -= damage;
    }
    #endregion logic spawn

    public void Start()
    {
    }

    //undetermine

    public static int CountMonster = 0;


    public Board board { get; set; }

    public Material mat;
    public Material enemyMat;





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

    public float attackSpeed = 1f;
    public float attackTime = 0;

    void Awake()
    {
        MonstersLayerMask = 1 << LayerMask.NameToLayer("Monster");
    }
    /*
    bool IsTargetInsideRange()
    {
        if (TargetAttack == null) return false;
        return Vector3.Distance(this.transform.position, TargetAttack.transform.position) < RangeAttack;
    }
    */

    void Update()
    {
        for (int i = 0; i < MAX_SIZE_COMPONENT; i++)
        {
            if (components[i] != null) components[i].Update();
        }


        if (Health <= 0)
        {
            Destroy();
            Debug.Log(string.Format("{0} out of health, I'm dead", MonsterID));
            OriginFactory.Reclaim(this);
            return;
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


    public void StopAttack(MonsterAttack attack)
    {
        monsterAttacks.Remove(attack);
    }

    /*
    bool AcquireTarget()
    {
        Collider[] targets = Physics.OverlapSphere(
            transform.localPosition, MonsterSight.GetSight(), MonstersLayerMask
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
    */

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
