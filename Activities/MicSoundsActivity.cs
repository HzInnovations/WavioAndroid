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

namespace Wavio.Activities
{
    [Activity(Label = "Sounds", ParentActivity = typeof(MicsActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "wavio.activities.MicsActivity")]
    public class MicSoundsActivity : AppCompatActivity
    {
        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;

        List<MicSound> sounds;
        ListView listView;
        private ProgressDialog progressDialog;
        SoundAdapter adapter;

        string wavioId;

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
                var result = e.Intent.GetBooleanExtra("sound_updated", false);
                if (result)
                {
                  
                }

            };

            listView = FindViewById<ListView>(Resource.Id.soundsListView);
            listView.ItemClick += OnListItemClick;

            adapter = new SoundAdapter(this, sounds);
            listView.Adapter = adapter;

            LocalBroadcastManager.GetInstance(this).RegisterReceiver(mRegistrationBroadcastReceiver,
              new IntentFilter("sound_updated"));

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