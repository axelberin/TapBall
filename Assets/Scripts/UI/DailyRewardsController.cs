using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DailyRewardManager;

public class DailyRewardsController : CanvasElementLocator
{
    private GameObject _rewardsContent;
    private List<GameObject> _rewardSlotPrefab = new();

    private void Start()
    {
        _rewardsContent = FindAndValidateGameObjectComponent(transform, "RewardsContent");

        _rewardSlotPrefab.Clear();

        for (int i = 1; i <= 7; i++)
        {
            GameObject slot = FindAndValidateGameObjectComponent(_rewardsContent.transform, $"contentReward{i}");
            _rewardSlotPrefab.Add(slot);
        }
        ShowRewardByDayInUI();
    }

    private void ShowRewardByDayInUI()
    {
        if (_rewardsContent == null)
            return;

        var currentDay = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardStreakName);
        var dailyReward = Instance.GetTodayReward;

    
        for (int i = 0; i < _rewardSlotPrefab.Count; i++)
        {
            int day = i + 1;
            GameObject slot = _rewardSlotPrefab[i];

            var claimRewardButton = FindAndValidateComponent<Button>(slot.transform, "ClaimDailyRewardButton");
            var rewardImage = FindAndValidateComponent<Image>(slot.transform, "DailyRewardImg");
            var amountText = FindAndValidateComponent<TextMeshProUGUI>(slot.transform, "DailyRewardText");

            

            claimRewardButton.onClick.RemoveAllListeners();
            if (day < currentDay || (day == currentDay && !Instance.CanClaimToday()))
            {
                claimRewardButton.interactable = false;
                rewardImage.gameObject.SetActive(false);
                amountText.gameObject.SetActive(false);
                continue;
            }

            if (day == currentDay && Instance.CanClaimToday())
            {
                claimRewardButton.interactable = true;
                claimRewardButton.onClick.AddListener(() =>
                {
                    rewardImage.gameObject.SetActive(true);
                    amountText.gameObject.SetActive(true);
                    Instance.ClaimDailyRewards();
                    rewardImage.sprite = dailyReward.rewardImage;
                    UIManager.Instance.SetText(amountText, dailyReward.amount);
                    ShowRewardByDayInUI();
                });
            }
            else
            {
                claimRewardButton.interactable = false;
            }

        }
    }
}
