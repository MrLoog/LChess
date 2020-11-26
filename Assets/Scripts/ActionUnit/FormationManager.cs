using System;
using System.Collections.Generic;
using UnityEngine;

public class FormationManager
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

    public const string EVENT_FORMATION_APPLY = "FORMATION_APPLY";
    public const string EVENT_CHECK_BEFORE = "BEFORE_CHECK";

    public List<int> GroupChecks;
    private static FormationManager _instance = new FormationManager();
    public static FormationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new FormationManager();
            }
            return _instance;
        }
    }
    private FormationManager()
    {

    }

    List<FormationFacade> _formations;

    public List<FormationFacade> Formations
    {
        get
        {
            if (_formations == null)
            {
                _formations = new List<FormationFacade>();
            }
            return _formations;
        }
        set
        {
            _formations = value;
        }
    }

    List<FormationFacade> _activeFormations;
    public List<FormationFacade> ActiveFormations
    {
        get
        {
            if (_activeFormations == null)
            {
                _activeFormations = new List<FormationFacade>();
            }
            return _activeFormations;
        }
        set
        {
            _activeFormations = value;
        }
    }

    internal void ApplyFormation()
    {
        Debug.Log("Formation Check Apply");
        ActiveFormations.ForEach(x =>
        {
            x.RemoveBuff();
        });
        List<FormationFacade> validFormations = new List<FormationFacade>();
        GroupChecks = ActionUnitManger.Instance.Groups;
        Events.InvokeOnAction(EVENT_CHECK_BEFORE);
        foreach (int group in GroupChecks)
        {
            foreach (FormationFacade f in Formations)
            {
                FormationFacade valid = FormationFacade.CreateFromFormation(f.Formation);
                valid = valid.Check(group);
                if (valid != null)
                {
                    valid.GroupOwner = group;
                    Debug.Log("Formation Found match formation " + group);
                    validFormations.Add(valid);
                }
            }
        }

        ActiveFormations = validFormations;
        Events.InvokeOnAction(EVENT_FORMATION_APPLY);
        ActiveFormations.ForEach(x =>
        {
            x.ApplyBuff();
        });

        // bool existsRemove = false;
        // bool existsNew = false;
        // ActiveFormations.Where(x => !validFormations.Contains(x)).ToList().ForEach(x =>
        // {
        //     existsRemove = true;
        //     x.RemoveBuff();
        // });
        // if (existsRemove) ActiveFormations.RemoveAll(x => !validFormations.Contains(x));
        // validFormations.Where(x => !ActiveFormations.Contains(x)).ToList().ForEach(x =>
        // {
        //     existsNew = true;
        //     x.ApplyBuff(0);
        // });
        // if (existsNew) ActiveFormations.AddRange(validFormations.Where(x => !ActiveFormations.Contains(x)));
    }

    internal void Init()
    {
        ActionUnitManger.Instance.Events.RegisterListener(ActionUnitManger.EVENT_UNIT_SPAWN).AddListener(ApplyFormation);
        ActionUnitManger.Instance.Events.RegisterListener(ActionUnitManger.EVENT_UNIT_DEATH).AddListener(ApplyFormation);
    }
}
