using System.Collections.Generic;
using UnityEngine;

public class FormationManager
{
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
        foreach (FormationFacade f in Formations)
        {
            FormationFacade valid = f.Check(0);
            if (valid != null)
            {
                Debug.Log("Formation Found match formation");
                validFormations.Add(valid);
            }
        }
        ActiveFormations = validFormations;
        validFormations.ForEach(x =>
        {
            x.ApplyBuff(0);
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
}
