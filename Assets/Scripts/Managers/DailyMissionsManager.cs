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
    public string missionName;
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

    private const string missions_ID_KEY = "DailyMissionsIDs";
    private const string missions_PROGRESS_KEY = "DailyMissionsProgress";

    [SerializeField] public int dailyMissionsCount = 5;


    public static Action<MissionType, object> OnMissionActionPerformed; //Ésta es la que llaman las acciones, por ejemplo, los toques, pasar niveles, etc
    public Action OnCompleteMission = delegate { };
    public Action OnDailyMissionsReset = delegate { };

    private Dictionary<string, float> _missionProgress = new();
    public static DailyMissionsManager Instance { get; private set; }

    private MissionData _constantMission;

    // private string _currentDay;
    // private const string LAST_DAY_KEY = "LastMissionDay";
    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

        CreateConnectionMission();
        StartCoroutine(DownloadAndParseCSV());
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RegenerateDailyMissions();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            string fakeYesterDay = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
            SaveAndLoadManager.SetStringValue(fakeYesterDay, SaveAndLoadManager.LastDayUpdateName);
            SaveAndLoadManager.Save();

            if(CheckForDayChange())
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

            if (values.Length < 9)
                continue;

            MissionData mission = new MissionData();
            mission.missionID = values[0].Trim();
            mission.gameMode = ParseGameMode(values[1].Trim());
            mission.missionName = values[2].Trim();
            mission.missionDescription = values[3].Trim();
            mission.missionType = ParseMissionType(values[4].Trim());
            if (int.TryParse(values[5].Trim(), out int objective))
            {
                mission.objectiveAmount = objective;
            }
            else mission.objectiveAmount = 0;

            if (int.TryParse(values[6].Trim(), out int difficulty))
            {
                mission.missionDifficulty = difficulty;
            }
            else mission.missionDifficulty = 1;
            mission.rewardType = ParseRewardType(values[7].Trim());

            if (int.TryParse(values[8].Trim(), out int reward))
            {
                mission.rewardAmount = reward;
            }
            else mission.rewardAmount = 0;

            _allAvailableMissions.Add(mission);
        }
        _isInitialized = true;

        if (CheckForDayChange())
        {
            RegenerateDailyMissions();
        }
        else
        {
            InitializeDailyMissions();
        }
        SaveAndLoadManager.SetStringValue(SaveAndLoadManager.LastDayUpdateName, DateTime.Today.ToString("yyyyMMdd"));
        SaveAndLoadManager.Save();
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
    //private void CreateTestMissions()
    //{
    //    _allAvailableMissions.Clear();

    //    _allAvailableMissions.Add(new MissionData
    //    {
    //        missionID = "TEST001",
    //        gameMode = GameManager.GameModes.Dunk,
    //        missionName = "Tapping genius",
    //        missionDescription = "Tap 10 times",
    //        missionType = MissionType.Touches,
    //        objectiveAmount = 10,
    //        missionDifficulty = 2,
    //        rewardType = RewardType.Coins,
    //        rewardAmount = 3
    //    });

    //    _allAvailableMissions.Add(new MissionData
    //    {
    //        missionID = "TEST002",
    //        gameMode = GameManager.GameModes.Dunk,
    //        missionName = "Level Madness",
    //        missionDescription = "Finish 3 levels",
    //        missionType = MissionType.LevelsPassed,
    //        objectiveAmount = 3,
    //        missionDifficulty = 1,
    //        rewardType = RewardType.Coins,
    //        rewardAmount = 2
    //    });

    //    _allAvailableMissions.Add(new MissionData
    //    {
    //        missionID = "TEST004",
    //        gameMode = GameManager.GameModes.Time,
    //        missionName = "Flash",
    //        missionDescription = "Complete a Timer level under 8 seconds",
    //        missionType = MissionType.TimeLimit,
    //        objectiveAmount = 8,
    //        missionDifficulty = 3,
    //        rewardType = RewardType.Coins,
    //        rewardAmount = 5
    //    });

    //    _allAvailableMissions.Add(new MissionData
    //    {
    //        missionID = "TEST005",
    //        gameMode = GameManager.GameModes.OneTouch,
    //        missionName = "Close call",
    //        missionDescription = "Win a OneTouch mode level with 5 or less touches left",
    //        missionType = MissionType.TouchesRemaining,
    //        objectiveAmount = 5,
    //        missionDifficulty = 2,
    //        rewardType = RewardType.Coins,
    //        rewardAmount = 4
    //    });
    //}

    #region DAY CHANGE LOGIC
    private bool CheckForDayChange()
    {
        //Hacerlo string también no int
        string lastSavedDay = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.LastDayUpdateName);
        string currentDay = DateTime.Today.ToString("yyyyMMdd");

        return lastSavedDay != currentDay;
    }
    #endregion

    private void RegenerateDailyMissions()
    {
        _missionProgress.Clear();
        _todayMissions.Clear();

        _todayMissions = SelectRandomMissions(dailyMissionsCount);

        AddConstantMissionToTodayMissions();


        OnDailyMissionsReset?.Invoke();
    }

    #region DAILY CONNECTION MISSION
    private void CreateConnectionMission()
    {
        _constantMission = new MissionData
        {
            missionID = "DAILY_LOGIN",
            gameMode = GameManager.GameModes.Dunk,
            missionName = "Daily Login",
            missionDescription = "Connect daily",
            missionType = MissionType.DailyLogin,
            objectiveAmount = 1,
            missionDifficulty = 1,
            rewardType = RewardType.Coins,
            rewardAmount = 5,
            completed = false,
            rewardGranted = false
        };

    }

    public void CompletConstantMission()
    {
        if (!_constantMission.completed && !_constantMission.rewardGranted)
        {
            _constantMission.completed = true;
            _constantMission.currentProgress = 1;

            //Guardar después en firebase
        }
    }

    public void ResetConstantMission()
    {
        _constantMission.completed = false;
        _constantMission.currentProgress = 0;
        _constantMission.rewardGranted = false;
        //Resetear eb Firebase también
    }

    #endregion
    private void InitializeDailyMissions()
    {
        _todayMissions = SelectRandomMissions(dailyMissionsCount);
        AddConstantMissionToTodayMissions();

        SaveAndLoadManager.SetStringValue(DateTime.Today.ToString("yyyyMMdd"), SaveAndLoadManager.LastDayUpdateName);
        SaveAndLoadManager.Save();
    }

    private void AddConstantMissionToTodayMissions()
    {
        _todayMissions.Insert(0, _constantMission);
    }
    public void CompleteMission(MissionData mission)
    {
        Debug.Log($"Mission completed: {mission.missionName}");

        GrantReward(mission.rewardType, mission.rewardAmount);
        mission.rewardGranted = true;
        OnCompleteMission?.Invoke();
    }

    private void GrantReward(RewardType rewardType, object rewardValue)
    {

        switch (rewardType)
        {
            case RewardType.Coins:
                if (rewardValue is int coins)
                    SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + coins, SaveAndLoadManager.CoinsName);
                break;
            case RewardType.Orbs:
                if (rewardValue is int orbs)
                    SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) + orbs, SaveAndLoadManager.OrbsName);
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
        .GroupBy(m => m.missionType)
        .OrderBy(x => Random.value)
        .Take(count - 1)
        .Select(group => group.OrderBy(x => Random.value).First())
        .Select(selected => new MissionData
        {
            missionID = selected.missionID,
            gameMode = selected.gameMode,
            missionName = selected.missionName,
            missionDescription = selected.missionDescription,
            missionType = selected.missionType,
            objectiveAmount = selected.objectiveAmount,
            missionDifficulty = selected.missionDifficulty,
            rewardType = selected.rewardType,
            rewardAmount = selected.rewardAmount
        }).ToList();
        #region OLD SELECTING SYSTEM
        //List<MissionData> selectedMissions = new List<MissionData>(_todayMissions);
        //List<MissionData> availableMissions = new List<MissionData>(_allAvailableMissions);

        //for (int i = 0; i < count && availableMissions.Count > 0; i++)
        //{
        //    int randomIndex = Random.Range(0, availableMissions.Count);
        //    MissionData selectedMission = availableMissions[randomIndex];

        //    MissionData missionCopy = new MissionData
        //    {
        //        missionID = selectedMission.missionID,
        //        gameMode = selectedMission.gameMode,
        //        missionName = selectedMission.missionName,
        //        missionDescription = selectedMission.missionDescription,
        //        missionType = selectedMission.missionType,
        //        objectiveAmount = selectedMission.objectiveAmount,
        //        missionDifficulty = selectedMission.missionDifficulty,
        //        rewardType = selectedMission.rewardType,
        //        rewardAmount = selectedMission.rewardAmount
        //    };

        //    selectedMissions.Add(missionCopy);
        //    availableMissions.RemoveAt(randomIndex);
        //    //Debug.Log(missionCopy.missionID);
        //    //Debug.Log(missionCopy.gameMode);
        //    //Debug.Log(missionCopy.missionName);
        //    //Debug.Log(missionCopy.missionDescription);
        //    //Debug.Log(missionCopy.missionType);
        //    //Debug.Log(missionCopy.objectiveAmount);
        //    //Debug.Log(missionCopy.missionDifficulty);
        //    //Debug.Log(missionCopy.rewardType);
        //    //Debug.Log(missionCopy.rewardAmount);
        //}
        #endregion
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
            if (mission.missionType == type && mission.gameMode == GameManager.Instance.GetCurrentGameMode && !mission.completed)
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
            }

            SaveAndLoadManager.SetDailyMissionProgressByMissionID(mission.missionID, mission.currentProgress);
        }


    }

    private void LoadSavedMissions()
    {
        _todayMissions = SelectRandomMissions(dailyMissionsCount);
        AddConstantMissionToTodayMissions();

        foreach(var mission in _todayMissions)
        {
            var savedData = SaveAndLoadManager.GetDailyMissionProgressDataByID(mission.missionID);

            if (savedData.lastUpdateDate == DateTime.Today.ToString("yyyyMMdd"))
            {
                mission.currentProgress = savedData.progress;
                mission.completed = mission.currentProgress >= mission.objectiveAmount;

                _missionProgress[mission.missionID] = savedData.progress;
            }
        }
    }

    public override IEnumerator InizializeManagers()
    {
        yield return null;
        while (_isInitialized)
            yield return null;
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