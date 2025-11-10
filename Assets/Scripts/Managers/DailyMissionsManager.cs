using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
public class MissionData
{
    [Header("ID")]
    public string missionID;

    [Header("Mission Config")]
    public GameManager.GameModes gameMode;
    public string missionDescription;
    public int missionDifficulty;
    public MissionType missionType;

    [Header("Objectives")]
    public int objectiveAmount;
    public float currentProgress;
    public bool completed = false;

    [Header("Rewards")]
    public RewardType rewardType;
    public int rewardAmount;
    public bool rewardGranted = false;

    public MissionData()
    {
        currentProgress = 0;
        completed = false;
        rewardGranted = false;
    }
}
public class DailyMissionsManager : ManagersManager
{
    private string _spreadSheetURL = "https://docs.google.com/spreadsheets/d/10vebWCUT7AbVgmcj5rOvFOdGKgiyTMoxyDSwXNgbW_U/export?format=csv&gid=0";

    private List<MissionData> _allAvailableMissions = new();
    private List<MissionData> _todayMissions = new();

    [SerializeField] public int dailyMissionsCount = 5;


    public static Action<MissionType, object> OnMissionActionPerformed; //Ésta es la que llaman las acciones, por ejemplo, los toques, pasar niveles, etc
    public Action OnCompleteMission = delegate { };
    public Action OnDailyMissionsReset = delegate { };

    private Dictionary<string, float> _missionProgress = new();
    public static DailyMissionsManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
        StartCoroutine(DownloadAndParseCSV());
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RegenerateDailyMissions();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveAndLoadManager.SetStringValue(
                DateTime.Today.AddDays(-1).ToString("yyyyMMdd"), SaveAndLoadManager.LastDayUpdateName);
            SaveAndLoadManager.Save();

            if (CheckForDayChange())
            {
                RegenerateDailyMissions();
            }
        }
    }
#endif
    private void OnEnable()
    {
        OnMissionActionPerformed += UpdateMissionProgress;
    }
    private void OnDisable()
    {
        OnMissionActionPerformed -= UpdateMissionProgress;
    }
    #region CSV
    private IEnumerator DownloadAndParseCSV()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_spreadSheetURL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                ParseCSV(request.downloadHandler.text);
            else
                Debug.LogError("Error al descargar el archivo CSV: " + request.error);
        }
    }
    private void ParseCSV(string csvContent)
    {
        csvContent = csvContent.Replace("\"", "");
        string[] lines = csvContent.Split('\n');

        _allAvailableMissions.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim()))
                continue;
            string[] values = lines[i].Split(",");

            if (values.Length < 7)
                continue;

            MissionData mission = new MissionData();
            mission.missionID = values[0].Trim();
            mission.gameMode = ParseGameMode(values[1].Trim());
            mission.missionDescription = LanguageManager.Instance.GetLocalizedText(mission.missionID);
            mission.missionType = ParseMissionType(values[2].Trim());
            if (int.TryParse(values[3].Trim(), out int objective))
            {
                mission.objectiveAmount = objective;
            }
            else mission.objectiveAmount = 0;

            if (int.TryParse(values[4].Trim(), out int difficulty))
            {
                mission.missionDifficulty = difficulty;
            }
            else mission.missionDifficulty = 1;
            mission.rewardType = ParseRewardType(values[5].Trim());

            if (int.TryParse(values[6].Trim(), out int reward))
            {
                mission.rewardAmount = reward;
            }
            else mission.rewardAmount = 0;

            _allAvailableMissions.Add(mission);
        }
        _isInitialized = true;
        InitializeDailyMissions();
    }

    private GameManager.GameModes ParseGameMode(string gameModeString)
    {
        return (GameManager.GameModes)Enum.Parse(typeof(GameManager.GameModes), gameModeString, true);
    }
    private MissionType ParseMissionType(string missionTypeString)
    {
        return (MissionType)Enum.Parse(typeof(MissionType), missionTypeString, true);
    }
    private RewardType ParseRewardType(string rewardString)
    {
        return (RewardType)Enum.Parse(typeof(RewardType), rewardString, true);
    }
    #endregion

    #region DAY CHANGE LOGIC
    private bool CheckForDayChange()
    {
        string lastSavedDay = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.LastDayUpdateName);
        string currentDay = DateTime.Today.ToString("yyyyMMdd");

        return string.IsNullOrEmpty(lastSavedDay) || lastSavedDay != currentDay;
    }
    #endregion

    private void RegenerateDailyMissions()
    {
        foreach (var missionToDeleteData in _allAvailableMissions)
        {
            SaveAndLoadManager.DeleteMissionDataByID(missionToDeleteData.missionID);
            SaveAndLoadManager.DeleteKey($"{missionToDeleteData.missionID}_RewardGranted", false, true);
        }
        _missionProgress.Clear();
        _todayMissions.Clear();

        _todayMissions = SelectRandomMissions(dailyMissionsCount);

        SaveAndLoadManager.SetStringValue(DateTime.Today.ToString("yyyyMMdd"), SaveAndLoadManager.LastDayUpdateName);

        foreach (var mission in _todayMissions)
        {
            SaveAndLoadManager.SetDailyMissionProgressByMissionID(mission.missionID, 0,
                DateTime.Today.ToString("yyyyMMdd"), true, true);
        }
        CompletConstantMission();
        SaveAndLoadManager.Save();

        OnDailyMissionsReset?.Invoke();
    }

    #region DAILY CONNECTION MISSION
    public void CompletConstantMission()
    {
        var dailyMission = _todayMissions.FirstOrDefault(m => m.missionType == MissionType.DailyLogin);

        if (!dailyMission.completed)
        {
            dailyMission.completed = true;
            dailyMission.currentProgress = dailyMission.objectiveAmount;
            _missionProgress[dailyMission.missionID] = dailyMission.objectiveAmount;
            SaveAndLoadManager.SetDailyMissionProgressByMissionID(dailyMission.missionID, dailyMission.currentProgress,
                DateTime.Today.ToString("yyyyMMdd"), true, true);
            SaveAndLoadManager.Save();
        }
    }
    #endregion
    private void InitializeDailyMissions()
    {
        LoadSavedMissions();

        if (_todayMissions.Count == 0 && CheckForDayChange())
        {
            RegenerateDailyMissions();
        }
    }

    private void LoadSavedMissions()
    {
        foreach (var missionCopy in _allAvailableMissions)
        {

            var savedData = SaveAndLoadManager.GetDailyMissionProgressDataByID(missionCopy.missionID);

            if (string.IsNullOrEmpty(savedData.lastUpdateDate))
                continue;

            if (savedData.lastUpdateDate != DateTime.Today.ToString("yyyyMMdd"))
            {
                SaveAndLoadManager.DeleteMissionDataByID(missionCopy.missionID, true, true);
                continue;
            }

            bool rewardGranted = SaveAndLoadManager.GetBoolValue($"{missionCopy.missionID}_RewardGranted");

            if (rewardGranted)
                continue;

            MissionData mission = new MissionData
            {
                missionID = missionCopy.missionID,
                gameMode = missionCopy.gameMode,
                missionDescription = missionCopy.missionDescription,
                missionType = missionCopy.missionType,
                objectiveAmount = missionCopy.objectiveAmount,
                missionDifficulty = missionCopy.missionDifficulty,
                rewardType = missionCopy.rewardType,
                rewardAmount = missionCopy.rewardAmount,
                currentProgress = savedData.progress,
                completed = savedData.progress >= missionCopy.objectiveAmount,
                rewardGranted = rewardGranted
            };

            _missionProgress[mission.missionID] = savedData.progress;
            _todayMissions.Add(mission);
        }
        _todayMissions = _todayMissions.OrderByDescending(m => m.missionType == MissionType.DailyLogin).ToList();
    }

    public void CompleteMission(MissionData mission)
    {
        GrantReward(mission.rewardType, mission.rewardAmount);
        mission.rewardGranted = true;

        _missionProgress[mission.missionID] = mission.currentProgress;
        SaveAndLoadManager.SetDailyMissionProgressByMissionID(mission.missionID, mission.currentProgress,
            DateTime.Today.ToString("yyyyMMdd"), true, true);
        SaveAndLoadManager.SetBoolValue(true, $"{mission.missionID}_RewardGranted");
        _todayMissions.Remove(mission);
        OnCompleteMission?.Invoke();
    }

    private void GrantReward(RewardType rewardType, object rewardValue)
    {

        switch (rewardType)
        {
            case RewardType.Coins:
                if (rewardValue is int coins)
                    SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName)
                        + coins, SaveAndLoadManager.CoinsName);
                break;
            case RewardType.Orbs:
                if (rewardValue is int orbs)
                    SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName)
                        + orbs, SaveAndLoadManager.OrbsName);
                break;
            case RewardType.BattlePassXP:
                if (rewardValue is float xP)
                    Debug.Log($"Se ha otorgado {xP} de XP al battlepass");
                break;
            case RewardType.Skin:
                if (rewardValue is string skin)
                    Debug.Log($"{skin}: {rewardValue}");
                break;
        }
    }

    private List<MissionData> SelectRandomMissions(int count)
    {
        _todayMissions.Clear();

        if (_allAvailableMissions.Count == 0)
            return new List<MissionData>();

        var selectedMissions = _allAvailableMissions
        .Where(x => SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedGameMode + x.gameMode) != 0 ||
        x.gameMode == GameManager.GameModes.Null || x.gameMode == GameManager.GameModes.Dunk)
        .GroupBy(m => m.missionType)
        .OrderBy(x => Random.value)
        .Take(count)
        .Select(group => group.OrderBy(x => Random.value).First())
        .Union(_allAvailableMissions.Where(m => m.missionType == MissionType.DailyLogin))
        .Select(selected => new MissionData
        {
            missionID = selected.missionID,
            gameMode = selected.gameMode,
            missionDescription = selected.missionDescription,
            missionType = selected.missionType,
            objectiveAmount = selected.objectiveAmount,
            missionDifficulty = selected.missionDifficulty,
            rewardType = selected.rewardType,
            rewardAmount = selected.rewardAmount
        }).OrderByDescending(m => m.missionType == MissionType.DailyLogin)
        .ToList();
        return selectedMissions;
    }

    private void UpdateMissionProgress(MissionType type, object value)
    {
        if (_todayMissions.Count == 0)
            return;

        float amount = 0;

        if (value is int intValue)
            amount = intValue;
        else if (value is float floatValue)
            amount = floatValue;

        foreach (var mission in _todayMissions)
        {
            if (mission.missionType == type && (mission.gameMode == GameManager.Instance.GetCurrentGameMode ||
                mission.gameMode == GameManager.GameModes.Null) && !mission.completed)
            {
                if (!_missionProgress.ContainsKey(mission.missionID))
                    _missionProgress[mission.missionID] = 0;

                switch (mission.missionType)
                {
                    case MissionType.TouchesRemaining:
                    case MissionType.TimeLimit:
                        if (amount <= mission.objectiveAmount)
                        {
                            mission.currentProgress = mission.objectiveAmount;
                            _missionProgress[mission.missionID] = mission.objectiveAmount;
                            mission.completed = true;
                        }
                        break;
                    case MissionType.LevelsPassed:
                    case MissionType.Touches:
                    case MissionType.CoinsCollected:
                    default:
                        float newProgress = _missionProgress[mission.missionID] + amount;
                        _missionProgress[mission.missionID] = Mathf.Min(newProgress, mission.objectiveAmount);
                        mission.currentProgress = _missionProgress[mission.missionID];

                        if (_missionProgress[mission.missionID] >= mission.objectiveAmount)
                            mission.completed = true;
                        break;
                }
                SaveAndLoadManager.SetDailyMissionProgressByMissionID(mission.missionID, mission.currentProgress,
                    DateTime.Today.ToString("yyyyMMdd"));
            }
        }
        SaveAndLoadManager.Save();
    }

    public override IEnumerator InizializeManagers()
    {
        yield return null;
        while (_isInitialized)
            yield return null;
    }

    public bool GetAllMissionsCompletedStatus()
    {
        return _todayMissions.Any(m => !m.completed);
    }

    public int GetAlMissionsCompletedCount()
    {
        return _todayMissions.Count(m => !m.rewardGranted);
    }

    public List<MissionData> GetTodayMissions => _todayMissions;
}
#region ENUMS
public enum MissionType
{
    Touches,
    TouchesRemaining,
    TimeLimit,
    CoinsCollected,
    LevelsPassed,
    DailyLogin
}

public enum RewardType
{
    Coins,
    Orbs,
    BattlePassXP,
    Skin
}
#endregion