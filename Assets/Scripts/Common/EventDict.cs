using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EventDict
{
    public Dictionary<string, UnityEvent> ActionDict;


    public EventDict()
    {
        ActionDict = new Dictionary<string, UnityEvent>();
    }

    public UnityEvent RegisterListener(string key)
    {
        if (!ActionDict.Keys.ToList().Contains(key))
        {
            ActionDict.Add(key, new UnityEvent());
        }
        return ActionDict[key];
    }

    public UnityEvent OverrideListener(string key, UnityEvent e)
    {
        if (!ActionDict.Keys.ToList().Contains(key))
        {
            ActionDict.Add(key, e);
            return null;
        }
        else
        {
            UnityEvent oldEvent = ActionDict[key];
            ActionDict[key] = e;
            return oldEvent;
        }
    }

    public bool RemoveListener(string key)
    {
        return
            ActionDict.Remove(key);
    }

    public void InvokeOnAction(string action)
    {
        if (ActionDict.Keys.ToList().Contains(action))
        {
            ActionDict[action].Invoke();
        }
    }
}
