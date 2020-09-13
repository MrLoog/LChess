using Assets.Scripts;
using Assets.Scripts.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour, ReceivedDmgAble
{
    public enum MonsterState
    {
        Idle, Move, Attack
    }
    //common prop
    [SerializeField]
    public Material SelectedMat;
    public Material mat;
    public Material enemyMat;
    public int MonstersLayerMask { get; private set; }

    public int MonsterID = 0;
    public static int TotalMonster { get; set; } = 0;
    private int _group = 0;
    public int Group
    {
        get => _group;
        set
        {
            _group = value;
            SelectedMat = (_group == 0 ? mat : enemyMat);
            transform.Find("Model/Cube").GetComponent<Renderer>().material = SelectedMat;
        }
    }

    public bool BattleMode = false;
    public Monster TargetAttack;
    public Board Board { get; set; }
    public MonsterSight MonsterSight { get; set; } = default;

    public DestroyAbleObj OnDestroyNotify { get; internal set; } = default;
    public MonsterFactory OriginFactory { get; set; }
    

    #region attribute monster statistic
    public float RangeAttack = 2f;
    public float Health = 100;
    public float DefaultHealth = 10000;
    public float AttackSpeed = 1f;
    public float MovementSpeed = 1f;
    public MonsterState State = MonsterState.Idle;





    #endregion



    #region logic spawn
    public void SpawnOn(GameTile tile)
    {
        transform.localPosition = tile.transform.localPosition;
        tile.Monster = this;
        float rate = Random.Range(0.0f, 1.0f);
        Health = rate * DefaultHealth;
        float size = 0.5f + 0.5f * rate;
        transform.localScale = new Vector3(size, size, size);
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


    #endregion logic spawn

    public void Start()
    {
    }

    //undetermine



    public void EnterBattleMode(MonsterSight sight)
    {
        MonsterSight = sight;
        BattleMode = true;
    }


    void Awake()
    {
        MonstersLayerMask = 1 << LayerMask.NameToLayer("Monster");
    }

    void Update()
    {
        for (int i = 0; i < MAX_SIZE_COMPONENT; i++)
        {
            if (components[i] != null) components[i].Update();
        }


        if (Health <= 0)
        {
            Debug.Log(string.Format("{0} out of health, I'm dead", MonsterID));
            Destroy();
            OriginFactory.Reclaim(this);
            return;
        }
    }


    private void Destroy()
    {
        Debug.Log(string.Format("{0} destroy", MonsterID));
        OnDestroyNotify.NotifyAllObserver();
    }

    public bool ChangeState(MonsterState newState)
    {
        State = newState;
        return true;
    }


    #region ReceivedDmgAble
    public float GetCurrentHealth()
    {
        return Health;
    }

    public void SetCurrentHealth(float health)
    {
        Health = health;
        float rate = health / DefaultHealth;
        float size = 0.5f + 0.5f * rate;
        transform.localScale = new Vector3(size, size, size);
    }

    public void ApplyDamage(float damage)
    {
        Health -= damage;
    }
    #endregion
}
