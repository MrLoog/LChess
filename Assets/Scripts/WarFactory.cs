using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{
    [SerializeField]
    MonsterAttack prefab = default;

    public MonsterAttack AnAttack => Get(prefab);

    T Get<T>(T prefab) where T : WarEntity
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(WarEntity entity)
    {
        Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(entity.gameObject);
    }
}
