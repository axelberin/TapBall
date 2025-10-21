using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;
using static DailyMissionsManager;

public class DailyMissionsCanvasController : CanvasElementLocator
{
    private GameObject _contentScroll;
    private GameObject _missionRowObjectPrefab;

    void Start()
    {
        _contentScroll = FindAndValidateGameObjectComponent(transform, "QuestsContent");

        AddressablesUtility.LoadAsset<GameObject>("DailyQuestRow", missionAddressable =>
        {
            _missionRowObjectPrefab = missionAddressable;

            for (int i = 0; i < Instance.GetTodayMissions.Count; i++)
            {
                GameObject newRow = Instantiate(_missionRowObjectPrefab, _contentScroll.transform);

                UpdateUI(newRow.transform, i);

                CheckAndCompleteConstantMission();
            }
        });
    }

    private void OnEnable()
    {
        if (_contentScroll != null && Instance.GetTodayMissions.Count > 0)
        {
            for (int i = 0; i < Instance.GetTodayMissions.Count; i++)
            {
                Transform rowTransform = _contentScroll.transform.GetChild(i);
                UpdateUI(rowTransform, i);
            }
        }
        Instance.OnDailyMissionsReset += RefreshAllMissionsUI;
    }

    private void OnDisable()
    {
        Instance.OnDailyMissionsReset -= RefreshAllMissionsUI;
    }

    private void RefreshAllMissionsUI()
    {
        if (_contentScroll == null) return;

        for (int i = 0; i < _contentScroll.transform.childCount; i++)
        {
            Transform rowTransform = _contentScroll.transform.GetChild(i);
            UpdateUI(rowTransform, i);
        }
    }
    public void UpdateUI(Transform rowTransform, int missionIndex)
    {
        var mission = Instance.GetTodayMissions[missionIndex];

        var rewardImage = FindAndValidateComponent<Image>(rowTransform, "RewardImg");
        var progressBar = FindAndValidateComponent<Image>(rowTransform, "ProgressBarImage");
        var missionDescription = FindAndValidateComponent<TextMeshProUGUI>(rowTransform, "QuestText");
        var missionProgressPercentage = FindAndValidateComponent<TextMeshProUGUI>(rowTransform, "ProgressText");
        var grantRewardButton = FindAndValidateComponent<Button>(rowTransform, "RewardButton");
        var rewardAmountText = FindAndValidateComponent<TextMeshProUGUI>(rowTransform, "RewardAmountText");

        var missionPercentage = mission.currentProgress / mission.objectiveAmount;

        rewardAmountText.text = $"x{mission.rewardAmount}";
        missionDescription.text = mission.missionDescription;
        progressBar.fillAmount = missionPercentage;
        missionProgressPercentage.text = MathF.Truncate(missionPercentage * 100).ToString() + "%";

        AddressablesUtility.LoadAsset<GameObject>($"{mission.rewardType}Image", rewardImageAddressable =>
        { 
            rewardImage.sprite = rewardImageAddressable.GetComponent<Image>().sprite;
            if(mission.rewardType == RewardType.Coins)
            {
                rewardImage.transform.localScale = new Vector3(2.74f, 2.74f);
            }
        });

        if (mission.rewardGranted == false)
        {
            grantRewardButton.interactable = mission.completed;
        }
        else
        {
            grantRewardButton.interactable = false;
            return;
        }
        grantRewardButton.onClick.RemoveAllListeners();
        grantRewardButton.onClick.AddListener(() =>
        {
            Instance.CompleteMission(mission);
            grantRewardButton.interactable = false;
        });
    }

    private void CheckAndCompleteConstantMission()
    {
        var constanMission = Instance.GetTodayMissions
            .FirstOrDefault(m => m.missionID == "DAILY_LOGIN");

        if(constanMission != null && !constanMission.completed && !constanMission.rewardGranted)
        {
            Instance.CompletConstantMission();
            RefreshAllMissionsUI();
        }
    }
}
