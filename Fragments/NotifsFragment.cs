using System;
using System.Collections.Generic;

using Android.Content;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using Wavio.Activities;
using Wavio.Adapters;
using Wavio.Models;
using Wavio.Helpers;
using Android.Preferences;
using Android.App;
using Acr.UserDialogs;
using RestSharp;
using Newtonsoft.Json;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Support.V7.App;

namespace Wavio.Fragments
{
    public class NotifsFragment : Android.Support.V4.App.Fragment
    {
        ViewGroup _container;
        Android.OS.Bundle bundle;

        List<Notif> notifs;
        private string micName;
        string micId;
        GridView gridView;
        NotifAdapter adapter;

        private AppCompatActivity home;

        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;

        private bool updatingNotifs = false;

        public NotifsFragment(AppCompatActivity Home, string MicName, string MicId)
        {
            RetainInstance = true;
            micName = MicName;
            micId = MicId;
            home = Home;
            
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
			base.OnCreateView(inflater, container, savedInstanceState);
            _container = container;
            bundle = savedInstanceState;

            HasOptionsMenu = true;

            notifs = NotifsManager.GetSavedNotifications(micId);

            var view = inflater.Inflate(Resource.Layout.fragment_notifs, null);
            gridView = view.FindViewById<GridView>(Resource.Id.notif_grid);

            //notif_layout
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            string layoutStyle = prefs.GetString("notif_layout", "0");
            if (layoutStyle == "0" || layoutStyle == "2")
            {
                gridView.NumColumns = 1;
            }
            else if (layoutStyle == "1" || layoutStyle == "3")
            {
                gridView.NumColumns = 2;
            }


            //gridView.FastScrollEnabled = prefs.GetBoolean("notif_scrollbar_show", true);
            
            
            adapter = new NotifAdapter(Activity, notifs);
            gridView.Adapter = adapter;
            
            gridView.ItemClick += GridOnItemClick;

            if (micId != "All")
            {
                if (!updatingNotifs)
                {
                    updatingNotifs = true;
                    RequestUpdateNotifs();
                }

            }

            mRegistrationBroadcastReceiver = new Shared.BroadcastReceiver();
            mRegistrationBroadcastReceiver.Receive += (sender, e) =>
            {
                if (micId != "All")
                {
                    RequestUpdateNotifs();
                }     
                else
                {
                    adapter.UpdateNotifs(NotifsManager.GetSavedNotifications(micId));

                    home.RunOnUiThread(() =>
                    {
                        adapter.NotifyDataSetChanged();
                    });

                    int index = gridView.FirstVisiblePosition;
                    gridView.SmoothScrollToPosition(index);
                    gridView.SmoothScrollBy(1, 1000);
                }    
            };

            return view;
        }

        override public void OnResume()
        {
            base.OnResume();

            /*
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            if (prefs.GetBoolean("settings_changes", false))
            {
                adapter = new NotifAdapter(Activity, notifs);
                gridView.Adapter = adapter;
                gridView.ItemClick += GridOnItemClick;
            }
            */

            if (micId != "All")
            {
                LocalBroadcastManager.GetInstance(_container.Context).RegisterReceiver(mRegistrationBroadcastReceiver,new IntentFilter("update_notifs"));

                if (!updatingNotifs)
                {
                    updatingNotifs = true;
                    RequestUpdateNotifs();
                }
            }
            else
            {
                LocalBroadcastManager.GetInstance(_container.Context).RegisterReceiver(mRegistrationBroadcastReceiver,
                    new IntentFilter("notif_All"));
            }
            


        }
        override public void OnPause()
        {
            LocalBroadcastManager.GetInstance(_container.Context).UnregisterReceiver(mRegistrationBroadcastReceiver);
            base.OnPause();
        }

        private void GridOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            /*
            var intent = new Intent(Activity, typeof(FriendActivity));
            intent.PutExtra("Title", notifs[itemClickEventArgs.Position].Name);
            intent.PutExtra("Image", notifs[itemClickEventArgs.Position].Image);
            StartActivity(intent);
            */
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);

            //var filterActionItem = menu.Add(Menu.None, Resource.Id.notif_ok, Menu.None, Resource.Id.notif_ok);
            inflater.Inflate(Resource.Menu.notifs_menu, menu);

            //return base.OnCreateOptionsMenu(menu);
            //MenuItemCompat.SetShowAsAction(filterActionItem, MenuItemCompat.ShowAsActionIfRoom);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.notif_ok:
                    ClearNotifs();
                    //Toast.MakeText(Context, "Selected Item: " + item.TitleFormatted, ToastLength.Short).Show();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void ClearNotifs()
        {
            NotifsManager.ClearNotifs(micId);

            adapter.UpdateNotifs(NotifsManager.GetSavedNotifications(micId));

            adapter.NotifyDataSetChanged();
            
            home.RunOnUiThread(() =>
            {
                adapter.NotifyDataSetChanged();
            });
            /*
            RunOnUiThread(() =>
            {
                adapter.NotifyDataSetChanged();
            });
            */

            int index = gridView.FirstVisiblePosition;
            gridView.SmoothScrollToPosition(index);
            gridView.SmoothScrollBy(1, 5);


            var localBroadcast = new Intent("update_notifs");
            LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);

            /*
            if (micId == "All")
            {
                var localBroadcast = new Intent("update_notifs");
                LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);
            }
            else
            {
                var localBroadcast = new Intent("notif_All");
                LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);
            }
            */
            
        }


        private void RequestUpdateNotifs()
        {
            MicsManager.instance.micsUpdating++;
            MicsManager.instance.updatesQueued = true;



            //UserDialogs.Instance.ShowLoading();

            var client = new RestClient(Shared.SERVERURL);
            var request = new RestRequest("resource/{id}", Method.POST);
            var parameters = new Dictionary<string, string>();
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            string gcmID = prefs.GetString("GCMID", "");
            string hwid = Android.OS.Build.Serial;

            if (prefs.GetBoolean("notif_show_update", false))
            {
                UserDialogs.Instance.ShowLoading();
            }

            if (prefs.GetBoolean("clear_instanceid", false) == true)
            {
                updatingNotifs = false;
                return;
            }

            string lastCheckString = prefs.GetString(micId + "_LastCheck", "");
            DateTime lastCheck = new DateTime(1970, 1, 2);
            if (!string.IsNullOrEmpty(lastCheckString))
            {
                lastCheck = DateTime.Parse(lastCheckString);
            }
            string dateFormatted = lastCheck.ToString("yyyy-MM-dd HH:mm:ss");

            parameters.Add(Shared.ParamType.REQUEST_CODE, Shared.RequestCode.GET_NOTIFICATIONS.ToString());
            parameters.Add(Shared.ParamType.WAVIO_ID, micId);
            parameters.Add(Shared.ParamType.GCM_ID, gcmID);
            parameters.Add(Shared.ParamType.HWID, hwid);
            parameters.Add(Shared.ParamType.NOTIFS_SINCE, dateFormatted);

            string requestJson = JsonConvert.SerializeObject(parameters);
            request.AddParameter(Shared.ParamType.REQUEST, requestJson);


            client.ExecuteAsync(request, response => {

                MicsManager.instance.micsUpdating--;
                //AndHUD.Shared.Dismiss();
                Shared.ServerResponse serverResponse = JsonConvert.DeserializeObject<Shared.ServerResponse>(response.Content);

                if (serverResponse == null)
                {
                    //AndHUD.Shared.ShowError(Context, "Network error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                    //UserDialogs.Instance.ErrorToast("Network error!");
                    UserDialogs.Instance.HideLoading();
                    return;
                }


                if (serverResponse.error == Shared.ServerResponsecode.DATABASE_ERROR)
                {
                    //AndHUD.Shared.ShowError(Context, "Server error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                    UserDialogs.Instance.ErrorToast("Server error!");
                }
                else if (serverResponse.error == Shared.ServerResponsecode.OK)
                {
                    var notifs = JsonConvert.DeserializeObject<List<Notif>>(serverResponse.data);
                    
                    var result = NotifsManager.AddNewNotifs(micId, notifs);

                    if (true || notifs.Count > 0)
                    {
                        var allNotifs = NotifsManager.GetSavedNotifications(micId);
                        adapter.UpdateNotifs(allNotifs);
                        //gridView.SetSelection(0);
                        //gridView.SmoothScrollToPosition(0);
                        int index = gridView.FirstVisiblePosition;
                        gridView.SmoothScrollToPosition(index);
                        gridView.SmoothScrollToPosition(0);
                        gridView.SmoothScrollBy(1, 500);

                        home.RunOnUiThread(() =>
                        {
                            adapter.NotifyDataSetChanged();
                        });
                    }

                    int i = MicsManager.instance.micsUpdating;
                    if (MicsManager.instance.micsUpdating <= 0 && MicsManager.instance.updatesQueued)
                    {
                        MicsManager.instance.micsUpdating = 0;
                        MicsManager.instance.updatesQueued = false;
                        var localBroadcast = new Intent("notif_All");
                        LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);
                    }
                    /*
                    MicsManager.instance.micsUpdated++;
                    if (MicsManager.instance.micsUpdated >= MicsManager.GetMicsFromPreferences().Count)
                    {
                        var localBroadcast = new Intent("notif_All");
                        LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);

                        MicsManager.instance.micsUpdated = 0;
                    }
                    */
                    //var localBroadcast = new Intent("notif_All");
                    //LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);

                    //AndHUD.Shared.ShowSuccess(Context, "Added!", AndroidHUD.MaskType.Clear, TimeSpan.FromSeconds(2));

                }
                else
                {
                    if (serverResponse.request != Shared.RequestCode.GET_NOTIFICATIONS)
                    {
                        //AndHUD.Shared.ShowError(Context, "Request type mismatch!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                        UserDialogs.Instance.ErrorToast("Request type mismatch!");
                        return;
                    }
                    //AndHUD.Shared.ShowError(Context, "Unknown error!", AndroidHUD.MaskType.Black, TimeSpan.FromSeconds(2));
                    UserDialogs.Instance.ErrorToast("Unknown error!");
                }

                updatingNotifs = false;
                UserDialogs.Instance.HideLoading();
                return;

            });
        }
    }
}