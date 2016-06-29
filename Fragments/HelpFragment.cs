

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
    public class HelpFragment : Fragment
    {
        
        public HelpFragment()
        {
            RetainInstance = true;
        }
		List<Question> questions;
        QuestionAdapter adapter;

        public HelpActivity parent;


        public override View OnCreateView(LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
			base.OnCreateView(inflater, container, savedInstanceState);

          	HasOptionsMenu = true;
            var view = inflater.Inflate(Resource.Layout.fragment_help, null);

            var grid = view.FindViewById<GridView>(Resource.Id.grid);
            //Get Icons
            questions = new List<Question>();


            RequestQuestionList();

            adapter = new QuestionAdapter(Activity, questions);
            grid.Adapter = adapter;
            grid.ItemClick += GridOnItemClick;
            return view;
        }

        void GridOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            //sound.sound_image = icons[itemClickEventArgs.Position].url;
            //RequestSetIcon();
            //var intent = new Intent(Activity, typeof(FriendActivity));
            //intent.PutExtra("Title", icons[itemClickEventArgs.Position].Title);
            //intent.PutExtra("Image", icons[itemClickEventArgs.Position].Image);
            //intent.PutExtra("Details", icons[itemClickEventArgs.Position].Details);
            //StartActivity(intent);
        }

        private void RequestQuestionList()
        {
            var loading = Acr.UserDialogs.UserDialogs.Instance.Loading("Loading Answers...");

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

                parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_QUESTION_LIST.ToString());
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
                        questions = JsonConvert.DeserializeObject<List<Question>>(serverResponse.data);
                        adapter.SetItems(questions);

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
                        if (serverResponse.request != Shared.RequestCode.GET_QUESTION_LIST)
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