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
using Wavio.Helpers;
using Android.Support.V4.App;
using Wavio.Models;
using Newtonsoft.Json;

namespace Wavio.Activities
{
    [Activity(Label = "Sound Settings", ParentActivity = typeof(MicSoundsActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "wavio.activities.MicSoundsActivity")]
    public class SoundSettingsActivity : AppCompatActivity
    {

        private MicSound sound;
        string micId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            //string micId = savedInstanceState.GetString("mic_id");
            //string micId = base.GetString("mic_id");
            micId = Intent.GetStringExtra("mic_id");
            sound = JsonConvert.DeserializeObject<MicSound>(Intent.GetStringExtra("sound"));

            //string micName = MicsManager.GetMicsFromPreferences().FirstOrDefault(e => e.WavioId == micId).Name;
            

            SetContentView(Resource.Layout.page_settings);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            toolbar.Title = sound.sound_name;

            SoundSettingsFragment fragment = new SoundSettingsFragment(micId, sound, this);

            SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, fragment)                    
                    .Commit();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:

                    NavUtils.NavigateUpFromSameTask(this);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void GoToIconSelect()
        {
            PickIconFragment fragment = new PickIconFragment(sound, micId);
            fragment.parent = this;

            SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, fragment)
                    .Commit();
        }
        public void NavigateUp()
        {
            NavUtils.NavigateUpFromSameTask(this);
        }
    }
}