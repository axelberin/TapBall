using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObstaclesManager : MonoBehaviour
{
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
            player?.Death();
    }
}
