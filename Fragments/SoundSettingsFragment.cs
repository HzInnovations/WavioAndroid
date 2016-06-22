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
using Wavio.Models;

namespace Wavio.Fragments
{
    public class SoundSettingsFragment : PreferenceFragmentCompat
    {
        Android.Support.V7.Preferences.PreferenceScreen deleteSoundButton;
        Android.Support.V7.Preferences.PreferenceScreen changeIconButton;

        Android.Support.V7.Preferences.CheckBoxPreference soundPushCheck;
        Android.Support.V7.Preferences.CheckBoxPreference soundVibrateCheck;
        Android.Support.V7.Preferences.CheckBoxPreference soundshowMessageCheck;

        public string micId;
        public MicSound sound;


        public SoundSettingsFragment(string MicId, MicSound Sound)
        {
            micId = MicId;
            sound = Sound;
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.sound_preferences);
            
            if (sound.sound_settings == null)
            {
                sound.sound_settings = new Dictionary<string, string>();
            }
            //Preference

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            soundPushCheck = FindPreference("sound_push") as Android.Support.V7.Preferences.CheckBoxPreference;
            soundPushCheck.PreferenceClick += settingsClicked;
            if (sound.sound_settings.ContainsKey("Push"))
            {
                soundPushCheck.Checked = Boolean.Parse(sound.sound_settings["Push"]);
            }
            else
            {
                sound.sound_settings.Add("Push", true.ToString());
                soundPushCheck.Checked = true;
            }

            soundVibrateCheck = FindPreference("sound_vibrate") as Android.Support.V7.Preferences.CheckBoxPreference;
            soundVibrateCheck.PreferenceClick += settingsClicked;
            if (sound.sound_settings.ContainsKey("Vibrate"))
            {
                soundVibrateCheck.Checked = Boolean.Parse(sound.sound_settings["Vibrate"]);
            }
            else
            {
                sound.sound_settings.Add("Vibrate", true.ToString());
                soundVibrateCheck.Checked = true;
            }

            soundshowMessageCheck = FindPreference("sound_open") as Android.Support.V7.Preferences.CheckBoxPreference;
            soundshowMessageCheck.PreferenceClick += settingsClicked;
            if (sound.sound_settings.ContainsKey("ShowMessage"))
            {
                soundshowMessageCheck.Checked = Boolean.Parse(sound.sound_settings["ShowMessage"]);
            }
            else
            {
                sound.sound_settings.Add("ShowMessage", false.ToString());
                soundshowMessageCheck.Checked = false;
            }

            deleteSoundButton = FindPreference("sound_delete") as Android.Support.V7.Preferences.PreferenceScreen;
            deleteSoundButton.PreferenceClick += deleteSound;

            changeIconButton = FindPreference("sound_icon") as Android.Support.V7.Preferences.PreferenceScreen;
            changeIconButton.PreferenceClick += changeIcon;

        }

        private void settingsClicked(object sender, Preference.PreferenceClickEventArgs e)
        {
            UpdateSettings();
        }

        

        private void changeIcon(object sender, Preference.PreferenceClickEventArgs e)
        {

        }

        private void deleteSound(object sender, Preference.PreferenceClickEventArgs e)
        {

        }

        private void UpdateSettings()
        {
            sound.sound_settings["Push"] = soundPushCheck.Checked.ToString();
            sound.sound_settings["Vibrate"] = soundPushCheck.Checked.ToString();
            sound.sound_settings["ShowMessage"] = soundPushCheck.Checked.ToString();
        }
    }
}