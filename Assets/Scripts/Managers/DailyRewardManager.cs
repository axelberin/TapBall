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
        _todayReward = SelectRandomRewardForDay(GetCurrentStreakDay());
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
        SaveAndLoadManager.SetStringValue(
            simulatedDay,
            SaveAndLoadManager.DailyRewardLastClaimDayName
        );

        _todayReward = SelectRandomRewardForDay(GetCurrentStreakDay());
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
        // Marcar como reclamado
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
                PowerUpManager.Instance.AddPowerUp(_todayReward.chosenPowerUp, _todayReward.amount);
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


