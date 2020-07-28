using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarEntity : MonoBehaviour
{
    WarFactory originFactory;

    public WarFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void Recycle()
    {
        originFactory.Reclaim(this);
    }
}
