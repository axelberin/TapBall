using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool<T>
{
    Func<T> _factoryMethod;

    List<T> _currentObjectCount;

    Action<T> _turnOnCallback;
    Action<T> _turnOffCallback;

    public ObjectPool(Func<T> factoryMethod, Action<T> turnOn, Action<T> turnOff, int initialObjectCount)
    {
        _factoryMethod = factoryMethod;
        _turnOnCallback = turnOn;
        _turnOffCallback = turnOff;
        _currentObjectCount = new List<T>();

        for (int i = 0; i < initialObjectCount; i++)
        {
            var obj = _factoryMethod();
            _turnOffCallback(obj);

            _currentObjectCount.Add(obj);
        }
    }

    public T GetSingleObject()
    {
        var result = default(T);

        if (_currentObjectCount.Count > 0)
        {

            result = _currentObjectCount[0];
            _currentObjectCount.RemoveAt(0);
        }
        else
        {
            result = _factoryMethod();
        }

        _turnOnCallback(result);
        return result;
    }

    public T GetAllObjects()
    {
        var result = default(T);

        if (_currentObjectCount.Count > 0)
        {
            result = _currentObjectCount[0];
            _currentObjectCount.RemoveAt(0);
        }

        _turnOnCallback(result);
        return result;
    }

    public void ReturnObject(T obj)
    {
        _turnOffCallback(obj);
        _currentObjectCount.Add(obj);
    }
}
