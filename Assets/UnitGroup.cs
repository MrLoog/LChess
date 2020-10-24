using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGroup : MonoBehaviour
{
    public GameObject RootElem;
    public int Group;
    public float speed = 1f;

    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        Group = RootElem.GetComponent<ActionUnit>().Group;
        gameObject.transform.GetComponent<Renderer>().material = Game.Instance.GetMatForGroup(Group);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0,
         0,
        360 * Time.deltaTime / speed
        ));
    }
}
