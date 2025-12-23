using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DailyRewardManager;

public class DailyRewardsController : CanvasElementLocator
{
    private GameObject _rewardsContent;
    private List<GameObject> _rewardSlotPrefab = new();

    private void OnEnable()
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
        var dailyReward = Instance.GetTodayReward;

        if (dailyReward == null)
            return;

        for (int i = 0; i < _rewardSlotPrefab.Count; i++)
        {
            int day = i + 1;
            GameObject slot = _rewardSlotPrefab[i];

            var claimButton = FindAndValidateComponent<Button>(slot.transform, "ClaimDailyRewardButton");
            var rewardImage = FindAndValidateComponent<Image>(slot.transform, "DailyRewardImg");
            var amountText = FindAndValidateComponent<TextMeshProUGUI>(slot.transform, "DailyRewardText");

            claimButton.onClick.RemoveAllListeners();

            if (day == SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DailyRewardStreakName))
            {
                rewardImage.sprite = dailyReward.rewardImage;
                rewardImage.gameObject.SetActive(true);

                amountText.text = dailyReward.amount.ToString();
                amountText.gameObject.SetActive(true);

                claimButton.interactable = Instance.CanClaimToday();
                claimButton.onClick.AddListener(() =>
                {
                    Debug.Log("Apretaste el reclamo de los rewards");
                    Instance.ClaimDailyRewards();
                    ShowRewardByDayInUI();
                });
            }
            else
            {
                claimButton.interactable = false;
                rewardImage.gameObject.SetActive(false);
                amountText.gameObject.SetActive(false);
            }
        }
    }
}
