using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSON : MonoBehaviour
{
    public static JSON Instance;

    [SerializeField] string _path = "Assets/Scripts/Json/data/";
    [SerializeField] string _buildPath = "";
    [SerializeField] string _apkPath = "sdcard/Android/obb/";

    [SerializeField] PlayerData _playerData;
    [SerializeField] DunkData _dunkData;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);

        if (Application.platform == RuntimePlatform.Android) _path = _apkPath;
        else if (Application.platform == RuntimePlatform.WindowsPlayer) _path = _buildPath;

        LoadAllData();
    }

    void LoadAllData()
    {
        LoadPlayerData();
        LoadDunkData();
    }

    #region PLAYER INFO
    public void SavePlayerData()
    {
        StreamWriter playerData = File.CreateText(_path + _playerData.fileName + ".json");

        string json = JsonUtility.ToJson(_playerData, true);

        playerData.Write(json);

        playerData.Close();
    }

    public void LoadPlayerData()
    {
        string finalPath = _path + _playerData.fileName + ".json";

        if (!File.Exists(finalPath)) return;

        string json = File.ReadAllText(finalPath);

        _playerData = JsonUtility.FromJson<PlayerData>(json);
    }
    #endregion

    #region DUNK DATA

    public void SaveDunkData()
    {
        StreamWriter dunkData = File.CreateText(_path + _dunkData.fileName + ".json");

        string json = JsonUtility.ToJson(_dunkData, true);

        dunkData.Write(json);

        dunkData.Close();
    }

    public void LoadDunkData()
    {
        string finalPath = _path + _dunkData.fileName + ".json";

        if (!File.Exists(finalPath)) return;

        string json = File.ReadAllText(finalPath);

        _dunkData = JsonUtility.FromJson<DunkData>(json);
    }

    #endregion

    public PlayerData GetPlayerData => _playerData;
    public DunkData GetDunkData => _dunkData;
}
