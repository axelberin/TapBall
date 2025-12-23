using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PowerUpManager;


public class DailyRewardManager : MonoBehaviour
{

    [SerializeField] private List<DailyReward> allRewards = new();
    private DailyRewardData _todayReward;

    [SerializeField] private int _rewardsToGrant;

    public static DailyRewardManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

    }
    private void Start()
    {
        _todayReward = LoadTodayReward();

        if (_todayReward == null)
        {
            _todayReward = SelectRandomRewardForDay(GetCurrentStreakDay());
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))//Continuar racha
        {
            ForceDayChange(System.DateTime.Today.AddDays(-1).ToString("yyyyMMdd"));
            TestReward();
        }
        if (Input.GetKeyDown(KeyCode.B)) //Romper racha
        {
            ForceDayChange(System.DateTime.Today.AddDays(-5).ToString("yyyyMMdd"));
            TestReward();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClaimDailyRewards();
        }
    }

    #region TESTING
    private void TestReward()
    {
        if (_todayReward == null)
        {
            Debug.LogWarning("No hay reward generado todavía");
            return;
        }
        Debug.Log($"RACHA: {GetCurrentStreakDay()}");
        Debug.Log(
            $"Reward -> Type: {_todayReward.Type}, " +
            $"Amount: {_todayReward.amount}, " +
            $"Quality: {_todayReward.quality}, " +
            $"PowerUp: {_todayReward.chosenPowerUp}"
        );
    }
    private void ForceDayChange(string simulatedDay)
    {
        SaveAndLoadManager.SetStringValue(simulatedDay, SaveAndLoadManager.DailyRewardLastClaimDayName);

        _todayReward = SelectRandomRewardForDay(GetCurrentStreakDay());
    }

    #endregion
#endif
    private DailyRewardData LoadTodayReward()
    {
        foreach (var reward in allRewards)
        {
            if (SaveAndLoadManager.IsDailyRewardFromToday(reward.rewardID, System.DateTime.Today.ToString("yyyyMMdd")))
            {
                return new DailyRewardData
                {
                    id = reward.rewardID,
                    Type = reward.Type,
                    amount = reward.amount,
                    quality = GetMinQualityByStreak(
                        SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardStreakName)
                    ),
                    chosenPowerUp = reward.chosenPowerUp,
                    rewardImage = reward.rewardImage
                };
            }
        }

        return null;
    }
    public int GetCurrentStreakDay()
    {
        string lastClaim = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardLastClaimDayName);

        if (string.IsNullOrEmpty(lastClaim))
            return 1;

        System.DateTime lastDate =
            System.DateTime.ParseExact(lastClaim, "yyyyMMdd", null);

        int daysDiff = (System.DateTime.Today - lastDate).Days;

        int streak = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardStreakName);

        if (daysDiff == 0)
            return Mathf.Clamp(streak, 1, 7);

        if (daysDiff == 1)
            return Mathf.Clamp(streak + 1, 1, 7);

        return 1;
    }
    private int GetMinQualityByStreak(int streakDay)
    {
        return streakDay switch
        {
            1 or 2 => 0,
            3 or 4 => 1,
            5 or 6 => 2,
            7 => 3,
            _ => 1
        };
    }

    public DailyRewardData SelectRandomRewardForDay(int streakCount)
    {
        _todayReward = allRewards
            .OrderBy(x => Random.value)
            .Select(reward => new DailyRewardData
            {
                id = reward.rewardID,
                Type = reward.Type,
                amount = reward.amount,
                quality = GetMinQualityByStreak(streakCount),
                chosenPowerUp = reward.chosenPowerUp,
                rewardImage = reward.rewardImage
            })
            .FirstOrDefault();

        if (_todayReward.Type == DailyRewardType.PowerUp)
            _todayReward.chosenPowerUp = SelectRandomPowerUpByProbability();

        SaveAndLoadManager.SetDailyRewardData(_todayReward.id, System.DateTime.Today.ToString("yyyyMMdd"), false, true, true);

        return _todayReward;

    }

    private PowerUpType SelectRandomPowerUpByProbability()
    {
        var probabilities = new[]
        {
            new { type = PowerUpType.TimeStopPowerUp,         weight = 55 },
            new { type = PowerUpType.StopTouchCounterPowerUp, weight = 25 },
            new { type = PowerUpType.ImmunityPowerUp,         weight = 15 },
            new { type = PowerUpType.RevivePowerUp,           weight = 5 }
        };

        int total = probabilities.Sum(p => p.weight);
        int rng = Random.Range(0, total);

        foreach (var p in probabilities)
        {
            if (rng < p.weight)
                return p.type;

            rng -= p.weight;

        }

        Debug.LogError("No probabilites recognized.");
        return PowerUpType.TimeStopPowerUp; //fallback 
    }

    public bool CanClaimToday()
    {
        if (_todayReward == null)
            return false;

        return !SaveAndLoadManager.IsDailyRewardClaimed(_todayReward.id);
    }

    public void ClaimDailyRewards()
    {
        if (!CanClaimToday())
            return;

        string rewardID = _todayReward.id;
        string today = System.DateTime.Today.ToString("yyyyMMdd");

        // Es el reward de hoy y ya fue reclamado?
        if (SaveAndLoadManager.IsDailyRewardFromToday(rewardID, today) &&
            SaveAndLoadManager.IsDailyRewardClaimed(rewardID))
            return;

        // Dar reward
        GrantRewards(_todayReward);
        Debug.Log($"Reward Claimed,id:{rewardID},type {_todayReward.Type}, amount {_todayReward.amount}");
        //Marcar como reclamado
        SaveAndLoadManager.SetIntValue(GetCurrentStreakDay(), SaveAndLoadManager.DailyRewardStreakName, true, true);
        SaveAndLoadManager.SetStringValue(System.DateTime.Today.ToString("yyyyMMdd"), SaveAndLoadManager.DailyRewardLastClaimDayName, true, true);
        SaveAndLoadManager.SetDailyRewardData(rewardID, today, true, true, true);
    }

    private void GrantRewards(DailyRewardData reward)
    {

        switch (reward.Type)
        {
            case DailyRewardType.Coins:
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName)
                    + reward.amount, SaveAndLoadManager.CoinsName);
                break;
            case DailyRewardType.Orbs:
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName)
                    + reward.amount, SaveAndLoadManager.OrbsName);
                break;
            case DailyRewardType.Skin:
                Debug.Log($"Skin reward");
                break;
            case DailyRewardType.PowerUp:
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
                    reward.chosenPowerUp.ToString()) + reward.amount, SaveAndLoadManager.PowerUpPrefix + reward.chosenPowerUp.ToString(), true, true);
                break;


        }
    }

    public enum DailyRewardType
    {
        PowerUp,
        Coins,
        Orbs,
        Skin
    }

    #region UTILITY
    public DailyRewardData GetTodayReward => _todayReward;
    #endregion

}

[System.Serializable]
public class DailyRewardData
{
    public string id;
    public DailyRewardManager.DailyRewardType Type;
    public int amount;
    public int quality;
    public PowerUpType chosenPowerUp;
    public Sprite rewardImage;
}


