using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;


using Wavio.Adapters;
using Android.Support.Design.Widget;
using Wavio.Helpers;
using System.Linq;
using System.Collections.Generic;
using Android.Support.V7.App;

namespace Wavio.Fragments
{
    public class TabbedNotifsFragment : Fragment
    {
		ViewPager viewPager;
        TabbedNotifsAdapter adapter;
        List<string> mics;
        TabLayout tabs;

        public int micUpdateCount = 0;

        AppCompatActivity home;


        private bool fetchingNotifs = false;

        public TabbedNotifsFragment(AppCompatActivity Home)
        {
            home = Home;
            RetainInstance = true;
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.fragment_notif_tabs, null);
            mics = new List<string>();

            
            viewPager = view.FindViewById<ViewPager>(Resource.Id.viewPager);
            viewPager.OffscreenPageLimit = 4;
			tabs = view.FindViewById<TabLayout>(Resource.Id.tabs);
			tabs.TabMode = TabLayout.ModeScrollable;

            //Since we are a fragment in a fragment you need to pass down the child fragment manager!
            //Linq for now, might have issues on IOS though

            SetupTabs();


            return view;
        }

        void SetupTabs()
        {

            var micNames = MicsManager.GetMicsFromPreferences().Select(e => e.Name).ToList();
            micNames.Insert(0, "All");
            var micIds = MicsManager.GetMicsFromPreferences().Select(e => e.WavioId).ToList();
            micIds.Insert(0, "All");

            
            adapter = new TabbedNotifsAdapter(home, ChildFragmentManager, micNames.ToArray(), micIds.ToArray());
            viewPager.Adapter = adapter;

            tabs.SetupWithViewPager(viewPager);
        }

        private void UpdateMics()
        {
            var micNames = MicsManager.GetMicsFromPreferences().Select(e => e.Name).ToList();
            micNames.Insert(0, "All");
            var micIds = MicsManager.GetMicsFromPreferences().Select(e => e.WavioId).ToList();
            micIds.Insert(0, "All");

            var same = micIds.SequenceEqual(mics);
            if (!same)
            {
                mics = micIds;
                adapter.SetTabs(micNames.ToArray(), micIds.ToArray());
                adapter.NotifyDataSetChanged();
            }
            
        }

        public override void OnResume()
        {
            base.OnResume();
            

            var prefs = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);

            var settingsChanged = prefs.GetBoolean("settings_changed", false);
            if (settingsChanged)
            {
                Android.Content.ISharedPreferencesEditor editor = prefs.Edit();
                editor.Remove("settings_changed");
                editor.Apply();
                SetupTabs();
            }

            UpdateMics();
            
        }
        

    }
}