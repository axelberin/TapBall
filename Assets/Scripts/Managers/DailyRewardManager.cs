using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PowerUpManager;


public class DailyRewardManager : MonoBehaviour
{

    [SerializeField] private List<DailyReward> allRewards = new();
    private List<DailyRewardData> _todayRewards = new();

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
        if (Input.GetKeyDown(KeyCode.N))
            SaveAndLoadManager.SetStringValue(System.DateTime.Today.AddDays(1).ToString("yyyyMMdd"),
                SaveAndLoadManager.DailyRewardLastClaimDayName);

        if (Input.GetKeyDown(KeyCode.B))
            SaveAndLoadManager.SetStringValue(System.DateTime.Today.AddDays(-1).ToString("yyyyMMdd"),
                SaveAndLoadManager.DailyRewardLastClaimDayName);

        if (Input.GetKeyDown(KeyCode.V))
        {
            //SelectRandomRewards(GetCurrentStreakDay());
            foreach (var reward in _todayRewards)
            {
                Debug.Log($"Type: {reward.Type}");
                Debug.Log($"Amount: {reward.amount}");
                Debug.Log($"Quality: {reward.quality}");
                Debug.Log($"PowerUp: {reward.chosenPowerUp}");
            }
        }
    }
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
            streak = 1;
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
            1 or 2 => 1,
            3 or 4 => 2,
            5 or 6 => 3,
            7 => 4,
            _ => 1
        };
    }

   // private DailyRewardData SelectRandomRewards(int streakCount)
   // {
   //     _todayRewards.Clear();
   //
   //     _todayRewards = allRewards
   //         .OrderBy(x => Random.value)
   //         .Select(reward => new DailyRewardData
   //         {
   //             Type = reward.Type,
   //             amount = reward.amount,
   //             quality = Random.Range(GetMinQualityByStreak(streakCount), 4),
   //             chosenPowerUp = reward.chosenPowerUp
   //         })
   //         .FirstOrDefault();
   //
   //     foreach (var reward in _todayRewards)
   //     {
   //         if (reward.Type == DailyRewardType.PowerUp)
   //         {
   //             reward.chosenPowerUp = SelectRandomPowerUpByProbability();
   //         }
   //     }
   // }

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

       // GrantRewards(SelectRandomRewards(GetCurrentStreakDay()));

        SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.DailyRewardClaimedTodayName);

        SaveAndLoadManager.Save();
    }

    private void GrantRewards(List<DailyRewardData> rewards)
    {
        foreach (var reward in rewards)
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
                    Debug.Log("Skin granted from rewards");
                    break;
            }
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


