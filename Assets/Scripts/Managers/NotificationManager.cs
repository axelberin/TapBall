using System;
using System.Collections;
using Unity.Notifications.Android;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private const string ChannelId = "default_channel";

    private void Start()
    {
        StartCoroutine(AskForPermissionRequest());

        var channel = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "General",
            Description = "Notifications_Multiverse",
            Importance = Importance.Default,
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    private IEnumerator AskForPermissionRequest()
    {
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
            yield return null;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false)
            SetNotificationsOnClose();
    }

    private void OnDisable()
    {
        SetNotificationsOnClose();
    }

    private void SetNotificationsOnClose()
    {
        if (LanguageManager.Instance == null)
            return;

        int r = UnityEngine.Random.Range(0, 2);

        string title = LanguageManager.Instance.GetLocalizedText($"notification_tittle_comeback_{r + 1}");
        string desc = LanguageManager.Instance.GetLocalizedText($"notification_description_comeback_{r + 1}");

        int smallIcon = 0;
        int largeIcon = r;

        if (title == null || desc == null)
        {
            title = LanguageManager.Instance.GetLocalizedText("notification_tittle_comeback_1");
            desc = LanguageManager.Instance.GetLocalizedText("notification_description_comeback_1");
            largeIcon = 0;
        }

        ClearAllNotifications();
        ScheduleComebackNotification(title, desc, smallIcon, largeIcon, DateTime.Now.AddHours(2));
        ScheduleComebackNotification(title, desc, smallIcon, largeIcon, DateTime.Now.AddHours(4));
        ScheduleComebackNotification(title, desc, smallIcon, largeIcon, DateTime.Now.AddHours(8));

        ScheduleNotificationAtTime("notification_tittle_daily_1", "notification_description_daily_1",
            0, 0, 11, 0);
        if (DailyMissionsManager.Instance && !DailyMissionsManager.Instance.AreAllTodayMissionsComplete)
            ScheduleNotificationAtTime("notification_tittle_daily_2", "notification_description_daily_2",
                0, 0, 20, 0);

        ScheduleComebackNotification(LanguageManager.Instance.GetLocalizedText("notification_tittle_christmas_1"),
            LanguageManager.Instance.GetLocalizedText("notification_description_christmas_1"),
            0, 2, new(DateTime.Now.Year, 12, 23, 11, 0, 0)); //Rudolph
        ScheduleComebackNotification(LanguageManager.Instance.GetLocalizedText("notification_tittle_christmas_1"), 
            LanguageManager.Instance.GetLocalizedText("notification_description_christmas_1"),
            0, 3, new(DateTime.Now.Year, 12, 24, 11, 0, 0)); //Santa
        ScheduleComebackNotification(LanguageManager.Instance.GetLocalizedText("notification_tittle_christmas_1"), 
            LanguageManager.Instance.GetLocalizedText("notification_description_christmas_1"),
            0, 4, new(DateTime.Now.Year, 12, 26, 11, 0, 0)); //Grinch
    }

    private void ScheduleComebackNotification(string tittle, string description, int smallIconNum, int largeIconNum, DateTime dateTime)
    {
        var notification = new AndroidNotification
        {
            Title = tittle,
            Text = description,
            SmallIcon = $"smallicon_{smallIconNum}",
            LargeIcon = $"largeicon_{largeIconNum}",
            FireTime = dateTime
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);
    }

    private void ScheduleNotificationAtTime(
    string titleKey,
    string descKey,
    int smallIconNum,
    int largeIconNum,
    int hour,
    int minute)
    {
        string title = LanguageManager.Instance.GetLocalizedText(titleKey);
        string desc = LanguageManager.Instance.GetLocalizedText(descKey);

        if (title == null || desc == null)
        {
            Debug.LogWarning($"Notification keys not found: {titleKey} / {descKey}");
            return;
        }

        DateTime now = DateTime.Now;
        DateTime fireTime = new(now.Year, now.Month, now.Day, hour, minute, 0);

        // Si ya pasó ese horario hoy, se programa para mañana
        if (fireTime <= now)
            fireTime = fireTime.AddDays(1);

        ScheduleComebackNotification(title, desc, smallIconNum, largeIconNum, fireTime);
    }


    private void ClearAllNotifications()
    {
        AndroidNotificationCenter.CancelAllNotifications();
    }
}
