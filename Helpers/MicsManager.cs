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
using Wavio.Models;
using Android.Preferences;
using Newtonsoft.Json;

namespace Wavio.Helpers
{
    class MicsManager
    {
        public int micsUpdating = 0;
        public bool updatesQueued = false;

        public static MicsManager instance;

        public static List<SavedMic> GetMicsFromPreferences()
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            string savedWavios = prefs.GetString("SavedMics", null);

            var newWavioList = new List<SavedMic>();
            if (string.IsNullOrEmpty(savedWavios))
            {

            }
            else
            {
                try
                {
                    newWavioList = JsonConvert.DeserializeObject<List<SavedMic>>(savedWavios);
                }
                catch
                {
                    Toast.MakeText(Application.Context, "Could not get Mic list.", ToastLength.Short).Show();
                }
            }
            //newWavioList.Add(new WavioListItem() { Name = "Mic Name", WavioId = "WAVIOID", LocalSettings = new Dictionary<string, string>() });
            //newWavioList.Add(new WavioListItem() { Name = "Mic Name2", WavioId = "WAVIOID2", LocalSettings = new Dictionary<string, string>() });
            return newWavioList;
        }

        public static bool AddMicToPreferences(string WavioID, string Name)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            //var wavioId = prefs.GetString("NewWavoID", "");
            //var wavioName = prefs.GetString("NewWavioName", "");
            editor.Remove("NewMicID");
            editor.Remove("NewMicName");
            editor.Apply();

            string storedWaviosJson = prefs.GetString("SavedMics", null);
            var storedWavios = new List<SavedMic>();

            if (!string.IsNullOrEmpty(storedWaviosJson))
            {
                storedWavios = JsonConvert.DeserializeObject<List<SavedMic>>(storedWaviosJson);
            }

            SavedMic newMic = new SavedMic();
            newMic.WavioId = WavioID;
            newMic.Name = Name;
            newMic.LocalSettings = new Dictionary<string, string>();
            newMic.LocalSettings["AlertWhenDisconnected"] = true.ToString();

            storedWavios.Add(newMic);

            string json = JsonConvert.SerializeObject(storedWavios);
            editor.PutString("SavedMics", json);
            editor.Apply();

            //AndHUD.Shared.Dismiss();

            //ViewModel.SelectedItem = newMic;
            //ViewModel.GoToEditWavio();
            return true;
        }

    }
}