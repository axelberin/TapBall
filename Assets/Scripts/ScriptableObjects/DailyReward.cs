using UnityEngine;
using UnityEngine.UI;
using static DailyRewardManager;
using static PowerUpManager;

[CreateAssetMenu(fileName = "DailyReward", menuName = "ScriptableObjects/DailyRewards")]
public class DailyReward : ScriptableObject
{
    public string rewardID;

    public DailyRewardType Type;
    public int amount;

    [Range(0,4)]
    public int quality;

    [System.NonSerialized]
    public PowerUpType chosenPowerUp;

    public Image rewardImage;
}
