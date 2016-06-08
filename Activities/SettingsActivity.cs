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
using Wavio.Fragments;
using Android.Support.V7.App;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Wavio.Activities
{
    [Activity(Label = "Settings", ParentActivity = typeof(HomeView))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "wavio.activities.HomeView")]
    public class SettingsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.page_settings);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, new SettingsFragment())
                    .Commit();
        }
    }
}