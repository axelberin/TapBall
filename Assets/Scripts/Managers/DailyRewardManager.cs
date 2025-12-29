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
    public System.Action OnDailyRewardUpdated;
    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

    }
    private void Start()
    {
        CheckForDayChange();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))//Continuar racha
        {
            ForceDayChange(1);
            TestReward();
        }
        if (Input.GetKeyDown(KeyCode.B)) //Romper racha
        {
            ForceDayChange(5);
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
    private void ForceDayChange(int simulatedDay)
    {
        string simulatedDate = System.DateTime.Today
        .AddDays(-simulatedDay)
        .ToString("yyyyMMdd");

        SaveAndLoadManager.SetStringValue(
            simulatedDate,
            SaveAndLoadManager.DailyRewardTodayDate,
            true, true
        );

        CheckForDayChange();
    }

    #endregion
#endif

    private void CheckForDayChange()
    {
        if (SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardTodayDate) != System.DateTime.Today.ToString("yyyyMMdd"))
        {
            SelectRandomRewardForDay(GetCurrentStreakDay());
        }
        else
        {
            _todayReward = LoadTodayReward();
        }

        OnDailyRewardUpdated?.Invoke();
    }
    private DailyRewardData LoadTodayReward()
    {
        if (string.IsNullOrEmpty(SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardTodayID)))
            return null;

        DailyReward reward = allRewards.FirstOrDefault(r => r.rewardID == SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardTodayID));

        if (reward == null)
            return null;

        return new DailyRewardData
        {
            id = reward.rewardID,
            Type = reward.Type,
            amount = reward.amount,
            quality = GetMinQualityByStreak(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardStreakName)),
            chosenPowerUp = reward.chosenPowerUp,
            rewardImage = reward.rewardImage
        };
    }
    public int GetCurrentStreakDay()
    {
        if (string.IsNullOrEmpty(SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardTodayDate)))
        {
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.DailyRewardStreakName, true, true);
            return 1;
        }


        int daysDiff = (System.DateTime.Today - System.DateTime.ParseExact(SaveAndLoadManager.
            GetStringValue(SaveAndLoadManager.DailyRewardTodayDate), "yyyyMMdd", null)).Days;

        int streak = SaveAndLoadManager.GetIntValue(
            SaveAndLoadManager.DailyRewardStreakName);

        int result;

        if (daysDiff == 0)
        {
            result = streak;
        }
        else if (daysDiff == 1)
        {
            if (SaveAndLoadManager.GetBoolValue(SaveAndLoadManager.DailyRewardTodayClaimed))
                result = streak + 1;
            else
                result = 1;
        }
        else
        {
            result = 1;
        }

        if (result > 7)
            result = 1;

        SaveAndLoadManager.SetIntValue(result,
            SaveAndLoadManager.DailyRewardStreakName, true, true);

        return result;
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
        .OrderBy(_ => Random.value)
        .Select(reward => new DailyRewardData
        {
            id = reward.rewardID,
            Type = reward.Type,
            amount = reward.amount,
            quality = GetMinQualityByStreak(streakCount),
            chosenPowerUp = reward.chosenPowerUp,
            rewardImage = reward.rewardImage
        })
        .First();

        if (_todayReward.Type == DailyRewardType.PowerUp)
            _todayReward.chosenPowerUp = SelectRandomPowerUpByProbability();

        SaveAndLoadManager.SetStringValue(_todayReward.id, SaveAndLoadManager.DailyRewardTodayID, true, true);
        SaveAndLoadManager.SetStringValue(System.DateTime.Today.ToString("yyyyMMdd"), SaveAndLoadManager.DailyRewardTodayDate, true, true);
        SaveAndLoadManager.SetBoolValue(false, SaveAndLoadManager.DailyRewardTodayClaimed, true, true);

        OnDailyRewardUpdated?.Invoke();
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
        return !SaveAndLoadManager.GetBoolValue(SaveAndLoadManager.DailyRewardTodayClaimed);
    }

    public void ClaimDailyRewards()
    {
        if (!CanClaimToday())
            return;

        GrantRewards(_todayReward);

        SaveAndLoadManager.SetBoolValue(true, SaveAndLoadManager.DailyRewardTodayClaimed, true, true);
        SaveAndLoadManager.SetIntValue(GetCurrentStreakDay(), SaveAndLoadManager.DailyRewardStreakName, true, true);
        SaveAndLoadManager.SetStringValue(System.DateTime.Today.ToString("yyyyMMdd"), SaveAndLoadManager.DailyRewardLastClaimDayName, true, true);

        OnDailyRewardUpdated?.Invoke();
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
    public int GetStreakDay => GetCurrentStreakDay();
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


