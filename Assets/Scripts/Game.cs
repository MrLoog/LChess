using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static string TAG_MONSTER = "CubeMonster";
    public static string TAG_BOARD = "Board";
    public static string TAG_BULLET = "Bullet";

    public static Game Instance { get; private set; }
    public int MAX_RANDOM_SPAWN = 10;
    private int _spawnCount = 0;

    [SerializeField]
    public Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField]
    Board board = default;

    [SerializeField]
    MonsterFactory monsterFactory = default;


    [SerializeField]
    WarFactory warFactory = default;

    public MonsterManager monsterManager = new MonsterManager();
    public GameBullet gameBullet = new GameBullet();

    static Game instance;

    public static MonsterAttack SpawnMonsterAttack()
    {
        MonsterAttack attack = instance.warFactory.AnAttack;
        return attack;
    }

    void OnEnable()
    {
        instance = this;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);

        board.Initialize(boardSize);
        board.ShowGrid = true;
    }
    void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomMode = !RandomMode;
            Debug.Log(string.Format("Random Mode {0}", RandomMode));
        }
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        if (RandomMode)
        {
            _randomProcess += Time.deltaTime;
            _cumUpTimeRandom += Time.deltaTime;
            if (_spawnCount >= MAX_RANDOM_SPAWN)
            {
                _cumUpTimeRandom = 0;
                _randomProcess = 0;
                _spawnCount = 0;
                RandomMode = false;
            }
            else if (_randomProcess >= RandomDelay)
            {
                _randomProcess = 0f;
                RandomSpawn();
                _spawnCount++;
            }
        }

    }

    bool RandomMode { get; set; } = false;
    float RandomDelay { get; set; } = 1f;
    float _randomProcess = 0f;
    public float MaxTimeRandom { get; set; } = 30f;
    public float _cumUpTimeRandom { get; set; } = 0f;

    private void RandomSpawn()
    {
        GameTile emptyTile = null;
        do
        {
            int x = Mathf.RoundToInt(Random.Range(0.0f, boardSize.x - 1));
            int y = Mathf.RoundToInt(Random.Range(0.0f, boardSize.y - 1));
            emptyTile = board.GetTile(x, y);
            if (emptyTile.Monster != null)
            {
                emptyTile = null;
            }
        } while (emptyTile == null && monsterManager.Total < ((boardSize.x) * (boardSize.y)));
        if (emptyTile == null) return;
        int totalGroup1 = monsterManager.GetTotalGroup(0);
        int totalGroup2 = monsterManager.GetTotalGroup(1);

        int group = 0;
        if (totalGroup1 + totalGroup2 > 0f)
        {
            float p1 = (float)totalGroup1 / (totalGroup1 + totalGroup2);
            float p2 = 1f + (float)totalGroup1 / (totalGroup1 + totalGroup2);
            group = Mathf.RoundToInt(Random.Range(p1, p2) * 0.5f);
            Debug.Log(string.Format("Random from {0} - {1} - {2}", p1, p2, group));
        }
        else
        {
            group = Mathf.RoundToInt(Random.Range(0f, 1f));
        }

        RandomSpawnMonster(emptyTile, group);
    }

    private void RandomSpawnMonster(GameTile tile, int group)
    {
        Monster monster = monsterFactory.Get();
        monster.SpawnOn(tile);
        monster.MonsterID = ++Monster.TotalMonster;
        monster.Group = group;
        monsterManager.Add(monster);
        monster.OnDestroyNotify.Attach(o =>
        {
            monsterManager.Remove((Monster)o);
        });

        MonsterSight sight = new MonsterSight(monster, this);
        monster.EnterBattleMode(sight);
    }

    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        Monster remove = monsterManager.GetMonster(TouchRay);
        if (remove != null)
        {
            monsterManager.Remove(remove);
            monsterFactory.Reclaim(remove);
        }
        else
        {
            RandomSpawnMonster(tile, Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
        }
        //GameTile tile = Board.GetTile(TouchRay);
        //if (tile != null)
        //{
        //    if (tile.Monster != null)
        //    {
        //        monsterManager.Remove(tile.Monster);
        //        monsterFactory.Reclaim(tile.Monster);
        //    }
        //    else
        //    {
        //        RandomSpawnMonster(tile, Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
        //    }
        //}
    }


}
