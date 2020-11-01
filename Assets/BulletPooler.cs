using System.Collections.Generic;
using UnityEngine;

public class BulletPooler : MonoBehaviour
{
    public static BulletPooler Instance;
    public List<MonsterAttack> pooledObjects;
    public GameObject objectToPool;
    public MonsterAttack _currentAvaiable;
    public MonsterAttack _lastAvaiable;
    public int DefaultInit { get; set; } = 50;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        MonsterAttack obj = InstanceMonsterAttack();
        _currentAvaiable = obj;
        for (int i = 1; i < DefaultInit; i++)
        {
            obj = InstanceMonsterAttack();
            _currentAvaiable.NextAvaiable = obj;
            _currentAvaiable = obj;
        }
        _currentAvaiable = pooledObjects[0];
        _lastAvaiable = pooledObjects[pooledObjects.Count - 1];
    }

    private MonsterAttack InstanceMonsterAttack()
    {
        MonsterAttack obj = Instantiate(objectToPool).GetComponent<MonsterAttack>();
        obj.gameObject.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetPooledObject()
    {
        GameObject obj = _currentAvaiable.gameObject;
        _currentAvaiable = _currentAvaiable.NextAvaiable;
        if (_currentAvaiable.NextAvaiable == null)
        {
            MonsterAttack newAttack = InstanceMonsterAttack();
            _currentAvaiable.NextAvaiable = newAttack;
            _lastAvaiable = newAttack;
            Debug.Log("Extend bullet " + pooledObjects.Count);
        }
        obj.SetActive(true);
        return obj;
    }

    public void RePooledObject(GameObject obj)
    {
        MonsterAttack attack = obj.GetComponent<MonsterAttack>();
        if (attack == null) return;
        obj.SetActive(false);
        _lastAvaiable.NextAvaiable = attack;
        attack.NextAvaiable = null;
        _lastAvaiable = attack;

    }
}
