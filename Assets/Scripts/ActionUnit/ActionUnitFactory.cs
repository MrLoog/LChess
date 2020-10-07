using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ActionUnitFactory : GameObjectFactory
{
    [SerializeField]
    ActionUnit prefab = default;

    public ActionUnit Get()
    {
        ActionUnit instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(ActionUnit unit)
    {
        Debug.Assert(unit.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(unit.gameObject);
    }
}
