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
            ScheduleComebackNotification(DateTime.Now.AddMinutes(1));
    }

    private void OnDisable()
    {
        ScheduleComebackNotification(DateTime.Now.AddMinutes(1));
    }

    private void ScheduleComebackNotification(DateTime dateTime)
    {
        AndroidNotificationCenter.CancelAllNotifications();

        var notification = new AndroidNotification
        {
            Title = "¡Volvé al Multiverso!",
            Text = "Tenés desafíos esperando y recompensas por conseguir.",
            SmallIcon = "icon_0",
            LargeIcon = "icon_1",
            FireTime = dateTime
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);
    }
}
