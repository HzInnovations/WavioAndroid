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
using static Wavio.Activities.MicSoundsActivity;
using Acr.UserDialogs;
using Android.Support.V4.Content;

namespace Wavio.Fragments
{
    public class MicSettingsFragment : PreferenceFragmentCompat
    {

        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;

        Android.Support.V7.Preferences.PreferenceScreen clearPreferencesButton;
        Android.Support.V7.Preferences.PreferenceScreen changeSensitivityButton;
        Android.Support.V7.Preferences.PreferenceScreen restoreNotificationsButton;
        Android.Support.V7.Preferences.PreferenceScreen deleteMicButton;

        public string micId;
        private IProgressDialog loadingDialog;
        private MicSettingsActivity parent;


        public MicSettingsFragment(string MicId, MicSettingsActivity Parent)
        {
            parent = Parent;
            micId = MicId;
        }
        public MicSettingsFragment()
        {

        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.mic_preferences);
            

            //Preference

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("settings_changed", true);
            editor.PutString("edit_mic_id", micId);
            editor.Apply();

            clearPreferencesButton = FindPreference("preference_mic_button_sounds") as Android.Support.V7.Preferences.PreferenceScreen;
            clearPreferencesButton.PreferenceClick += micSounds;
            changeSensitivityButton = FindPreference("preference_mic_button_sensitivity") as Android.Support.V7.Preferences.PreferenceScreen;
            changeSensitivityButton.PreferenceClick += setSensitivity;
            restoreNotificationsButton = FindPreference("preference_mic_button_restore") as Android.Support.V7.Preferences.PreferenceScreen;
            restoreNotificationsButton.PreferenceClick += restoreNotifications;
            deleteMicButton = FindPreference("preference_mic_button_delete") as Android.Support.V7.Preferences.PreferenceScreen;
            deleteMicButton.PreferenceClick += deleteMic;
            //preference_mic_button_sensitivity

            mRegistrationBroadcastReceiver = new Shared.BroadcastReceiver();
            mRegistrationBroadcastReceiver.Receive += (sender, e) =>
            {
                var reply_error = e.Intent.GetStringExtra("reply_error");
                var reply_data = e.Intent.GetStringExtra("reply_data");

                if (reply_data == Shared.QueuedDeviceRequestType.CHANGE_SENSITIVITY)
                {
                    if (reply_error == Shared.ServerResponsecode.OK.ToString())
                    {
                        if (loadingDialog != null)
                            loadingDialog.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.SuccessToast("Success!");
                    }
                    else
                    {
                        if (loadingDialog != null)
                            loadingDialog.Hide();
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Error Changing Sensitivity");
                    }
                }
            };

            LocalBroadcastManager.GetInstance(base.Activity).RegisterReceiver(mRegistrationBroadcastReceiver,
             new IntentFilter("reply_error"));

        }

        private void deleteMic(object sender, Preference.PreferenceClickEventArgs e)
        {
            RequestDeleteMic();
        }

        public void RequestDeleteMic()
        {

            var progress = Acr.UserDialogs.UserDialogs.Instance.Progress("Deleting mic...");

            string hwid = Android.OS.Build.Serial;

            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");
            

            try
            {
                if (string.IsNullOrEmpty(micId))
                {
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: No ID given!");
                    return;
                }
                if (string.IsNullOrEmpty(gcmID))
                {
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: No GCM ID!");
                    return;
                }

                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.UNREGISTER_MIC.ToString());
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

                    progress.Hide();

                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        var saveSuccess = MicsManager.RemoveMicFromPreferences(micId);
                        if (saveSuccess)
                        {

                        }
                        editor.PutBoolean("settings_changed", true);
                        editor.PutBoolean("mic_added", true);
                        editor.Apply();

                        parent.NavigateUp();
                    }
                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.UNREGISTER_MIC)
                        {
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("Request type mismatch!");
                            return;
                        }
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Unknown error!");
                    }
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

        private void restoreNotifications(object sender, Preference.PreferenceClickEventArgs e)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.Remove(micId + "_LastCheck");
            editor.Apply();
            Acr.UserDialogs.UserDialogs.Instance.SuccessToast("Notifications Restored");
        }

        private void setSensitivity(object sender, Preference.PreferenceClickEventArgs e)
        {
            CheckMicOnline(PostMicCheck.ChangeSensitivity);
        }

        async void SensitivityDialog(int defaultValue)
        {
            var newSensitivityDialog = new PromptConfig();
            newSensitivityDialog.SetInputMode(InputType.Number);
            newSensitivityDialog.Title = "Mic Sensitivity";
            newSensitivityDialog.Message = "Valid values are from 1 to 10";
            newSensitivityDialog.OkText = "Set";
            newSensitivityDialog.CancelText = "Cancel";
            newSensitivityDialog.Placeholder = "5";
            newSensitivityDialog.Text = defaultValue.ToString();
            var micIdResponse = await UserDialogs.Instance.PromptAsync(newSensitivityDialog);
            if (micIdResponse.Ok)
            {
                int newValue = 5;

                var value = micIdResponse.Text;

                if (micIdResponse.Text != "")
                {
                    try
                    {
                        newValue = int.Parse(micIdResponse.Text);
                    }
                    catch
                    {
                        newValue = 0;
                    }
                }
                
                
                if(newValue < 1 || newValue > 10)
                {
                    Acr.UserDialogs.UserDialogs.Instance.ShowError("Please enter a valid number from 1 to 10");
                }
                else
                {
                    RequestSetSensitivity(newValue);
                }
            }

        }

        private void micSounds(object sender, Preference.PreferenceClickEventArgs e)
        {
            //MicSoundsActivity

            var intent = new Intent(base.Context, typeof(MicSoundsActivity));
            intent.PutExtra("id", micId);
            StartActivity(intent);

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
                            if (post == PostMicCheck.ChangeSensitivity)
                            {
                                //SensitivityDialog();
                                RequestGetSensitivity();
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

        private void RequestSetSensitivity(int sensitivity)
        {
            loadingDialog = Acr.UserDialogs.UserDialogs.Instance.Loading("Contacting Mic...");

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

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.SET_SHARED_SETTINGS.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, micId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);

                Dictionary<string, string> newSharedSettings = new Dictionary<string, string>();
                newSharedSettings.Add(WavioSharedSettings.SENSITIVITY, sensitivity.ToString());
                var sharedSettingsJson = JsonConvert.SerializeObject(newSharedSettings);
                parameters.Add(Shared.ParamType.SHARED_SETTINGS, sharedSettingsJson);

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
                        //Acr.UserDialogs.UserDialogs.Instance.SuccessToast("Successfully updated!");
                        loadingDialog = Acr.UserDialogs.UserDialogs.Instance.Loading("Awaiting Response...", OnCancel);
                    }

                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.SET_SHARED_SETTINGS)
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
        private void RequestGetSensitivity()
        {
            loadingDialog = Acr.UserDialogs.UserDialogs.Instance.Loading("Getting Settings...");

            string hwid = Android.OS.Build.Serial;

            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");
            

            try
            {
                if (string.IsNullOrEmpty(micId))
                {
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: No ID given!");
                    return;
                }


                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_SHARED_SETTINGS.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, micId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);

                string requestJson = JsonConvert.SerializeObject(parameters);
                request.AddParameter(Shared.ParamType.REQUEST, requestJson);

                Console.WriteLine("Waiting for response");


                client.ExecuteAsync(request, response => {

                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);

                    loadingDialog.Hide();


                    if (serverResponse == null)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        
                        var sharedSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(serverResponse.data);
                        SensitivityDialog(int.Parse(sharedSettings[Shared.WavioSharedSettings.SENSITIVITY]));
                    }

                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.GET_SHARED_SETTINGS)
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

        private void OnCancel()
        {
            Acr.UserDialogs.UserDialogs.Instance.ShowError("Canceled!");
        }
    }
}