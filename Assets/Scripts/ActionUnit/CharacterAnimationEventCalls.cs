using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAnimationEventCalls : MonoBehaviour
{
    public UnityEvent OnAttack { get; set; } = new UnityEvent();
    public Dictionary<string, UnityEvent> ActionDict = new Dictionary<string, UnityEvent>();

    public const string K_ACTION_ATTACK = "ATTACK";
    public const string K_ACTION_ATTACK_END = "END_ATTACK";
    public const string K_ACTION_DIE = "DIE";
    public const string K_STATE_ATTACK_IN = "STATE_ATTACK_IN";


    void InvokeOnAttack()
    {
        OnAttack.Invoke();
    }
    public UnityEvent RegisterListener(string key)
    {
        if (!ActionDict.Keys.ToList().Contains(key))
        {
            ActionDict.Add(key, new UnityEvent());
        }
        return ActionDict[key];
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
