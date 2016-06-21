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
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using static Wavio.Helpers.Shared;
using Wavio.Helpers;
using Android.Support.V4.Content;
using Wavio.Adapters;
using Wavio.Models;
using Android.Preferences;
using RestSharp;
using Newtonsoft.Json;
using System.Net;
using Android.Support.V4.App;
using Acr.UserDialogs;
using System.Text.RegularExpressions;

namespace Wavio.Activities
{
    [Activity(Label = "Sounds", ParentActivity = typeof(MicsActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "wavio.activities.MicsActivity")]
    public class MicSoundsActivity : AppCompatActivity
    {
        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;

        List<MicSound> sounds;
        ListView listView;
        private Android.App.ProgressDialog progressDialog;
        SoundAdapter adapter;

        string wavioId;

        ExpectedResponse awaitingResponse = ExpectedResponse.None;

        public enum ExpectedResponse
        {
            None,
            NowRecording,
            DoneRecording,
            SoundDeleted,
            
        };

        public enum PostMicCheck
        {
            AddSound,
            EditSound,
            DeleteSound
        };

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mics_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    NavUtils.NavigateUpFromSameTask(this);
                    break;
                case Resource.Id.new_mic:
                    //AddSound();
                    CheckMicOnline(PostMicCheck.AddSound);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void AddSound()
        {
            NewSoundDialog();
        }

        private void CheckMicOnline(PostMicCheck post)
        {
            try
            {
                progressDialog = Android.App.ProgressDialog.Show(this, "Please wait...", "Checking mic status...", true);
            }
            catch
            {
                //Acr.UserDialogs.UserDialogs.Instance.Progress("Please wait...");
            }
            string hwid = Android.OS.Build.Serial;
            
            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            var micTimeout = prefs.GetFloat("mic_timeout", 10000);

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
               

                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_LAST_ALIVE.ToString());
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

                        double nowInMs = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        double micTime = double.Parse(serverResponse.data);
                        double difference = nowInMs - micTime;
                        if (Math.Abs(difference) < micTimeout) //time travel and whatnot
                        {
                            if (post == PostMicCheck.AddSound)
                            {
                                AddSound();
                            }
                            else if (post == PostMicCheck.DeleteSound)
                            {

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
                        if (progressDialog != null)
                            progressDialog.Dismiss();
                        //AndHUD.Shared.ShowError(_context, "Server error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.GET_LAST_ALIVE)
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

        async void NewSoundDialog()
        {
            var newMicIdDialog = new PromptConfig();
            newMicIdDialog.SetInputMode(InputType.Default);
            newMicIdDialog.Title = "Record a new sound";
            //newMicIdDialog.Message = "Have your mic ready to record your sound, then press the record button. ";
            newMicIdDialog.Message = "What is the name of this sound?";
            newMicIdDialog.OkText = "NEXT";
            newMicIdDialog.CancelText = "CANCEL";
            newMicIdDialog.Placeholder = "Doorbell";
            var micIdResponse = await UserDialogs.Instance.PromptAsync(newMicIdDialog);
            if (micIdResponse.Ok)
            {
                Regex r = new Regex("^[a-zA-Z0-9]*$");
                if (!r.IsMatch(micIdResponse.Text) || micIdResponse.Text == "")
                {
                    UserDialogs.Instance.Alert("Please use a different name.");
                }
                else
                {
                  
                    var newDialog = new ConfirmConfig();
                    newDialog.Message = "Have your mic ready to record your sound, then press the record button. ";
                    newDialog.OkText = "RECORD";
                    newDialog.Title = "Record a new sound";

                    var recordResponse = await UserDialogs.Instance.ConfirmAsync(newDialog);
                    if (recordResponse)
                    {
                        AddNewSoundRequest(micIdResponse.Text);
                    }
                }
            }
        }

        private void AddNewSoundRequest(string soundName)
        {
            try
            {
                base.RunOnUiThread(() =>
                {
                    progressDialog = Android.App.ProgressDialog.Show(this, "Please wait...", "Sending new sound request...", true);
                });                
            }
            catch (Exception e)
            {
                string s = e.ToString();
                //Acr.UserDialogs.UserDialogs.Instance.Progress("Please wait...");
            }

            string hwid = Android.OS.Build.Serial;
            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            var newSound = new SoundInfo();
            newSound.sound_name = soundName;
            newSound.sound_image = "http://jmprog.com/hzinnovations/icons/pulse2.png";

            try
            {
                if (string.IsNullOrEmpty(wavioId))
                {
                    if (progressDialog != null)
                        progressDialog.Dismiss();
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: Missing ID");
                    return;
                }


                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.RECORD_NEW_SOUND.ToString());
                parameters.Add(Shared.ParamType.SOUND_INFO, JsonConvert.SerializeObject(newSound));

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
                        //Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        if (progressDialog != null)
                            progressDialog.Dismiss();
                        //AndHUD.Shared.ShowSuccess(_context, "Added!", AndroidHUD.MaskType.Clear, TimeSpan.FromSeconds(2));                        

                        progressDialog = Android.App.ProgressDialog.Show(this, "Please wait...", "Initializing mic...", true);
                        awaitingResponse = ExpectedResponse.NowRecording;

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
                        if (serverResponse.request != Shared.RequestCode.RECORD_NEW_SOUND)
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

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            sounds = new List<MicSound>();
            var tempSound1 = new MicSound();
            tempSound1.name = "...";
            sounds.Add(tempSound1);

            SetContentView(Resource.Layout.page_sounds);
            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mRegistrationBroadcastReceiver = new Shared.BroadcastReceiver();
            mRegistrationBroadcastReceiver.Receive += (sender, e) =>
            {
                //progressDialog.Dismiss();5
                var result = e.Intent.GetStringExtra("reply_error");
                if (result == Shared.ServerResponsecode.STARTED_RECORDING.ToString())
                {
                    if (progressDialog != null)
                        progressDialog.Dismiss();
                    if (awaitingResponse == ExpectedResponse.NowRecording)
                    {
                        progressDialog = Android.App.ProgressDialog.Show(this, "RECORDING...", "Play your sound...", true);
                        awaitingResponse = ExpectedResponse.DoneRecording;
                    }
                }
                else if (result == Shared.ServerResponsecode.DONE_RECORDING.ToString())
                {
                    if (progressDialog != null)
                        progressDialog.Dismiss();
                    if (awaitingResponse == ExpectedResponse.DoneRecording)
                    {

                    }
                }

            };

            listView = FindViewById<ListView>(Resource.Id.soundsListView);
            listView.ItemClick += OnListItemClick;

            adapter = new SoundAdapter(this, sounds);
            listView.Adapter = adapter;

            LocalBroadcastManager.GetInstance(this).RegisterReceiver(mRegistrationBroadcastReceiver,
              new IntentFilter("reply_error"));

            //wavioId = savedInstanceState.GetString("id", "");

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            wavioId = prefs.GetString("edit_mic_id", "");

            GetSoundList();
        }

        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void GetSoundList()
        {            

            try
            {
                progressDialog = Android.App.ProgressDialog.Show(this, "Please wait...", "Downloading Sound List...", true);
            }
            catch
            {
                //Acr.UserDialogs.UserDialogs.Instance.Progress("Please wait...");
            }
            
            string hwid = Android.OS.Build.Serial;
            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            try
            {
                if (string.IsNullOrEmpty(wavioId))
                {
                    if (progressDialog != null)
                        progressDialog.Dismiss();
                    Acr.UserDialogs.UserDialogs.Instance.ErrorToast("Error: Missing ID");
                    return;
                }
               

                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_SOUND_LIST.ToString());
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
                        //Acr.UserDialogs.UserDialogs.Instance.ShowError("Network error!");
                        return;
                    }


                    if (serverResponse.error == Shared.ServerResponsecode.OK)
                    {
                        if (progressDialog != null)
                            progressDialog.Dismiss();
                        //AndHUD.Shared.ShowSuccess(_context, "Added!", AndroidHUD.MaskType.Clear, TimeSpan.FromSeconds(2));                        

                        var sounds = JsonConvert.DeserializeObject<List<MicSound>>(serverResponse.data);
                        UpdateList(sounds);

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
                        if (serverResponse.request != Shared.RequestCode.GET_SOUND_LIST)
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

        private void UpdateList(List<MicSound> newList)
        {
            //DO NOT update twice, it will crash the app.

            //adapter = new SoundAdapter(this, newList);
            //listView.Adapter = adapter;

            adapter.ClearAndAdd(newList);
            
            base.RunOnUiThread(() =>
            {
                adapter.NotifyDataSetChanged();
            });

            

            //listView.SmoothScrollToPosition(0);
            //listView.ScrollListBy(1);
            //listView.SmoothScrollBy(100, 1000);
        }
    }
}