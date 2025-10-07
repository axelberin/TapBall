using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;
using static DailyMissionsManager;

public class DailyMissionsCanvasController : CanvasElementLocator
{
    private GameObject _contentScroll;
    private GameObject _missionRowObjectPrefab;

    //private Image _rewardImage = null;
    //private Image _progressBar = null;
    //private TextMeshProUGUI _missionDescription;
    //private TextMeshProUGUI _missionProgressPercentage;
    //private Button _grantRewardButton;


    void Start()
    {
        _contentScroll = FindAndValidateGameObjectComponent(transform, "QuestsContent");

        AddressablesUtility.LoadAsset<GameObject>("DailyQuestRow", missionAddressable =>
        {
            _missionRowObjectPrefab = missionAddressable;

            for (int i = 0; i < Instance.GetTodayMissions.Count; i++)
            {
                Instantiate(_missionRowObjectPrefab, _contentScroll.transform);
                UpdateUI();

            }
        });

    }

    private void UpdateUI()
    {
        var _rewardImage = FindAndValidateComponent<Image>(transform, "RewardImg");
        var _progressBar = FindAndValidateComponent<Image>(transform, "ProgressBarImage");
        var _missionDescription = FindAndValidateComponent<TextMeshProUGUI>(transform, "QuestText");
        var _missionProgressPercentage = FindAndValidateComponent<TextMeshProUGUI>(transform, "ProgressText");
        var _grantRewardButton = FindAndValidateComponent<Button>(transform, "RewardButton");

        _missionDescription.text = Instance.GetMissionData.missionDescription;
    }
}
