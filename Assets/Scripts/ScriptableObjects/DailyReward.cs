using UnityEngine;
using static DailyRewardManager;
using static PowerUpManager;

[CreateAssetMenu(fileName = "DailyReward", menuName = "ScriptableObjects/DailyRewards")]
public class DailyReward : ScriptableObject
{
    public string rewardID;

    public DailyRewardType Type;
    public int amount;

    //(0: Common, día 1 y 2), (1: Good, día 3 y 4), (2: Epic, día 5 y 6), (3: Legendary, día 7)
    [Range(0,3)]
    public int quality;

    [System.NonSerialized]
    public PowerUpType chosenPowerUp;

    public Sprite rewardImage;
}
