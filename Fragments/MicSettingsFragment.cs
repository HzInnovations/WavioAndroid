using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Preferences;
using Android.Support.V7.App;
using Android.Gms.Gcm.Iid;
using System.IO;
using RestSharp;
using Wavio.Helpers;
using Newtonsoft.Json;
using static Wavio.Helpers.Shared;
using System.Net;
using Org.Json;
using Wavio.Activities;

namespace Wavio.Fragments
{
    public class MicSettingsFragment : PreferenceFragmentCompat
    {
        Android.Support.V7.Preferences.PreferenceScreen clearPreferencesButton;

        public string micId;
        
        
        public MicSettingsFragment(string MicId)
        {
            micId = MicId;
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.mic_preferences);
            

            //Preference

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("settings_changed", true);
            editor.Apply();

            clearPreferencesButton = FindPreference("preference_mic_button_sounds") as Android.Support.V7.Preferences.PreferenceScreen;
            clearPreferencesButton.PreferenceClick += micSounds;

        }

        private void micSounds(object sender, Preference.PreferenceClickEventArgs e)
        {
            //MicSoundsActivity

            var intent = new Intent(base.Context, typeof(MicSoundsActivity));
            StartActivity(intent);

        }
    }
}