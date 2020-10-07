using UnityEngine;
using System.Collections;

public class FindObjScript 
{

    public static T GetObjScriptFromCollider<T>(Collider collider)
    {
        return collider.transform.parent.GetComponent<T>();
    }
}
