using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Factory : MonoBehaviour
{
    public static Factory Instance;

    [SerializeField] int _deathShadowAmmount;
    [SerializeField] DeathShadow _deathShadowPrefab;
    public ObjectPool<DeathShadow> deathShadowPool;

    List<DeathShadow> _deathShadowList = new List<DeathShadow>();

    private void Awake()
    {
        if (!Instance) 
            Instance = this;
    }

    void Start()
    {
        //if (_deathShadowPrefab) 
        //    deathShadowPool = new ObjectPool<DeathShadow>(DeathShadowCreator, _deathShadowPrefab.TurnOn, _deathShadowPrefab.TurnOff, _deathShadowAmmount);
    }

    public void AddDeathShadow(DeathShadow deathShadow)
    {
        //_deathShadowList.Add(deathShadow);
        //if (_deathShadowList.Count > 50) 
        //    _deathShadowList.FirstOrDefault()?.ReturnObject();
    }

    DeathShadow DeathShadowCreator()
    {
        return Instantiate(_deathShadowPrefab);
    }

    public void ReturnDeathShadow(DeathShadow deathShadow)
    {
        _deathShadowList.Remove(deathShadow);
        deathShadowPool.ReturnObject(deathShadow);
    }
}
