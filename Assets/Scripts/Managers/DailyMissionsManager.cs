using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
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

    public MissionData()
    {
        currentProgress = 0;
        completed = false;
    }
}
public class DailyMissionsManager : MonoBehaviour
{
    //private string _spreadSheetURL = "https://docs.google.com/spreadsheets/d/10vebWCUT7AbVgmcj5rOvFOdGKgiyTMoxyDSwXNgbW_U/edit?gid=0#gid=0";

    private List<MissionData> _allAvailableMissions = new();
    private List<MissionData> _todayMissions = new();

    [SerializeField] public int dailyMissionsCount = 2;


    public static event Action<List<MissionData>> OnMissionReady;
    public static event Action<MissionData> OnMissionCompleted;
    public static event Action<MissionData, float> OnMissionProgressUpdated; //Hacer con object en vez de float directo para hacerlo genérico


    private Dictionary<string, float> _missionProgress = new();
    public static DailyMissionsManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        CreateTestMissions();
        InitializeDailyMissions();
    }

    private void CreateTestMissions()
    {
        _allAvailableMissions.Clear();

        _allAvailableMissions.Add(new MissionData
        {
            missionID = "TEST001",
            gameMode = GameManager.GameModes.Dunk,
            missionName = "Tapping genius",
            missionDescription = "Tap 10 times",
            missionType = MissionType.Touches,
            objectiveAmount = 10,
            missionDifficulty = 2,
            rewardType = RewardType.Coins,
            rewardAmount = 3
        });

        _allAvailableMissions.Add(new MissionData
        {
            missionID = "TEST002",
            gameMode = GameManager.GameModes.Dunk,
            missionName = "Level Madness",
            missionDescription = "Finish 3 levels",
            missionType = MissionType.LevelsPassed,
            objectiveAmount = 3,
            missionDifficulty = 1,
            rewardType = RewardType.Coins,
            rewardAmount = 2
        });

        _allAvailableMissions.Add(new MissionData
        {
            missionID = "TEST004",
            gameMode = GameManager.GameModes.Time,
            missionName = "Flash",
            missionDescription = "Complete a Timer level under 8 seconds",
            missionType = MissionType.TimeLimit,
            objectiveAmount = 8,
            missionDifficulty = 3,
            rewardType = RewardType.Coins,
            rewardAmount = 5
        });

        _allAvailableMissions.Add(new MissionData
        {
            missionID = "TEST005",
            gameMode = GameManager.GameModes.OneTouch,
            missionName = "Close call",
            missionDescription = "Win a OneTouch mode level with 5 or less touches left",
            missionType = MissionType.Touches,
            objectiveAmount = 5,
            missionDifficulty = 2,
            rewardType = RewardType.Coins,
            rewardAmount = 4
        });
    }

    private void InitializeDailyMissions()
    {
        SelectRandomMissions(dailyMissionsCount);
    }

    private void CompleteMission(MissionData mission)
    {
        Debug.Log($"Mission completed: {mission.missionName}");

        GrantReward(mission.rewardType, mission.rewardAmount);

        mission.completed = true;
        OnMissionCompleted?.Invoke(mission);

    }

    private void GrantReward(RewardType rewardType, int amount)//probar con object como un generic de variables(INvestigar xd)
    {
        switch (rewardType)
        {
            case RewardType.Coins:
                SaveAndLoadManager.SetIntValue(amount, SaveAndLoadManager.CoinsName, true, true);
                break;
            case RewardType.Orbs:
                SaveAndLoadManager.SetIntValue(amount, SaveAndLoadManager.OrbsName, true, true);
                break;
            case RewardType.BattlePassXP:
                Debug.Log($"Se ha otorgado {(float)amount} de XP al battlepass");
                break;
        }
    }

    private List<MissionData> SelectRandomMissions(int count)
    {
        if (_allAvailableMissions.Count == 0)
            return new List<MissionData>();

        List<MissionData> selectedMissions = new();
        List<MissionData> availableMissions = new List<MissionData>(_allAvailableMissions);

        for (int i = 0; i < count && availableMissions.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableMissions.Count);
            MissionData selectedMission = availableMissions[randomIndex];

            MissionData missionCopy = new MissionData
            {
                missionID = selectedMission.missionID,
                gameMode = selectedMission.gameMode,
                missionName = selectedMission.missionName,
                missionDescription = selectedMission.missionDescription,
                missionType = selectedMission.missionType,
                objectiveAmount = selectedMission.objectiveAmount,
                missionDifficulty = selectedMission.missionDifficulty,
                rewardType = selectedMission.rewardType,
                rewardAmount = selectedMission.rewardAmount
            };

            selectedMissions.Add(missionCopy);
            availableMissions.RemoveAt(randomIndex);
            // Debug.Log(missionCopy.missionID);
            // Debug.Log(missionCopy.gameMode);
            // Debug.Log(missionCopy.missionName);
            // Debug.Log(missionCopy.missionDescription);
            // Debug.Log(missionCopy.missionType);
            // Debug.Log(missionCopy.objectiveAmount);
            // Debug.Log(missionCopy.missionDifficulty);
            // Debug.Log(missionCopy.rewardType);
            // Debug.Log(missionCopy.rewardAmount);
        }

        return selectedMissions;
    }

    private void UpdateMissionProgress(MissionType type, float amount)//Posiblemente también tenga que usar object
    {
        if (_todayMissions.Count == 0)
            return;

        bool updated = false;

        foreach (var mission in _todayMissions)
        {
            if (mission.missionType == type && !mission.completed)
            {
                float newProgress = _missionProgress[mission.missionID] + amount;
                _missionProgress[mission.missionID] = Mathf.Min(newProgress, mission.objectiveAmount);

                //Terminar el progreso de la misión
            }
        }
    }
    #region CSV
    // private IEnumerator DownloadAndParseCSV()
    // {
    //     using (UnityWebRequest request = UnityWebRequest.Get(_spreadSheetURL))
    //     {
    //         yield return request.SendWebRequest();
    //
    //         if (request.result == UnityWebRequest.Result.Success)
    //             ParseCSV(request.downloadHandler.text);
    //         else
    //             Debug.LogError("Error al descargar el archivo CSV: " + request.error);
    //     }
    // }
    // private void ParseCSV(string csvContent)
    // {
    //     csvContent = csvContent.Replace("\"", "");
    //     string[] lines = csvContent.Split('\n');
    //
    //     for (int i = 1; i < lines.Length; i++)
    //     {
    //         if (string.IsNullOrEmpty(lines[i].Trim()))
    //             continue;
    //         string[] values = lines[i].Split(",");
    //
    //         if (values.Length < 9)
    //             continue;
    //
    //         MissionData quest = new MissionData();
    //         quest.missionID = values[0].Trim();
    //         quest.gameMode = ParseGameMode(values[1].Trim());
    //         quest.missionName = values[2].Trim();
    //         quest.missionDescription = values[3].Trim();
    //         quest.missionType = ParseMissionType(values[4].Trim());
    //         if (int.TryParse(values[5].Trim(), out int objective))
    //         {
    //             quest.objectiveAmount = objective;
    //         }
    //         else quest.objectiveAmount = 0;
    //
    //         if (int.TryParse(values[6].Trim(), out int difficulty))
    //         {
    //             quest.missionDifficulty = difficulty;
    //         }
    //         else quest.missionDifficulty = 1;
    //         quest.rewardType = ParseRewardType(values[7].Trim());
    //
    //         if (int.TryParse(values[8].Trim(), out int reward))
    //         {
    //             quest.rewardAmount = reward;
    //         }
    //         else quest.rewardAmount = 0;
    //
    //         _allAvailableMissions.Add(quest);
    //
    //     }
    // }
    //
    // private GameManager.GameModes ParseGameMode(string gameModeString)
    // {
    //     return (GameManager.GameModes)Enum.Parse(typeof(GameManager.GameModes), gameModeString, true);
    // }
    // private MissionType ParseMissionType(string missionTypeString)
    // {
    //     return (MissionType)Enum.Parse(typeof(MissionType), missionTypeString, true);
    // }
    // private RewardType ParseRewardType(string rewardString)
    // {
    //     return (RewardType)Enum.Parse(typeof(RewardType), rewardString, true);
    // }
    #endregion
}
#region ENUMS
public enum MissionType
{
    Touches,
    TimeLimit,
    CoinsCollected,
    LevelsPassed
}

public enum RewardType
{
    Coins,
    Orbs,
    BattlePassXP
}
#endregion