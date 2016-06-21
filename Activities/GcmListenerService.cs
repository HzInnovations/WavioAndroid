using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Gcm;
using Android.Support.V4.Content;
using Android.Util;
using Android.Preferences;
using Wavio.Helpers;
using Newtonsoft.Json.Linq;

namespace Wavio.Activities
{
    [Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
    public class MyGcmListenerService : GcmListenerService
    {

        public override void OnMessageReceived(string from, Bundle data)
        {
            Log.Info("Wavio", "OnMessageReceived");

            var SharedSettings = new Dictionary<String, String>();
            var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();

            try
            {
                //string dataJson = data.GetString("data");
                //ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(data);

                //if (serverResponse == null)
                {
                    // return;
                }

                editor.PutBoolean("new_notif", true);
                editor.Apply();

                string reply_request = data.GetString("request");
                //string reply_result = data.GetString("result");
                string reply_error = data.GetString("error");
                string reply_data = data.GetString("data");


                if (reply_error == Shared.ServerResponsecode.NEW_NOTIFICATION.ToString())
                {
                    JObject soundDetected;
                    try
                    {
                        soundDetected = JObject.Parse(reply_data);
                    }
                    catch
                    {
                        var name = Convert.ToString(reply_data);
                        SendNotification(name, true);
                        return;
                    }
                    

                    string soundName = soundDetected.GetValue("name").ToString();
                    string soundDate = soundDetected.GetValue("date").ToString();
                    string soundFrom = soundDetected.GetValue("from").ToString();
                    //string soundImageUrl = soundDetected.GetValue("imgurl").ToString();
                    var soundSettings = soundDetected.GetValue("settings").ToObject<Dictionary<string, string>>();

                    if (soundSettings != null && soundSettings.ContainsKey("Push") && bool.Parse(soundSettings["Push"]) == true)
                    {
                        if (soundSettings.ContainsKey("Vibrate") && bool.Parse(soundSettings["Vibrate"]) == true)
                        {
                            SendNotification(soundName, true);
                        }
                        else
                        {
                            SendNotification(soundName, false);
                        }
                    }
                    else if (soundSettings != null && soundSettings.ContainsKey("Push") && bool.Parse(soundSettings["Push"]) == false)
                    {

                    }
                    else if (soundSettings == null)
                    {
                        SendNotification(soundName, true);
                    }

                    if (soundSettings != null && soundSettings.ContainsKey("ShowMessage") && bool.Parse(soundSettings["ShowMessage"]) == true)
                    {
                        string lastMessageDate = sharedPreferences.GetString("LastDate", "");

                        bool first = false;
                        if (string.IsNullOrEmpty(lastMessageDate))
                        {
                            var dtnowa = new DateTime(DateTime.Now.Ticks);
                            lastMessageDate = dtnowa.ToString();
                            first = true;
                        }
                        var dtlast = DateTime.Parse(lastMessageDate);
                        var dtnow = new DateTime(DateTime.Now.Ticks);
                        TimeSpan difference = dtnow - dtlast;


                        if (difference.TotalSeconds > 10 || first) //check if it's been at least 10 seconds since last popup to avoid spam/abuse
                        {
                            editor.PutString("LastDate", dtnow.ToString());
                            editor.Apply();
                        }
                        else
                        {
                            int notlongenough = 1;
                        }

                    }

                    var localBroadcast = new Intent("update_notifs");
                    LocalBroadcastManager.GetInstance(this).SendBroadcast(localBroadcast);
                }                
                else //handle all responses here
                {
                    var registrationComplete = new Intent("reply_error");
                    registrationComplete.PutExtra("reply_error", reply_error);
                    LocalBroadcastManager.GetInstance(this).SendBroadcast(registrationComplete);
                }

                Log.Info("OnMessageReceived", "message has no settings.");

                /*
                if (reply_error == Com.ServerResponsecode.DONE_RECORDING.ToString())
                {
                    var registrationComplete = new Intent("addEditSound");
                    registrationComplete.PutExtra("reply_error", reply_error);
                    LocalBroadcastManager.GetInstance(this).SendBroadcast(registrationComplete);
                }
                else if (reply_error == Com.ServerResponsecode.OK.ToString())
                {
                    if ()
                    var registrationComplete = new Intent("addEditSound");
                    registrationComplete.PutExtra("reply_error", reply_result);
                    LocalBroadcastManager.GetInstance(this).SendBroadcast(registrationComplete);
                }
                else if (reply_error == "SOUND_DELETED")
                {
                    var registrationComplete = new Intent("micSounds");
                    registrationComplete.PutExtra("reply_error", reply_result);
                    LocalBroadcastManager.GetInstance(this).SendBroadcast(registrationComplete);
                }
                */

            }
            catch
            {

            }
            //SendNotification(message);
        }


        public void handleMessage()
        {

            var registrationComplete = new Intent("addEditSound");
            LocalBroadcastManager.GetInstance(this).SendBroadcast(registrationComplete);
        }

        void SendNotification(string message, bool vibrate)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            string title = prefs.GetString("notif_title", "Sound Detected");
            title = title.Replace("#SOUND", message);

            string notifBody = prefs.GetString("notif_body", "#SOUND");
            notifBody = notifBody.Replace("#SOUND", message);

            var intent = new Intent(this, typeof(HomeView));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            Notification.Builder notificationBuilder;
            if (vibrate)
            {
                notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.wavio_icon)
                .SetContentTitle(title)
                .SetContentText(notifBody)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetDefaults(NotificationDefaults.Vibrate);

            }
            else
            {
                notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.wavio_icon)
                .SetContentTitle(title)
                .SetContentText(notifBody)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);
            }


            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.Notify(0, notificationBuilder.Build());
        }




    }
}