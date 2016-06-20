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

namespace Wavio.Activities
{
    [Activity(Label = "Sounds", ParentActivity = typeof(MicsActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "wavio.activities.MicsActivity")]
    public class MicSoundsActivity : AppCompatActivity
    {
        Shared.BroadcastReceiver mRegistrationBroadcastReceiver;

        List<MicSound> sounds;
        ListView listView;

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.page_sounds);
            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

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
            listView.Adapter = new SoundAdapter(this, sounds);

            LocalBroadcastManager.GetInstance(this).RegisterReceiver(mRegistrationBroadcastReceiver,
              new IntentFilter("sound_updated"));
        }

        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}