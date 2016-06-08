using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using Wavio.Adapters;
using Wavio.Models;
using Android.Support.V7.App;
using Com.Nostra13.Universalimageloader.Core;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Wavio.Helpers;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using Android.Preferences;
using Android.Support.V4.Content;
using RestSharp;
using Newtonsoft.Json;
using Android.Gms.Gcm.Iid;
using static Wavio.Helpers.Shared;
using System.Net;

namespace Wavio.Activities
{
    [Activity(Label = "Microphones",ParentActivity = typeof(HomeView))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "wavio.activities.HomeView")]
	public class MicsActivity : AppCompatActivity
    {
        List<SavedMic> mics;
        ListView listView;
        ImageLoader imageLoader;
        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;
        private bool waitingForGCM = false;

        Android.App.ProgressDialog progressDialog;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mics_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);           
            SetContentView(Resource.Layout.page_mics);
            imageLoader = ImageLoader.Instance;					

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);
            
            SupportActionBar.SetDisplayHomeAsUpEnabled (true);

            mics = MicsManager.GetMicsFromPreferences();

          

            mRegistrationBroadcastReceiver = new Shared.BroadcastReceiver();
            mRegistrationBroadcastReceiver.Receive += (sender, e) =>
            {
                //progressDialog.Dismiss();
                var result = e.Intent.GetBooleanExtra("gcm_success", false);
                if (result)
                {
                    if (waitingForGCM)
                    {
                        waitingForGCM = false;
                        SubmitNewMic();
                    }
                }
                
            };

            LocalBroadcastManager.GetInstance(this).RegisterReceiver(mRegistrationBroadcastReceiver,
              new IntentFilter("registrationComplete"));

            listView = FindViewById<ListView>(Resource.Id.micsListView);
            listView.ItemClick += OnListItemClick;
            listView.Adapter = new MicAdapter(this, mics);

            //var grid = FindViewById<GridView>(Resource.Id.grid);
            //grid.Adapter = new MonkeyAdapter(this, friends);
            //grid.ItemClick += GridOnItemClick;

        }

        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //Toast.MakeText(this, "Item " + e.Position, ToastLength.Short).Show();
            //throw new NotImplementedException();
            var intent = new Intent(this, typeof(MicSettingsActivity));
            intent.PutExtra("mic_id", MicsManager.GetMicsFromPreferences()[(int)e.Id].WavioId);
            StartActivity(intent);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)           
            {
                case Android.Resource.Id.Home:
					NavUtils.NavigateUpFromSameTask(this);
                    break;
                case Resource.Id.new_mic:
                    AddMic();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

     
        private void AddMic()
        {
            NewMicDialog();
        }

        async void NewMicDialog()
        {
            var newMicIdDialog = new PromptConfig();
            newMicIdDialog.SetInputMode(InputType.Default);
            newMicIdDialog.Title = "Add a new mic";
            newMicIdDialog.OkText = "NEXT";
            newMicIdDialog.CancelText = "CANCEL";
            newMicIdDialog.Placeholder = "Mic ID";
            var micIdResponse = await UserDialogs.Instance.PromptAsync(newMicIdDialog);
            if (micIdResponse.Ok)
            {
                Regex r = new Regex("^[a-zA-Z0-9]*$");
                if (!r.IsMatch(micIdResponse.Text) || micIdResponse.Text == "")
                {
                    UserDialogs.Instance.Alert("Please enter a valid Mic ID!");
                }
                else
                {
                    var newMicNameDialog = new PromptConfig();
                    newMicNameDialog.SetInputMode(InputType.Name);
                    newMicIdDialog.Title = "Would you like to name your mic?";
                    newMicIdDialog.OkText = "ADD";
                    newMicIdDialog.CancelText = "CANCEL";
                    newMicIdDialog.Placeholder = "Mic Name (Optional)";
                    var micNameResponse = await UserDialogs.Instance.PromptAsync(newMicIdDialog);
                    if (micNameResponse.Ok)
                    {
                        AddNewMic(micIdResponse.Text, micNameResponse.Text);
                    }
                }
            }
        }

        private void AddNewMic(string WavioID, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = WavioID;
            }
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");
            editor.PutString("NewWavoID", WavioID);
            editor.PutString("NewWavioName", name);
            editor.Apply();
            if (string.IsNullOrEmpty(gcmID))
            {                
                RegisterGCMID();
            }
            else
            {
                SubmitNewMic();
                //RegisterGCMID();
            }
        }

        public void RegisterGCMID()
        {
            //AndHUD.Shared.Show(_context, "Registering...", (int)AndroidHUD.MaskType.Clear);
            progressDialog = Android.App.ProgressDialog.Show(this, "Please wait...", "Registering gcm...", true);
            if (true)
            {
                waitingForGCM = true;
                var intent = new Intent(this, typeof(RegistrationIntentService));
                StartService(intent);
            }
        }

        public void SubmitNewMic()
        {
            try
            {
                progressDialog = Android.App.ProgressDialog.Show(this, "Please wait...", "Registering mic...", true);
            }
            catch
            {
                //Acr.UserDialogs.UserDialogs.Instance.Progress("Please wait...");
            }
            

            string hwid = Android.OS.Build.Serial;

            //AndHUD.Shared.Show(_context, "Adding Mic...", (int)AndroidHUD.MaskType.Clear);
            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");
            var wavioId = prefs.GetString("NewWavoID", "");
            var wavioName = prefs.GetString("NewWavioName", "");
            
            try
            {
                if (string.IsNullOrEmpty(wavioId))
                {
                    if (progressDialog != null)
                        progressDialog.Dismiss();
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: No ID given!");
                    //AndHUD.Shared.ShowError(_context, "Error: No ID given!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                    return;
                }
                if (string.IsNullOrEmpty(gcmID))
                {
                    if (progressDialog != null)
                        progressDialog.Dismiss();
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: No GCM ID!");
                    //AndHUD.Shared.ShowError(_context, "Error: No GCM ID!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                    return;
                }

                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.REGISTER_MIC.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, wavioId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);
                string requestJson = JsonConvert.SerializeObject(parameters);
                request.AddParameter(Shared.ParamType.REQUEST, requestJson);

                Console.WriteLine("Waiting for response");


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
                        //AndHUD.Shared.ShowSuccess(_context, "Added!", AndroidHUD.MaskType.Clear, TimeSpan.FromSeconds(2));                        
                        var saveSuccess = MicsManager.AddMicToPreferences(wavioId, wavioName);
                        if (saveSuccess)
                        {

                        }
                        //Acr.UserDialogs.UserDialogs.Instance.ShowSuccess("Added!");
                        editor.PutBoolean("settings_changed", true);

                        editor.PutBoolean("mic_added", true);
                        editor.Apply();

                        NavUtils.NavigateUpFromSameTask(this);

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
                        if (serverResponse.request != Shared.RequestCode.REGISTER_MIC)
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
        }

        public void OnResume()
        {
            base.OnResume(); // Always call the superclass first.
            mics = new List<SavedMic>();
            mics = MicsManager.GetMicsFromPreferences();
            //refresh listview?
        }



        
    }
}