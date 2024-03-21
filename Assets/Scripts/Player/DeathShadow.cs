using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathShadow : MonoBehaviour
{
    public void ReturnObject()
    {
        Factory.Instance.ReturnDeathShadow(this);
    }

    public void TurnOff(DeathShadow obj)
    {
        obj.gameObject.SetActive(false);
    }

    public void TurnOn(DeathShadow obj)
    {
        Factory.Instance.AddDeathShadow(obj);
        obj.gameObject.SetActive(true);
    }
}
