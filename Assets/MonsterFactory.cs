using UnityEngine;

[CreateAssetMenu]
public class MonsterFactory : GameObjectFactory
{
    [SerializeField]
    Monster prefab = default;

    public Monster Get()
    {
        Monster instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(Monster enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }
}
