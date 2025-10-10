using System;
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
    }

    private void UpdateUI(Transform rowTransform, int missionIndex)
    {
        var mission = Instance.GetTodayMissions[missionIndex];

        var rewardImage = FindAndValidateComponent<Image>(rowTransform, "RewardImg");
        var progressBar = FindAndValidateComponent<Image>(rowTransform, "ProgressBarImage");
        var missionDescription = FindAndValidateComponent<TextMeshProUGUI>(rowTransform, "QuestText");
        var missionProgressPercentage = FindAndValidateComponent<TextMeshProUGUI>(rowTransform, "ProgressText");
        var grantRewardButton = FindAndValidateComponent<Button>(rowTransform, "RewardButton");

        var missionPercentage = mission.currentProgress / mission.objectiveAmount;

        missionDescription.text = mission.missionDescription;
        progressBar.fillAmount = missionPercentage;
        missionProgressPercentage.text = MathF.Truncate(missionPercentage * 100).ToString() + "%";

        AddressablesUtility.LoadAsset<GameObject>($"{mission.rewardType}Image", rewardImageAddressable =>//Subir después el addressable
        {
            rewardImage = rewardImageAddressable.GetComponent<Image>();
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
}
