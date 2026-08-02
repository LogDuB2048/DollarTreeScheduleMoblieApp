using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using DollarTreeApp2;
using Firebase.Messaging;


[Service(Exported = true)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService
{

    private const string CHANNEL_ID = "local_notification_channel";
    private const int NOTIFICATION_ID = 1001;
    public override void OnMessageReceived(RemoteMessage message)
    {
        // Handle the message here
        var body = message?.GetNotification()?.Body;
        if (body.Length < 1)
            body = "";
        var data = message?.Data;

        CreateNotificationChannel();
        ShowNotification(body);

    }

    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
    }
    private void ShowNotification(string tempBody)
    {
        // This defines what happens when the user clicks the notification
        Intent intent = new Intent(this, typeof(MainActivity));
        PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable)!;
        
        // Use the native Android NotificationCompat Builder
        NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
            .SetContentTitle("Schedule Update")
            .SetContentText("These days have been updated: \n" + tempBody)
            .SetSmallIcon(DollarTreeApp2.Resource.Mipmap.appicon) // System launcher icon
            .SetContentIntent(pendingIntent)
            .SetStyle(new NotificationCompat.BigTextStyle() 
                    .BigText("These days have been updated: \n" + tempBody))
            .SetAutoCancel(true); // Dismisses the notification when clicked

        // Issue the notification via system services
        NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
        notificationManager.Notify(NOTIFICATION_ID, builder.Build());
    }
    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var name = "Local Notifications";
            var description = "Channel used for local C# application alerts";
            var importance = NotificationImportance.Default;

            NotificationChannel channel = new NotificationChannel(CHANNEL_ID, name, importance)
            {
                Description = description
            };

            // Register the channel with the Android system
            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}