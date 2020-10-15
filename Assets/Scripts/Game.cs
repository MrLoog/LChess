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
    ActionUnitFactory actionUnitFactory = default;


    [SerializeField]
    WarFactory warFactory = default;

    public MonsterManager monsterManager = new MonsterManager();
    public ActionUnitManger ActionUnitManger = ActionUnitManger.Instance;
    public GameBullet gameBullet = new GameBullet();

    static Game instance;
    List<TileUnitData> TileUnitDatas { get; set; }



    public static MonsterAttack SpawnMonsterAttack()
    {
        MonsterAttack attack = instance.warFactory.AnAttack;
        return attack;
    }

    private KeyCode[] numberKeyCodes = new KeyCode[] { KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9 };
    public int SelectedSpawnUnit = -1;
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

        Resources.LoadAll<TileUnitData>("ScriptableObjects");
        TileUnitDatas = Resources.FindObjectsOfTypeAll<TileUnitData>().ToList();
        if (TileUnitDatas.Count == 0)
            Debug.Log("Could not find any tile unit data scriptable objects");
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
        // foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        // {
        //     if (Input.GetKeyDown(kcode))
        //         Debug.Log("KeyCode down: " + kcode);


        // }
        for (int i = 0; i < numberKeyCodes.Length; ++i)
        {
            if (Input.GetKeyDown(numberKeyCodes[i]))
            {
                SelectedSpawnUnit = i;
                Debug.Log(SelectedSpawnUnit.ToString() + " Pick " + GetUnitByKeyCode().name);
                break;
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            AllBattleMode();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            RandomSpawn2GroupEqual();
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
                AllBattleMode();
            }
            else if (_randomProcess >= RandomDelay)
            {
                _randomProcess = 0f;
                RandomSpawn();
                _spawnCount++;
            }
        }

    }


    public void AllBattleMode(bool IsBattle = true)
    {
        for (int i = 0; i < ActionUnitManger.Total; i++)
        {
            ActionUnitManger.GetAll()[i].BattleMode = IsBattle;
        }
        MainMenuControl.Instance.ScanAndShow(true);
    }

    public void ClearBoard()
    {
        List<ActionUnit> destroy = new List<ActionUnit>();
        foreach (ActionUnit u in ActionUnitManger.GetAll())
        {
            u.BattleMode = false;
            destroy.Add(u);
        }
        foreach (ActionUnit u in destroy)
        {
            DestroyUnit(u);
        }
    }

    internal void DestroyUnit(ActionUnit actionUnit)
    {
        if (!actionUnit)
            return;
        ActionUnitManger.Instance.Remove(actionUnit);
        Destroy(actionUnit.gameObject);

    }

    bool RandomMode { get; set; } = false;
    float RandomDelay { get; set; } = 0.1f;
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
            if (emptyTile.Monster != null || emptyTile.ActionUnit != null)
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

    public void RandomSpawn2GroupEqual()
    {
        GameTile spawnTile = null;
        int group = 0;
        for (int i = 0; i < MAX_RANDOM_SPAWN; i++)
        {
            group = i % 2 == 0 ? 0 : 1;

            spawnTile = board.GetEmptyTileGroup(group);
            if (spawnTile != null)
            {
                RandomSpawnMonster(spawnTile, group).transform.localRotation = Quaternion.Euler(0, group == 1 ? 180f : 0, 0);
            }
        }
        MainMenuControl.Instance.ScanAndShow(true);
    }

    private void RandomSpawnMonster2(GameTile tile, int group)
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

    private ActionUnit RandomSpawnMonster(GameTile tile, int group)
    {
        ActionUnit monster = actionUnitFactory.Get();
        monster.tileUnitData = GetUnitByKeyCode();
        monster.SpawnOn(tile);
        monster.UnitID = ++ActionUnit.TotalUnit;
        monster.Group = group;
        monster.SpawnCharacter();
        ActionUnitManger.Add(monster);
        return monster;
    }

    private TileUnitData GetUnitByKeyCode()
    {
        Debug.Log(SelectedSpawnUnit.ToString() + "/" + TileUnitDatas.Count);
        if (SelectedSpawnUnit > -1 && SelectedSpawnUnit < TileUnitDatas.Count)
        {
            return TileUnitDatas[SelectedSpawnUnit];
        }
        else
        {
            return TileUnitDatas.PickRandom();
        }
    }

    void HandleTouch2()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile == null) return;
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
    }

    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile == null) return;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 faceTarget = tile.transform.position;
            // faceTarget.y = tile.transform.position.z;
            // faceTarget.z = tile.transform.position.y;
            for (int i = 0; i < ActionUnitManger.Total; i++)
            {
                ActionUnitManger.GetAll()[i].FaceTarget(faceTarget);
            }
            return;
        }
        ActionUnit remove = ActionUnitManger.GetUnit(TouchRay);
        if (remove != null)
        {
            ActionUnitManger.Remove(remove);
            actionUnitFactory.Reclaim(remove);
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
