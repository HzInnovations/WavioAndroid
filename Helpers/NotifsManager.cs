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
using Android.Preferences;
using Wavio.Models;
using Newtonsoft.Json;

namespace Wavio.Helpers
{
    class NotifsManager
    {

        public static List<Notif> GetSavedNotifications(string micId)
        {
            //ViewModel.Notifications = new List<SavedNotification>();

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            string savedMicsString = prefs.GetString("SavedMics", null);
            var savedMics = new List<SavedMic>();
            var savedNotifications = new List<Notif>();

            if (string.IsNullOrEmpty(savedMicsString))
            {
                //we have no mics
            }
            else
            {
                try
                {
                    savedMics = JsonConvert.DeserializeObject<List<SavedMic>>(savedMicsString);
                }
                catch
                {
                    //Toast.MakeText(this, "Error deserializing list!", ToastLength.Short).Show();
                }
            }

            for (int i = 0; i < savedMics.Count; i++)
            {
                if (savedMics[i].WavioId == micId)
                {
                    string micNotifications = prefs.GetString(savedMics[i].WavioId + "_Notifs", "");

                    /*
                    if (!string.IsNullOrEmpty(micNotifications) && micNotifications == "CLEAR")
                    {
                        savedNotifications = new List<SavedNotif>();
                        editor.Remove(storedWavios[i].WavioId + "_Notifs");
                        editor.Apply();
                    }else
                    */
                    if (!string.IsNullOrEmpty(micNotifications))
                    {

                        savedNotifications = JsonConvert.DeserializeObject<List<Notif>>(micNotifications);
                        //ViewModel.Notifications.AddRange(savedNotifications);
                        //ViewModel.Notifications = ViewModel.Notifications;
                    }
                }

                if (micId == "All")
                {
                    string micNotifications = prefs.GetString(savedMics[i].WavioId + "_Notifs", "");

                    /*
                    if (!string.IsNullOrEmpty(micNotifications) && micNotifications == "CLEAR")
                    {
                        savedNotifications = new List<SavedNotif>();
                        editor.Remove(storedWavios[i].WavioId + "_Notifs");
                        editor.Apply();
                    }
                    else
                    */if (!string.IsNullOrEmpty(micNotifications))
                    {

                        var newNotifs = JsonConvert.DeserializeObject<List<Notif>>(micNotifications);
                        savedNotifications.AddRange(newNotifs);
                        //ViewModel.Notifications.AddRange(savedNotifications);
                        //ViewModel.Notifications = ViewModel.Notifications;
                    }
                }
            }
            
            return savedNotifications;
        }

        public static bool AddNewNotifs(string MicID, List<Notif> newNotifs)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            var allNotifs = GetSavedNotifications(MicID);

            allNotifs.AddRange(newNotifs);

            var notifsString = JsonConvert.SerializeObject(allNotifs);

            editor.PutString(MicID + "_Notifs", notifsString);
            editor.PutString(MicID + "_LastCheck", DateTime.Now.ToString());
            editor.Apply();

            return true;
        }

        public static bool ClearNotifs(string MicID)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            if (MicID != "All")
            {
                editor.PutString(MicID + "_Notifs", JsonConvert.SerializeObject(new List<Notif>()));
                editor.Apply();
            }
            else
            {
                var mics = MicsManager.GetMicsFromPreferences();
                for (int i = 0; i < mics.Count; i++)
                {
                    editor.PutString(mics[i].WavioId + "_Notifs", JsonConvert.SerializeObject(new List<Notif>()));
                }
                editor.Apply();
            }
            return true;
        }
    }
}