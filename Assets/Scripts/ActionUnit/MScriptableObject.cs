using UnityEngine;

public abstract class MScriptableObject : ScriptableObject
{
    public T Clone<T>() where T : MScriptableObject
    {
        T newIns = InstanceNew<T>();
        string json = JsonUtility.ToJson(this);
        JsonUtility.FromJsonOverwrite(json, newIns);
        return newIns;
    }

    public T InstanceNew<T>() where T : MScriptableObject
    {
        return (T)ScriptableObject.CreateInstance(typeof(T));
    }
}
