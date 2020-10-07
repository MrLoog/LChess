using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionUnitAttack : MonoBehaviour
{
    
    private ActionUnit Host;
    // Start is called before the first frame update
    void Start()
    {
        Host = gameObject.GetComponent<ActionUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        Host.FaceTarget();
    }
}
