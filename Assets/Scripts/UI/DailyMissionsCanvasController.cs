using Unity.Android.Gradle;
using UnityEngine;

public class DailyMissionsCanvasController : CanvasElementLocator
{
    private GameObject _contentScroll;
    void Start()
    {
        _contentScroll = FindAndValidateGameObjectComponent(transform, "QuestsContent");

        for(int i = 1; i < DailyMissionsManager.Instance.GetTodayMissions.Count; i++)
        {
           // Instantiate()
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
