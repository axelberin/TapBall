using UnityEngine;
using static DailyRewardManager;
using static PowerUpManager;

[CreateAssetMenu(fileName = "DailyReward", menuName = "ScriptableObjects/DailyRewards")]
public class DailyReward : ScriptableObject
{
    public DailyRewardType Type;
    public int amount;

    [Range(0,3)]
    public int quality;

    [System.NonSerialized]
    public PowerUpType chosenPowerUp;
}
