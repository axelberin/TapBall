using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreCanvas : ACanvas
{
    public static StoreCanvas Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
