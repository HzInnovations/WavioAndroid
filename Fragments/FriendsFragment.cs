using System;
using System.Collections.Generic;

using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using Wavio.Activities;
using Wavio.Adapters;
using Wavio.Models;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;

namespace Wavio.Fragments
{
    public class FriendsFragment : Fragment
    {
		Android.Support.V4.View.ViewPager viewPager;
		FragmentPagerAdapter adapter;

        public FriendsFragment()
        {
            RetainInstance = true;
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.fragment_notif_tabs, null);

            
            // Create your application here
            viewPager = view.FindViewById<ViewPager>(Resource.Id.viewPager);
            viewPager.OffscreenPageLimit = 4;
			var tabs = view.FindViewById<TabLayout>(Resource.Id.tabs);
			tabs.TabMode = TabLayout.ModeScrollable;
            //Since we are a fragment in a fragment you need to pass down the child fragment manager!
            adapter = new FriendsAdapter (ChildFragmentManager);


			viewPager.Adapter = adapter;

            //tabs.SetupWithViewPager(viewPager);
            tabs.SetupWithViewPager(viewPager);
            return view;
        }

    }
}