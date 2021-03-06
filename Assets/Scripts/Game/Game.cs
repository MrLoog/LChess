﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{

    private EventDict _events;
    public EventDict Events
    {
        get
        {
            if (_events == null) _events = new EventDict();
            return _events;
        }
    }

    public const string EVENT_GAME_READY = "GAME_READY";
    public static string TAG_MONSTER = "CubeMonster";
    public static string TAG_BOARD = "Board";
    public static string TAG_BULLET = "Bullet";

    public static Game Instance { get; private set; }
    public int MAX_RANDOM_SPAWN = 10;
    private int _spawnCount = 0;


    public const int USER_GROUP = 0;

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

    internal void NewGame(GameMode.GameModeType type)
    {
        Profile.InitNewGame(type);
        ActiveGameModeCtrl = Profile.GameModeCtrl;
        RoundManager = Profile.InitRoundManager();
        OnGame = true;
        Events.InvokeOnAction(EVENT_GAME_READY);
    }


    [SerializeField]
    ActionUnitFactory actionUnitFactory = default;



    public ActionUnitManger ActionUnitManger = ActionUnitManger.Instance;

    public RoundManager RoundManager;

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
            _onGame = value;
        }
    }

    public ActionUnitEvent OnUnitSelected = new ActionUnitEvent();


    public RoundPlan Plan;

    public GameMode ActiveGameModeCtrl;
    public PlayerProfile Profile;


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
        Resources.LoadAll<Formation>("Formation");
        TileUnitDatas = Resources.FindObjectsOfTypeAll<ActionUnitData>().Where(x => x.Level == 1).Cast<TileUnitData>().ToList();
        if (TileUnitDatas.Count == 0)
            Debug.Log("Could not find any tile unit data scriptable objects");
        List<Formation> lst = Resources.FindObjectsOfTypeAll<Formation>().Where(x => x.PrevLevel == null).ToList();
        foreach (Formation f in lst)
        {
            FormationManager.Instance.Formations.Add(FormationFacade.CreateFromFormation(f));
        }
        // FormationManager.Instance.Formations = Resources.FindObjectsOfTypeAll<Formation>().Where(x => x.PrevLevel == null).Select(x => FormationFacade.CreateFromFormation(x)).ToList();

        RegisterEvent();
        Profile = new PlayerProfile();
    }

    private void Start()
    {
        FormationManager.Instance.Init();
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
        // KeyCodePreUnitSpawn();
        ToggerBoardGridShow();
        PressHotkeyAllBattle();
        // RandomSpawnMode();
        RoundUpdate();

        if (InputUtils.Mouse1Press())
        {
            HandleTouch();
        }

    }

    private void RoundUpdate()
    {
        if (RoundManager?.Round != null)
        {
            if (RoundManager.Round.ValidEndGame(true))
            {
                // RoundManager.EndRound();
            }
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
            ActionUnit grab = GrabUnit.GetComponent<ActionUnit>();
            GameTile tile = board.GetTile(InputUtils.GetTouchRayMouse());
            if (_isGrab)
            {
                if (tile != null)
                {
                    // GrabUnit.transform.localPosition = new Vector3(tile.transform.localPosition.x,
                    // 1,
                    // tile.transform.localPosition.z);

                    if (Physics.Raycast(InputUtils.GetTouchRayMouse(), out RaycastHit hit))
                    {
                        GrabUnit.transform.localPosition = new Vector3(hit.point.x,
                        1,
                        hit.point.z);
                    }


                }
            }
            if (InputUtils.Mouse1Free())
            {
                if (_isGrab)
                {
                    if (tile == null)
                    {
                        tile = board.GetTileByCordinate(GrabUnit.transform.localPosition);
                    }
                    bool newPos = true;
                    _isGrab = false;
                    grab.EnterGrabMode(false);
                    GrabUnit.transform.localPosition = new Vector3(tile.transform.localPosition.x,
                    1,
                    tile.transform.localPosition.z);
                    GrabUnit.transform.localPosition += Vector3.down;
                    if (tile.ActionUnit != null)
                    {
                        if (tile.ActionUnit.UnitID != grab.UnitID)
                        {
                            newPos = false;
                            MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_OTHER_UNIT, 1f);
                        }
                        else
                        {
                            //no change 
                        }
                    }
                    else
                    {
                        // if (!tile.PrepareTile)
                        // {
                        //     int unitOnBoard = ActionUnitManger.Instance.GetAll().Where(x => x.enabled && x.Group == 0 && !x.TilePos.PrepareTile && x.UnitID != grab.UnitID).Count();
                        //     if (unitOnBoard + 1 > Profile.GameModeCtrl.GetMaxSpawn())
                        //     {
                        //         newPos = false;
                        //         MainMenuControl.Instance.ShowUserMessage(UserMessageManager.MES_LIMIT_UNIT, 1f);
                        //     }
                        // }

                    }
                    if (newPos)
                    {
                        //change reference
                        grab.TilePos.ActionUnit = null;
                        grab.TilePos = tile;
                        tile.ActionUnit = grab;
                        if (!grab.TilePos.PrepareTile)
                        {
                            UnitLevelManager.Instance.ValidLevelUpUnit(grab);
                        }
                        Debug.Log("Formation new pos");
                        FormationManager.Instance.ApplyFormation();
                    }
                    else
                    {
                        //exists unit so return grab to origin position
                        GrabUnit.transform.localPosition = new Vector3(grab.TilePos.transform.localPosition.x,
                        GrabUnit.transform.localPosition.y,
                        grab.TilePos.transform.localPosition.z);
                    }

                }
                GrabUnit = null;
                _accumGrabTime = 0;
            }
            else if (!_isGrab)
            {
                _accumGrabTime += Time.deltaTime;
                if (_accumGrabTime > 0.3f)
                {
                    grab.EnterGrabMode(true);
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
        actionUnit.TilePos.ActionUnit = null;
        actionUnit.TilePos = null;
        ActionUnitManger.Instance.Remove(actionUnit);
        Destroy(actionUnit.gameObject);

    }

    bool RandomMode { get; set; } = false;
    float RandomDelay { get; set; } = 0.1f;
    float _randomProcess = 0f;
    public float MaxTimeRandom { get; set; } = 30f;


    public float _cumUpTimeRandom { get; set; } = 0f;

    internal void SpawnPrepareUnit(TileUnitData tileUnitData)
    {
        GameTile validTile = board.GetPrepareTile();
        if (validTile != null)
        {
            SpawnMonster(validTile, 0, tileUnitData);
        }
    }

    public ActionUnit SpawnMonster(GameTile tile, int group, TileUnitData typeUnit)
    {
        ActionUnit monster = actionUnitFactory.Get();
        monster.tileUnitData = typeUnit;
        monster.SpawnOn(tile);
        monster.UnitID = ++ActionUnit.TotalUnit;
        monster.Group = group;
        monster.SpawnCharacter();
        monster.transform.localRotation = Quaternion.Euler(0, group == 1 ? 180f : 0, 0);
        ActionUnitManger.Add(monster);
        if (!tile.PrepareTile)
        {
            FormationManager.Instance.ApplyFormation();
        }
        return monster;
    }

    public void MirrorSpawn()
    {
        GameTile spawnTile = null;
        int group = 1;
        List<ActionUnit> units = ActionUnitManger.Instance.GetAll().Where(x => !x.TilePos.PrepareTile).ToList();
        int maxLevel = -1;
        int existsMaxLevel = 1;
        maxLevel = 1 + (Game.Instance.RoundManager.Round.Level <= 10 ? 0 : +Mathf.CeilToInt((Game.Instance.RoundManager.Round.Level - 10f) / 5f));
        TileUnitData dataSpawn = null;
        int spawnCount = Profile.GameModeCtrl.GetMaxSpawn();
        foreach (ActionUnit unit in units)
        {
            spawnTile = board.GetRandomEmptyTileGroup(group);
            if (spawnTile != null)
            {
                dataSpawn = unit.tileUnitData;
                int level = ((ActionUnitData)unit.tileUnitData).Level;
                if (maxLevel != -1 &&
                level > maxLevel)
                {
                    existsMaxLevel = units.Select(x => x.tileUnitData).Where(x => ((ActionUnitData)x).Level <= maxLevel).OrderByDescending(x => ((ActionUnitData)x).Level).Select(x => ((ActionUnitData)x).Level).FirstOrDefault();
                    dataSpawn = RandomFromList(units.Select(x => x.tileUnitData).Cast<ActionUnitData>().ToList(), existsMaxLevel);

                }
                SpawnMonster(spawnTile, group, dataSpawn);
                spawnCount--;
            }
        }
        if (spawnCount > 0)
        {
            for (int i = spawnCount; i > 0; i--)
            {
                spawnTile = board.GetRandomEmptyTileGroup(group);
                if (spawnTile != null)
                {
                    dataSpawn = RandomFromList(units.Select(x => x.tileUnitData).Cast<ActionUnitData>().ToList());
                    SpawnMonster(spawnTile, group, dataSpawn);
                }
            }
        }
        MainMenuControl.Instance.ScanAndShow(true);
    }

    public TileUnitData RandomFromList(List<ActionUnitData> source, int levelLimit = 0)
    {
        List<ActionUnitData> avaiable = source.Where(x => levelLimit == -1 || ((ActionUnitData)x).Level == levelLimit).ToList();
        if (avaiable.Count == 0)
        {
            avaiable = TileUnitDatas.Where(x => levelLimit == 0 || ((ActionUnitData)x).Level == levelLimit).Cast<ActionUnitData>().ToList();
        }
        return avaiable.PickRandom();
    }

    public void StartGame()
    {
        // if (RoundMode)
        //     MirrorSpawn();
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
        // else if (!RoundMode)
        // {
        // RandomSpawnMonster(tile, InputUtils.LeftShiftPress() ? 1 : 0);
        // }
    }

    private void RemoveUnit(ActionUnit remove)
    {
        ActionUnitManger.Remove(remove);
        actionUnitFactory.Reclaim(remove);
    }

    public void FocusUnit(ActionUnit focus)
    {
        InGameMenuControl.Instance.FocusUnit = focus;
        if (focus.Group == USER_GROUP)
        {
            if (RoundManager.Round.CurPhase == Round.RoundPhase.NotStart)
            {
                //chỉ grab khi ở phase not start
                GrabUnit = focus.gameObject;
            }
        }
    }


}
