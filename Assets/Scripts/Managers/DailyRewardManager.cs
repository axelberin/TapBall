using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PowerUpManager;


public class DailyRewardManager : MonoBehaviour
{

    [SerializeField] private List<DailyReward> allRewards = new();
    private DailyRewardData _todayRewards;

    [SerializeField] private int _rewardsToGrant;

    public static DailyRewardManager Instance { get; private set; }

#if UNITY_EDITOR
    [SerializeField] private int debugDayOffset = 0;
#endif

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))//Continuar racha
        {
            SaveAndLoadManager.SetStringValue(
                System.DateTime.Today.AddDays(-1).ToString("yyyyMMdd"),
                SaveAndLoadManager.DailyRewardLastClaimDayName
            );

            TestReward();
        }
        if (Input.GetKeyDown(KeyCode.B)) //Romper racha
        {
            SaveAndLoadManager.SetStringValue(
                System.DateTime.Today.AddDays(-5).ToString("yyyyMMdd"),
                SaveAndLoadManager.DailyRewardLastClaimDayName
            );

            TestReward();
        }
    }

    #region TESTING
    private void TestReward()
    {
        DailyRewardData reward = SelectRandomRewards(GetCurrentStreakDay());

        Debug.Log($"RACHA: {GetCurrentStreakDay()}");
        Debug.Log(
            $"Reward -> Type: {reward.Type}, " +
            $"Amount: {reward.amount}, " +
            $"Quality: {reward.quality}, " +
            $"PowerUp: {reward.chosenPowerUp}"
        );
    }
    #endregion
#endif

    public int GetCurrentStreakDay()
    {
        string lastClaim = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardLastClaimDayName);
        string today = System.DateTime.Now.ToString("yyyyMMdd");

        if (string.IsNullOrEmpty(lastClaim))
        {
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.DailyRewardStreakName);
            SaveAndLoadManager.SetStringValue(today, SaveAndLoadManager.DailyRewardLastClaimDayName);
            SaveAndLoadManager.Save();
            return 1; //Si el último día de claimeo es null, lo setea como el primer día de la racha
        }

        System.DateTime lastDate = System.DateTime.ParseExact(lastClaim, "yyyyMMdd", null); //Parsea el día exacto 
        int daysDiff = (System.DateTime.Today - lastDate).Days;//Calcula cuántos días pasaron del último claim

        int streak = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardStreakName);//Consigo la racha

        if (daysDiff == 1)
            streak++;
        else if (daysDiff > 1)
            streak = 1;

        if (streak > 7)
        {
            streak = 1;
            for (int i = 1; i <= 7; i++)
            {
                SaveAndLoadManager.SetRewardClaimedKey($"DailyReward_Day_{GetCurrentStreakDay()}", false);
            }
        }
        //Hago los cálculos dependiendo de si tiene racha contínua o no
        SaveAndLoadManager.SetIntValue(streak, SaveAndLoadManager.DailyRewardStreakName);
        SaveAndLoadManager.SetStringValue(today, SaveAndLoadManager.DailyRewardLastClaimDayName);
        SaveAndLoadManager.Save();

        return streak; //Seteo los values, los guardo y los retorno
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

    private DailyRewardData SelectRandomRewards(int streakCount)
    {
        _todayRewards = allRewards
            .OrderBy(x => Random.value)
            .Select(reward => new DailyRewardData
            {
                Type = reward.Type,
                amount = reward.amount,
                quality = GetMinQualityByStreak(streakCount),
                chosenPowerUp = reward.chosenPowerUp
            })
            .FirstOrDefault();

        if (_todayRewards.Type == DailyRewardType.PowerUp)
            _todayRewards.chosenPowerUp = SelectRandomPowerUpByProbability();

        return _todayRewards;

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
        string lastClaim = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.DailyRewardLastClaimDayName);

        if (string.IsNullOrEmpty(lastClaim))
            return true;

        if (lastClaim != System.DateTime.Now.ToString("yyyyMMdd"))
            return true;

        return SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardClaimedTodayName) == 0;
    }

    public void ClaimDailyRewards()
    {
        if (!CanClaimToday())
            return;

        string rewardID = $"DailyReward_Day_{GetCurrentStreakDay()}";

        if (SaveAndLoadManager.IsRewardClaimed(rewardID))
            return; // ya fue reclamado ese día

        DailyRewardData reward = SelectRandomRewards(GetCurrentStreakDay());
        GrantRewards(reward);

        SaveAndLoadManager.SetRewardClaimedKey(rewardID, true, true);
        SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.DailyRewardClaimedTodayName, true);
    }

    private void GrantRewards(DailyRewardData reward)
    {

        switch (reward.Type)
        {
            case DailyRewardType.Coins:
                SaveAndLoadManager.SetIntValue(reward.amount, SaveAndLoadManager.CoinsName);
                break;

            case DailyRewardType.Orbs:
                SaveAndLoadManager.SetIntValue(reward.amount, SaveAndLoadManager.OrbsName);
                break;

            case DailyRewardType.PowerUp:
                PowerUpManager.Instance.AddPowerUp(reward.chosenPowerUp, reward.amount);
                break;

            case DailyRewardType.Skin:
                Debug.Log("Skin granted from reward");
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
    #endregion

}

[System.Serializable]
public class DailyRewardData
{
    public DailyRewardManager.DailyRewardType Type;
    public int amount;
    public int quality;
    public PowerUpType chosenPowerUp;
}


