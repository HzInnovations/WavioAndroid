

using System;
using System.Collections.Generic;

using Android.Content;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using Wavio.Activities;
using Wavio.Adapters;
using Wavio.Models;
using Android.Preferences;

using RestSharp;
using Wavio.Helpers;
using Newtonsoft.Json;
using static Wavio.Helpers.Shared;
using System.Net;

namespace Wavio.Fragments
{
    public class PickIconFragment : Fragment
    {
        public SoundSettingsActivity parent;

        public MicSound sound;
        public string micId;

        public PickIconFragment(MicSound Sound, string MicId)
        {
            micId = MicId;
            sound = Sound;
            RetainInstance = true;
        }

        public PickIconFragment()
        {
            RetainInstance = true;
        }
		List<Icon> icons;
        IconAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
			base.OnCreateView(inflater, container, savedInstanceState);

          	HasOptionsMenu = true;
            var view = inflater.Inflate(Resource.Layout.fragment_browse, null);

            var grid = view.FindViewById<GridView>(Resource.Id.grid);
            //Get Icons
            icons = new List<Icon>();
            var newIcon = new Icon();
            newIcon.name = "default";
            newIcon.url = "http://jmprog.com/hzinnovations/icons/pulse2.png";
            icons.Add(newIcon);

            RequestIconList();

            adapter = new IconAdapter(Activity, icons);
            grid.Adapter = adapter;
            grid.ItemClick += GridOnItemClick;
            return view;
        }

        void GridOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            sound.sound_image = icons[itemClickEventArgs.Position].url;
            RequestSetIcon();
            //var intent = new Intent(Activity, typeof(FriendActivity));
            //intent.PutExtra("Title", icons[itemClickEventArgs.Position].Title);
            //intent.PutExtra("Image", icons[itemClickEventArgs.Position].Image);
            //intent.PutExtra("Details", icons[itemClickEventArgs.Position].Details);
            //StartActivity(intent);
        }

        private void RequestIconList()
        {
            var loading = Acr.UserDialogs.UserDialogs.Instance.Loading("Getting icon list...");

            string hwid = Android.OS.Build.Serial;

            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");            

            try
            {
                
                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_ICON_LIST.ToString());
                //parameters.Add(Shared.ParamType.WAVIO_ID, micId);
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
                        icons = JsonConvert.DeserializeObject<List<Icon>>(serverResponse.data);
                        adapter.SetItems(icons);

                        parent.RunOnUiThread(() =>
                        {
                            adapter.NotifyDataSetChanged();
                        });

                        loading.Hide();
                    }

                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.GET_ICON_LIST)
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

        private void RequestSetIcon()
        {
            var loading = Acr.UserDialogs.UserDialogs.Instance.Loading("Updating...");

            string hwid = Android.OS.Build.Serial;

            var SharedSettings = new Dictionary<String, String>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            String gcmID = prefs.GetString("GCMID", "");

            try
            {

                var client = new RestClient(Shared.SERVERURL);
                var request = new RestRequest("resource/{id}", Method.POST);
                var parameters = new Dictionary<string, string>();

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.EDIT_SOUND.ToString());
                parameters.Add(Shared.ParamType.WAVIO_ID, micId);
                parameters.Add(Shared.ParamType.GCM_ID, gcmID);
                parameters.Add(Shared.ParamType.HWID, hwid);

                string soundJson = JsonConvert.SerializeObject(sound);
                parameters.Add(Shared.ParamType.SOUND_INFO, soundJson);

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
                        parent.NavigateUp();
                    }

                    else if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                    {
                        Acr.UserDialogs.UserDialogs.Instance.ShowError("Server error!");
                    }
                    else
                    {
                        if (serverResponse.request != Shared.RequestCode.EDIT_SOUND)
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