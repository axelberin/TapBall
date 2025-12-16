using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PowerUpManager;


public class DailyRewardManager : MonoBehaviour
{

    [SerializeField] private List<DailyReward> allRewards = new List<DailyReward>();
    private List<DailyReward> _todayRewards = new List<DailyReward>();

    [SerializeField] private int _rewardsToGrant;

    public DailyRewardManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.V))
        {
            SelectRandomRewards(GetCurrentStreakDay());
            foreach (var reward in _todayRewards)
            {
                Debug.Log($"Reward: {reward.name}");
                Debug.Log($"Type: {reward.Type}");
                Debug.Log($"Amount: {reward.amount}");
                Debug.Log($"Quality: {reward.quality}");
            }
        }
#endif
    }

    private int GetCurrentStreakDay()
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

    private List<DailyReward> SelectRandomRewards(int count)
    {
        _todayRewards.Clear();

        _todayRewards = allRewards
            .OrderBy(x => Random.value)
            .Take(count)
            .ToList();

        foreach (var reward in _todayRewards)
        {
            if (reward.Type == DailyRewardType.PowerUp)
            {
                reward.chosenPowerUp = SelectRandomPowerUpByProbability();
            }
        }

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

    public enum DailyRewardType
    {
        PowerUp,
        Coins,
        Orbs,
        Skin
    }
}


