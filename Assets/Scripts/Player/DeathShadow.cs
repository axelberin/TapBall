using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathShadow : MonoBehaviour
{
    private void Start()
    {
        var sprite = GetComponent<Sprite>();
        //if (sprite != null)
        //    sprite = GameManager.Instance.SetGetPlayer.GetSprite;     //TODO igualar a la skin actual desde el addresable.
    }
    //public void ReturnObject()
    //{
    //    Factory.Instance.ReturnDeathShadow(this);
    //}

    //public void TurnOff(DeathShadow obj)
    //{
    //    obj.gameObject.SetActive(false);
    //}

    //public void TurnOn(DeathShadow obj)
    //{
    //    Factory.Instance.AddDeathShadow(obj);
    //    obj.gameObject.SetActive(true);
    //}
}
