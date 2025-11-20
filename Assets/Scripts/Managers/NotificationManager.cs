using System;
using Unity.Notifications.Android;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private const string ChannelId = "default_channel";

    private void Start()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "General",
            Importance = Importance.High,
            Description = "Notificaciones de Multiverse Tap Ball"
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            ScheduleComebackNotification();
        }
    }

    private void OnApplicationQuit()
    {
        ScheduleComebackNotification();
    }

    private void ScheduleComebackNotification()
    {
        AndroidNotificationCenter.CancelAllNotifications();

        var notification = new AndroidNotification
        {
            Title = "¡Volvé al Multiverso!",
            Text = "Tenés desafíos esperando y recompensas por conseguir.",
            SmallIcon = "icon_0",
            LargeIcon = "icon_1",
            FireTime = DateTime.Now.AddHours(24)
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);
    }
}
