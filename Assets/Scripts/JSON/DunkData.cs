using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DunkData
{
    public string fileName = "DunkInfo";
    public List<int> S_DunkBest = new List<int>();
    public List<int> S_DunkLevels = new List<int>();
    public List<bool> S_DunkWithoutDeath = new List<bool>();
}
