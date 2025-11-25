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
    }

    private void ScheduleComebackNotification(string tittle, string description, int smallIconNum, int largeIconNum, DateTime dateTime)
    {
        var notification = new AndroidNotification
        {
            Title = tittle,
            Text = description,
            SmallIcon = $"smallIcon_{smallIconNum}",
            LargeIcon = $"largeIcon_{largeIconNum}",
            FireTime = dateTime
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);
    }

    private void ClearAllNotifications()
    {
        AndroidNotificationCenter.CancelAllNotifications();
    }
}
