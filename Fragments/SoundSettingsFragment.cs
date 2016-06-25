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
using static Wavio.Activities.MicSoundsActivity;
using Android.Support.V4.Content;

namespace Wavio.Fragments
{
    public class SoundSettingsFragment : PreferenceFragmentCompat
    {
        Android.Support.V7.Preferences.PreferenceScreen deleteSoundButton;
        Android.Support.V7.Preferences.PreferenceScreen changeIconButton;

        Android.Support.V7.Preferences.CheckBoxPreference soundPushCheck;
        Android.Support.V7.Preferences.CheckBoxPreference soundVibrateCheck;
        Android.Support.V7.Preferences.CheckBoxPreference soundshowMessageCheck;


        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;

        public string micId;
        public MicSound sound;
        SoundSettingsActivity parent;

        private Acr.UserDialogs.IProgressDialog loading;

        public SoundSettingsFragment(string MicId, MicSound Sound, SoundSettingsActivity Parent)
        {
            micId = MicId;
            sound = Sound;
            parent = Parent;
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.sound_preferences);


            mRegistrationBroadcastReceiver = new Shared.BroadcastReceiver();
            mRegistrationBroadcastReceiver.Receive += (sender, e) =>
            {
                var reply_error = e.Intent.GetStringExtra("reply_error");
                var reply_data = e.Intent.GetStringExtra("reply_data");

                if (reply_data == Shared.QueuedDeviceRequestType.NEW_SOUND)
                {
                    if (reply_error == Shared.ServerResponsecode.OK.ToString())
                    {
                        if (loading != null)
                            loading.Hide();
                        var intent = new Intent(parent, typeof(MicSoundsActivity));
                        parent.NavigateUpTo(intent);
                    }
                    else
                    {
                        if (loading != null)
                            loading.Hide();
                        var intent = new Intent(parent, typeof(MicSoundsActivity));
                        parent.NavigateUpTo(intent);
                    }                    
                }
                if (reply_data == Shared.QueuedDeviceRequestType.DELETE_SOUND)
                {
                    if (reply_error == Shared.ServerResponsecode.OK.ToString())
                    {
                        if (loading != null)
                            loading.Hide();
                        var intent = new Intent(parent, typeof(MicSoundsActivity));
                        parent.NavigateUpTo(intent);
                    }
                    else
                    {
                        if (loading != null)
                            loading.Hide();
                        var intent = new Intent(parent, typeof(MicSoundsActivity));
                        parent.NavigateUpTo(intent);
                    }
                }
            };

            LocalBroadcastManager.GetInstance(base.Activity).RegisterReceiver(mRegistrationBroadcastReceiver,
              new IntentFilter("reply_error"));



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
            if (soundPushCheck.Checked == false)
            {
                soundVibrateCheck.Checked = false;
            }
            UpdateSettings();
        }

        

        private void changeIcon(object sender, Preference.PreferenceClickEventArgs e)
        {

        }

        private void deleteSound(object sender, Preference.PreferenceClickEventArgs e)
        {
            CheckMicOnline(PostMicCheck.DeleteSound);
        }

        private void RequestDeleteSound()
        {
            loading = Acr.UserDialogs.UserDialogs.Instance.Loading("Deleting Sound. Sending Request...");

            string hwid = Android.OS.Build.Serial;
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            var soundInfo = JsonConvert.SerializeObject(sound);

            try
            {
                if (string.IsNullOrEmpty(micId))
                {
                    loading.Hide();
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: Missing ID");
                    return;
                }


                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.DELETE_SOUND.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, micId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);
                parameters.Add(Shared.ParamType.SOUND_NAME, sound.sound_name);
                string requestJson = JsonConvert.SerializeObject(parameters);
                request.AddParameter(Shared.ParamType.REQUEST, requestJson);

                Console.WriteLine("Waiting for response");


                client.ExecuteAsync(request, response => {

                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);

                    if (serverResponse == null)
                    {
                        loading.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        //loading.Hide();
                        //Acr.UserDialogs.UserDialogs.Instance.ShowSuccess("Updated Successfully");
                        loading = Acr.UserDialogs.UserDialogs.Instance.Loading("Deleting Sound. Waiting For Mic...", RequestCancel);
                        //var intent = new Intent(parent, typeof(MicSoundsActivity));
                        //parent.NavigateUpTo(intent);
                    }

                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        loading.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.EDIT_SOUND)
                        {
                            loading.Hide();
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("Request type mismatch!");
                            return;
                        }
                        loading.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Unknown error!");
                    }
                    return;

                });

            }
            catch (WebException ex)
            {
                string _exception = ex.ToString();
                loading.Hide();
                Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                Console.WriteLine("--->" + _exception);
            }
        }

        private void RequestCancel()
        {
            var intent = new Intent(parent, typeof(MicSoundsActivity));
            parent.NavigateUpTo(intent);
        }

        private void UpdateSettings()
        {
            sound.sound_settings["Push"] = soundPushCheck.Checked.ToString();
            sound.sound_settings["Vibrate"] = soundVibrateCheck.Checked.ToString();
            sound.sound_settings["ShowMessage"] = soundshowMessageCheck.Checked.ToString();


            var loading = Acr.UserDialogs.UserDialogs.Instance.Loading("Updating Settings...");

            string hwid = Android.OS.Build.Serial;
            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            var soundInfo = JsonConvert.SerializeObject(sound);

            try
            {
                if (string.IsNullOrEmpty(micId))
                {
                    loading.Hide();
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: Missing ID");
                    return;
                }


                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.EDIT_SOUND.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, micId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);
                parameters.Add(Shared.ParamType.SOUND_INFO, soundInfo);
                string requestJson = JsonConvert.SerializeObject(parameters);
                request.AddParameter(Shared.ParamType.REQUEST, requestJson);

                Console.WriteLine("Waiting for response");


                client.ExecuteAsync(request, response => {

                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);

                    if (serverResponse == null)
                    {
                        loading.Hide();
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        loading.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowSuccess("Updated Successfully");
                    }

                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        loading.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.EDIT_SOUND)
                        {
                            loading.Hide();
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("Request type mismatch!");
                            return;
                        }
                        loading.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Unknown error!");
                    }
                    return;

                });

            }
            catch (WebException ex)
            {
                string _exception = ex.ToString();
                loading.Hide();
                Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                Console.WriteLine("--->" + _exception);
            }
        }

        private void CheckMicOnline(PostMicCheck post)
        {
            Acr.UserDialogs.UserDialogs.Instance.Loading("Checking mic status...");

            string hwid = Android.OS.Build.Serial;

            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            var micTimeout = float.Parse(prefs.GetString("mic_timeout", "10000"));

            try
            {
                if (string.IsNullOrEmpty(micId))
                {
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: No ID given!");
                    //AndHUD.Shared.ShowError(_context, "Error: No ID given!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                    return;
                }


                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_LAST_ALIVE.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, micId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);
                string requestJson = JsonConvert.SerializeObject(parameters);
                request.AddParameter(Shared.ParamType.REQUEST, requestJson);

                Console.WriteLine("Waiting for response");


                client.ExecuteAsync(request, response => {

                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);

                    if (serverResponse == null)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        double nowInMs = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        double micTime = double.Parse(serverResponse.data);
                        double difference = nowInMs - micTime;
                        if (Math.Abs(difference) < micTimeout) //time travel and whatnot
                        {
                            if (post == PostMicCheck.AddSound)
                            {

                            }
                            else if (post == PostMicCheck.DeleteSound)
                            {
                                RequestDeleteSound();
                            }
                            else if (post == PostMicCheck.EditSound)
                            {

                            }
                        }
                        else
                        {
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("Mic is offline!");
                        }


                    }
                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.GET_LAST_ALIVE)
                        {
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("Request type mismatch!");
                            return;
                        }
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Unknown error!");
                    }
                    Acr.UserDialogs.UserDialogs.Instance.HideLoading();
                    return;

                });

            }
            catch (WebException ex)
            {
                string _exception = ex.ToString();
                Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                Console.WriteLine("--->" + _exception);
            }
        }
    }
}