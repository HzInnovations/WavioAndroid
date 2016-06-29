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

namespace Wavio.Fragments
{
    public class SettingsFragment : PreferenceFragmentCompat
    {
        Android.Support.V7.Preferences.PreferenceScreen clearPreferencesButton;
        Android.Support.V7.Preferences.PreferenceScreen instanceButton;
        Android.Support.V7.Preferences.PreferenceScreen testNotifButton;
        Android.Support.V7.Preferences.PreferenceScreen testButton1;
        

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.preferences2);
            

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("settings_changed", true);
            editor.Apply();

            clearPreferencesButton = FindPreference("button_clear") as Android.Support.V7.Preferences.PreferenceScreen;
            clearPreferencesButton.PreferenceClick += clearPreferences;

            instanceButton = FindPreference("button_instance") as Android.Support.V7.Preferences.PreferenceScreen;
            instanceButton.PreferenceClick += makeInstance;

            testNotifButton = FindPreference("button_test_notif") as Android.Support.V7.Preferences.PreferenceScreen;
            testNotifButton.PreferenceClick += RequestTestNotif;

            //testButton1 = FindPreference("button_test1") as Android.Support.V7.Preferences.PreferenceScreen;
            //testButton1.PreferenceClick += runTestButton1;
        }

        private void RequestTestNotif(object sender, Preference.PreferenceClickEventArgs e)
        {
            base.Context.SendBroadcast(new Intent("com.google.android.intent.action.GTALK_HEARTBEAT"));
            base.Context.SendBroadcast(new Intent("com.google.android.intent.action.MCS_HEARTBEAT"));

            //var progress = Acr.UserDialogs.UserDialogs.Instance.Progress("Requesting test notification...");
            var progressDialog = Android.App.ProgressDialog.Show(base.Context, "Please wait...", "Requesting test notification...", true);
            try
            {

                var prefs = PreferenceManager.GetDefaultSharedPreferences(base.Context);
                ISharedPreferencesEditor editor = prefs.Edit();
                String gcmID = prefs.GetString("GCMID", "");
                string hwid = Android.OS.Build.Serial;

                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.REQUEST_TEST_NOTIF.ToString());
                //parameters.Add(Shared.ParamType.WAVIO_ID, wavioId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);
                string requestJson = JsonConvert.SerializeObject(parameters);
                request.AddParameter(Shared.ParamType.REQUEST, requestJson);
                

                client.ExecuteAsync(request, response => {

                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response.Content);

                    if (serverResponse == null)
                    {
                        if (progressDialog != null)
                            progressDialog.Dismiss();
                        //AndHUD.Shared.ShowError(_context, "Network error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        if (progressDialog != null)
                            progressDialog.Dismiss();

                        var gcmResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(serverResponse.data);

                        //var gcmResponse = JsonConvert.DeserializeAnonymousType(serverResponse.data);
                        int failures = Convert.ToInt32(gcmResponse["failure"]);
                        if (failures > 0)
                        {
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("GCM Error.");
                        }
                    }
                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        if (progressDialog != null)
                            progressDialog.Dismiss();
                        //AndHUD.Shared.ShowError(_context, "Server error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.REQUEST_TEST_NOTIF)
                        {
                            if (progressDialog != null)
                                progressDialog.Dismiss();
                            //AndHUD.Shared.ShowError(_context, "Request type mismatch!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                            Acr.UserDialogs.UserDialogs.Instance.ShowError("Request type mismatch!");
                            return;
                        }
                        if (progressDialog != null)
                            progressDialog.Dismiss();
                        //AndHUD.Shared.ShowError(_context, "Unknown error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Unknown error!");
                    }
                    return;

                });

            }
            catch (WebException ex)
            {
                string _exception = ex.ToString();
                if (progressDialog != null)
                    progressDialog.Dismiss();
                //AndHUD.Shared.ShowError(_context, "Error: " + ex.ToString(), AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                Console.WriteLine("--->" + _exception);
            }

            //progress.Dispose();
            //   Acr.UserDialogs.UserDialogs.Instance.ShowSuccess();
        }
        
        private void makeInstance(object sender, Preference.PreferenceClickEventArgs e)
        {
            var prefs = PreferenceManager.SharedPreferences;
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.Clear(); //Old mics wont work if GCM changes
            editor.Apply();


            editor.PutBoolean("clear_instanceid", true);
            editor.Apply();
            Acr.UserDialogs.UserDialogs.Instance.ShowSuccess("Add a mic to complete the process.");
        }

        private void clearPreferences(object sender, Android.Support.V7.Preferences.Preference.PreferenceClickEventArgs e)
       {
           var prefs = PreferenceManager.SharedPreferences;
           ISharedPreferencesEditor editor = prefs.Edit();
           editor.Clear();
           editor.Apply();
           Acr.UserDialogs.UserDialogs.Instance.ShowSuccess("Preferences cleared!");
       }
       
    }
}