using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    public static string TAG_MONSTER = "CubeMonster";
    public static string TAG_BOARD = "Board";
    public static string TAG_BULLET = "Bullet";

    public static Game Instance { get; private set; }
    public int MAX_RANDOM_SPAWN = 10;
    private int _spawnCount = 0;

    [SerializeField]
    public bool _roundMode;

    public bool RoundMode
    {
        get
        {
            return _roundMode;
        }
        set
        {
            ClearBoard();
            RoundManager.Instance.Reset();
            _roundMode = value;
        }
    }


    public Material MatGroup1;
    public Material MatGroup2;

    internal Material GetMatForGroup(int group)
    {
        return group == 0 ? MatGroup1 : MatGroup2;
    }



    [SerializeField]
    public Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField]
    public Board board = default;

    [SerializeField]
    MonsterFactory monsterFactory = default;

    [SerializeField]
    ActionUnitFactory actionUnitFactory = default;


    [SerializeField]
    WarFactory warFactory = default;

    public MonsterManager monsterManager = new MonsterManager();
    public ActionUnitManger ActionUnitManger = ActionUnitManger.Instance;

    public RoundManager RoundManager;
    public GameBullet gameBullet = new GameBullet();

    static Game instance;
    public List<TileUnitData> TileUnitDatas { get; set; }

    public TileUnitData PickupUnit;
    public GameObject PickupModel;

    public GameObject GrabUnit;
    public float _accumGrabTime;
    public bool _isGrab;

    private bool _onGame;
    public bool OnGame
    {
        get
        {
            return _onGame;
        }
        set
        {
            MainMenuControl.Instance.ShowGameState(value);
            _onGame = value;
        }
    }

    public ActionUnitEvent OnUnitSelected = new ActionUnitEvent();

    public static MonsterAttack SpawnMonsterAttack()
    {
        MonsterAttack attack = instance.warFactory.AnAttack;
        return attack;
    }

    public RoundPlan Plan;

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
        TileUnitDatas = Resources.FindObjectsOfTypeAll<ActionUnitData>().Where(x => x.Level == 1).Cast<TileUnitData>().ToList();
        if (TileUnitDatas.Count == 0)
            Debug.Log("Could not find any tile unit data scriptable objects");


        RegisterEvent();
    }

    private void Start()
    {
        RoundManager = RoundManager.Instance;
    }

    private void RegisterEvent()
    {
        OnUnitSelected.AddListener(FocusUnit);
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

    // Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    void Update()
    {
        // foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        // {
        //     if (Input.GetKeyDown(kcode))
        //         Debug.Log("KeyCode down: " + kcode);


        // }
        DetectGrabUnit();
        UpdatePickupUnit();
        KeyCodePreUnitSpawn();
        ToggerBoardGridShow();
        PressHotkeyAllBattle();
        PressHotkeySpawnGroupEqual();
        RandomSpawnMode();
        RoundUpdate();

        if (InputUtils.Mouse1Press())
        {
            HandleTouch();
        }

    }

    private void RoundUpdate()
    {
        if (RoundManager.Round != null)
        {
            if (RoundManager.Round.ValidEndGame(true))
            {
                RoundManager.EndRound();
            }
        }
    }

    private void RandomSpawnMode()
    {
        if (InputUtils.HotkeyRandomMode())
        {
            RandomMode = !RandomMode;
            Debug.Log(string.Format("Random Mode {0}", RandomMode));
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

    private void PressHotkeySpawnGroupEqual()
    {
        if (InputUtils.HotkeySpawnGroupEqual())
        {
            RandomSpawn2GroupEqual();
        }
    }

    private void PressHotkeyAllBattle()
    {
        if (InputUtils.HotkeyAllBattle())
        {
            AllBattleMode();
        }
    }

    private void ToggerBoardGridShow()
    {
        if (InputUtils.HotkeyBoardGrid())
        {
            board.ShowGrid = !board.ShowGrid;
        }
    }

    private void KeyCodePreUnitSpawn()
    {
        int keyPress = InputUtils.GetNumberKeyPress();
        if (keyPress > -1)
        {
            SelectedSpawnUnit = keyPress;
            Debug.Log(SelectedSpawnUnit.ToString() + " Pick " + GetUnitByKeyCode().name);
        }

    }

    private void DetectGrabUnit()
    {
        if (GrabUnit != null)
        {
            ActionUnit actionUnit = GrabUnit.GetComponent<ActionUnit>();
            if (_isGrab)
            {
                GameTile tile = board.GetTile(InputUtils.GetTouchRayMouse());
                if (tile != null)
                {
                    GrabUnit.transform.localPosition = new Vector3(tile.transform.localPosition.x,
                    1,
                    tile.transform.localPosition.z);
                    Debug.Assert(actionUnit.TilePos != null, "Tile Pos should always not null");
                    actionUnit.TilePos.ActionUnit = null;
                    actionUnit.TilePos = tile;
                    tile.ActionUnit = actionUnit;
                }
            }
            if (InputUtils.Mouse1Free())
            {
                if (_isGrab)
                {
                    _isGrab = false;
                    GrabUnit.transform.localPosition += Vector3.down;
                    if (RoundMode)
                    {
                        if (!actionUnit.TilePos.PrepareTile)
                        {
                            UnitLevelManager.Instance.ValidLevelUpUnit(actionUnit);
                        }
                    }
                }
                GrabUnit = null;
                _accumGrabTime = 0;
            }
            else if (!_isGrab)
            {
                _accumGrabTime += Time.deltaTime;
                if (_accumGrabTime > 1f)
                {
                    Debug.Log("Grab" + _accumGrabTime);
                    GrabUnit.transform.localPosition += Vector3.up;
                    _isGrab = true;
                }
            }
        }
    }

    private void UpdatePickupUnit()
    {
        if (PickupUnit != null &&
            (InputUtils.LeftMousePress()
            || (InputUtils.Mouse1Press() && !InputUtils.LeftShiftPress())
            )
        )
        {
            PickupUnit = null;
            if (PickupModel != null)
            {
                Destroy(PickupModel);
            }
        }
        if (PickupUnit != null)
        {
            ShowUnitPickup();
        }
    }

    private void ShowUnitPickup()
    {
        if (PickupUnit == null) return;
        GameTile tile = board.GetTile(InputUtils.GetTouchRayMouse());
        if (tile == null) return;

        if (PickupModel == null)
        {
            PickupModel = Instantiate(PickupUnit.characterPrefab);
            for (int i = 0; i < TileUnitDatas.Count; i++)
            {
                if (TileUnitDatas[i].unitName.Equals(PickupUnit.unitName))
                {
                    SelectedSpawnUnit = i;
                    break;
                }
            }
        }
        PickupModel.transform.localPosition = tile.transform.localPosition;
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
            if (RoundMode && group == 1) continue; // mirror mode chỉ spawn group 0

            spawnTile = board.GetEmptyTileGroup(group);
            if (spawnTile != null)
            {
                RandomSpawnMonster(spawnTile, group);
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
        if (RoundMode) group = 0;
        return SpawnMonster(tile, group, GetUnitByKeyCode());
        // ActionUnit monster = actionUnitFactory.Get();
        // monster.tileUnitData = GetUnitByKeyCode();
        // monster.SpawnOn(tile);
        // monster.UnitID = ++ActionUnit.TotalUnit;
        // monster.Group = group;
        // monster.SpawnCharacter();
        // ActionUnitManger.Add(monster);
        // return monster;
    }

    internal void SpawnPrepareUnit(TileUnitData tileUnitData)
    {
        GameTile validTile = board.GetPrepareTile();
        if (validTile != null)
        {
            SpawnMonster(validTile, 0, tileUnitData);
        }
    }

    private ActionUnit SpawnMonster(GameTile tile, int group, TileUnitData typeUnit, TileUnitData curData = null)
    {
        ActionUnit monster = actionUnitFactory.Get();
        monster.tileUnitData = typeUnit;
        if (curData != null)
        {
            ActionUnitData data = (ActionUnitData)ScriptableObject.CreateInstance(typeof(ActionUnitData));
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(curData), data);
            monster.CurrentStatus = data;
        }
        monster.SpawnOn(tile);
        monster.UnitID = ++ActionUnit.TotalUnit;
        monster.Group = group;
        monster.SpawnCharacter();
        monster.transform.localRotation = Quaternion.Euler(0, group == 1 ? 180f : 0, 0);
        ActionUnitManger.Add(monster);
        return monster;
    }

    public void MirrorSpawn()
    {
        GameTile spawnTile = null;
        int group = 1;
        List<ActionUnit> units = ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile).ToList();
        foreach (ActionUnit unit in units)
        {
            spawnTile = board.GetRandomEmptyTileGroup(group);
            if (spawnTile != null)
            {
                SpawnMonster(spawnTile, group, unit.tileUnitData, unit.CurrentStatus);
            }
        }
        MainMenuControl.Instance.ScanAndShow(true);
    }

    public void StartGame()
    {
        // if (RoundMode)
        //     MirrorSpawn();
        OnGame = true;
        RoundManager.StartNewRound(0, ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile).ToList());
        // AllBattleMode();
    }

    public void EndGame()
    {
        AllBattleMode(false);
        ClearBoard();
        OnGame = false;
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
        GameTile tile = board.GetTile(InputUtils.GetTouchRayMouse());
        if (tile == null) return;
        Monster remove = monsterManager.GetMonster(InputUtils.GetTouchRayMouse());
        if (remove != null)
        {
            monsterManager.Remove(remove);
            monsterFactory.Reclaim(remove);
        }
        else
        {
            RandomSpawnMonster(tile, InputUtils.LeftShiftPress() ? 1 : 0);
        }
    }

    void HandleTouch()
    {
        GameTile tile = board.GetTile(InputUtils.GetTouchRayMouse());
        if (tile == null) return;
        if (InputUtils.LeftControlPress())
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
        ActionUnit focus = ActionUnitManger.GetUnit(InputUtils.GetTouchRayMouse());
        if (focus != null)
        {
            // RemoveUnit(focus);
            OnUnitSelected.Invoke((ActionUnit)focus);
        }
        else
        {
            RandomSpawnMonster(tile, InputUtils.LeftShiftPress() ? 1 : 0);
        }
    }

    private void RemoveUnit(ActionUnit remove)
    {
        ActionUnitManger.Remove(remove);
        actionUnitFactory.Reclaim(remove);
    }

    public void FocusUnit(ActionUnit focus)
    {
        InGameMenuControl.Instance.FocusUnit = focus;
        GrabUnit = focus.gameObject;
    }


}
