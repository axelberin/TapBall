using System;
using System.Collections;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
#if UNITY_ANDROID
    private const string ChannelId = "default_channel";
#endif
    private bool _scheduledThisSession = false;

    private void Start()
    {
        StartCoroutine(AskForPermissionRequest());

#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "General",
            Description = "Notifications_Multiverse",
            Importance = Importance.Default,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    private IEnumerator AskForPermissionRequest()
    {
#if UNITY_ANDROID
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
            yield return null;

#elif UNITY_IOS
        var req = new AuthorizationRequest(
            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);

        while (!req.IsFinished)
            yield return null;
#endif

        yield break;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false)
            SetNotificationsOnClose();
        else
            _scheduledThisSession = false;
    }

    private void OnDisable()
    {
        SetNotificationsOnClose();
    }

    private void SetNotificationsOnClose()
    {
        if (_scheduledThisSession)
            return;
        _scheduledThisSession = true;

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

    private void ScheduleComebackNotification(string title, string description, int smallIconNum, int largeIconNum, DateTime dateTime)
    {
#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = title,
            Text = description,
            SmallIcon = $"smallicon_{smallIconNum}",
            LargeIcon = $"largeicon_{largeIconNum}",
            FireTime = dateTime
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);

#elif UNITY_IOS
        // iOS: disparo por intervalo (delta desde ahora). Es confiable y simple.
        var now = DateTime.Now;
        var seconds = (dateTime - now).TotalSeconds;

        if (seconds <= 1)
            return;

        var trigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = new TimeSpan(0, 0, (int)Math.Ceiling(seconds)),
            Repeats = false
        };

        var notif = new iOSNotification
        {
            Identifier = Guid.NewGuid().ToString(),
            Title = title,
            Body = description,
            ShowInForeground = true, // útil para test; si no querés que aparezcan en foreground, ponelo en false
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound | PresentationOption.Badge),
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notif);
#endif
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
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
#endif
    }
}
