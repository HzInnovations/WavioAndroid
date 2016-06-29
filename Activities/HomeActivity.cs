using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;

using Wavio.Fragments;
using Android.Support.Design.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Acr.UserDialogs;
using Android.Content;
using System;
using Wavio.Helpers;
using Android.Preferences;
using Android.Support.V4.Content;

//using UniversalImageLoader.Core;

namespace Wavio.Activities
{
	[Activity (Label = "Wavio", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/ic_launcher")]
	public class HomeView : BaseActivity
	{
		DrawerLayout drawerLayout;
		NavigationView navigationView;
        private int currentActivity = -1;

        int viewId = Resource.Layout.page_home_view;


        protected override int LayoutResource {
			get {

				return viewId;
			}
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            if (prefs.GetBoolean("disable_drawer_header", false))
            {
                viewId = Resource.Layout.page_home_noheader;
            }

            base.OnCreate (savedInstanceState);
            var config = ImageLoaderConfiguration.CreateDefault(ApplicationContext);
            // Initialize ImageLoader with configuration.
            //ImageLoader.Instance.Init(config);

            DisplayImageOptions defaultOptions = new DisplayImageOptions.Builder()
                .CacheInMemory(true)
                .CacheOnDisk(true)
                .Build();
            ImageLoaderConfiguration newConfig = new ImageLoaderConfiguration.Builder(ApplicationContext)
                .DefaultDisplayImageOptions(defaultOptions)
                .Build();
            ImageLoader.Instance.Init(newConfig);

            UserDialogs.Init(this);
            MicsManager.instance = new MicsManager();

            
            ISharedPreferencesEditor editor = prefs.Edit();
            if (string.IsNullOrEmpty(prefs.GetString("notif_title", "")))
            {
                editor.PutString("notif_title", "Sound Detected");
                editor.Apply();
            }
            if (string.IsNullOrEmpty(prefs.GetString("notif_body", "")))
            {
                editor.PutString("notif_body", "#SOUND");
                editor.Apply();
            }

            drawerLayout = FindViewById<DrawerLayout> (Resource.Id.drawer_layout);
            


            SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu);
			navigationView = FindViewById<NavigationView> (Resource.Id.nav_view);

            for (int i = 1; i < 6; i++)
            {
                navigationView.Menu.GetItem(i).SetCheckable(false);
            }

            navigationView.NavigationItemSelected += (sender, e) => {

                //e.MenuItem.SetChecked (true);
                e.MenuItem.SetChecked(false);
                //navigationView.Selected = false;

                switch (e.MenuItem.ItemId)
				{
				case Resource.Id.nav_home:
                    e.MenuItem.SetChecked(true);
                    ListItemClicked(0);
					break;
				case Resource.Id.nav_mics:
                    ListItemClicked(1);
					break;
				case Resource.Id.nav_bt:
					ListItemClicked(2);
					break;
                case Resource.Id.nav_settings:
                    ListItemClicked(3);
                    break;
                case Resource.Id.nav_feedback:
                    ListItemClicked(4);
                    break;
                case Resource.Id.nav_help:
                    ListItemClicked(5);
                    break;
                }				

				drawerLayout.CloseDrawers ();                

            };

			//if first time you will want to go ahead and click first item.
			if (savedInstanceState == null)
            {
                //SupportActionBar.SetWindowTitle("Notifications");

                ListItemClicked (-1);
			}
		}
        

        private void ListItemClicked (int position)
		{
			Android.Support.V4.App.Fragment fragment = null;
            //SupportActionBar.SetWindowTitle("Wavio");

            


            switch (position) {
            case -1:
                fragment = new TabbedNotifsFragment(this);
                break;
            case 0:
                    //just update the notifs instead of recreating the fragment, which sometimes causes problems.
                    UpdateNotifs();
                break;
			case 1:               
                var intent = new Intent(this, typeof(MicsActivity));
                StartActivity(intent);
                break;
			case 2:
                var intent3 = new Intent(this, typeof(BluetoothActivity));
                StartActivity(intent3);
                break;
            case 3:                
                var intent2 = new Intent(this, typeof(SettingsActivity));
                StartActivity(intent2);                
                break;
            case 4:
                //GetFeedback();
                break;
            case 5:
                GetFeedback();
                break;
            }

            
            if (fragment != null)
            {
                if (currentActivity != 0)
                {
                    SupportFragmentManager.BeginTransaction()
                   .Replace(Resource.Id.content_frame, fragment)
                   .Commit();
                }
                
            }

            currentActivity = position;

        }

        private void UpdateNotifs()
        {
            var localBroadcast = new Intent("update_notifs");
            LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(localBroadcast);
        }

	    private void GetFeedback()
        {
            //int fragmentCount = SupportFragmentManager.Fragments.Count;
            //var s = SupportFragmentManager.Fragments[0];
            //var s2 = SupportFragmentManager.Fragments[0].GetType();
            //var same = SupportFragmentManager.Fragments[0].GetType() == typeof(TabbedNotifsFragment);
            //AndroidHUD.AndHUD.Shared.Show(this, s);
        }

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Android.Resource.Id.Home:
				drawerLayout.OpenDrawer (Android.Support.V4.View.GravityCompat.Start);
				return true;
			}
			return base.OnOptionsItemSelected (item);
		}
	}
}

