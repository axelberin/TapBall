using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DailyRewardManager;

public class DailyRewardsController : CanvasElementLocator
{
    private GameObject _rewardsContent;
    private readonly List<GameObject> _rewardSlots = new();


    private void OnEnable()
    {
        _rewardsContent = FindAndValidateGameObjectComponent(transform, "RewardsContent");

        _rewardSlots.Clear();

        for (int i = 1; i <= 7; i++)
        {
            GameObject slot = FindAndValidateGameObjectComponent(_rewardsContent.transform,$"contentReward{i}");
            _rewardSlots.Add(slot);
        }
        Instance.OnDailyRewardUpdated += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        Instance.OnDailyRewardUpdated -= RefreshUI;
    }

    private void RefreshUI()
    {
        DailyRewardData todayReward = Instance.GetTodayReward;
        if (todayReward == null)
            return;

        for (int i = 0; i < _rewardSlots.Count; i++)
        {
            int day = i + 1;
            GameObject slot = _rewardSlots[i];

            Button claimButton = FindAndValidateComponent<Button>(slot.transform,"ClaimDailyRewardButton");
            Image rewardImage = FindAndValidateComponent<Image>( slot.transform,"DailyRewardImg");
            TextMeshProUGUI amountText = FindAndValidateComponent<TextMeshProUGUI>( slot.transform,"DailyRewardText");

            claimButton.onClick.RemoveAllListeners();

            rewardImage.gameObject.SetActive(false);
            amountText.gameObject.SetActive(false);
            claimButton.interactable = false;

            if (day == Instance.GetStreakDay)
            {
                rewardImage.sprite = todayReward.rewardImage;
                rewardImage.gameObject.SetActive(true);

                amountText.text = todayReward.amount.ToString();
                amountText.gameObject.SetActive(true);

                bool canClaim = Instance.CanClaimToday();
                claimButton.interactable = canClaim;

                if (canClaim)
                {
                    claimButton.onClick.AddListener(() =>
                    {
                        Debug.Log("Apretaste el reclamo de los rewards");
                        Instance.ClaimDailyRewards();
                    });
                }
            }
        }
    }
}
