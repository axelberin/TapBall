using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PowerUpManager;


public class DailyRewardManager : MonoBehaviour
{

    [SerializeField] private List<DailyReward> allRewards = new List<DailyReward>();
    private List<DailyReward> _todayRewards = new List<DailyReward>();

    private int _rewardsToGrant;

    public DailyRewardManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

    }

    private List<DailyReward> SelectRandomRewards(int count)
    {
        _todayRewards.Clear();

        var rewardsPool = allRewards;

        _todayRewards = rewardsPool
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


