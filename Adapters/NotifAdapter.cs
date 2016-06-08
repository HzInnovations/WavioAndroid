using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Wavio.Models;
using System;
using Android.Preferences;
//using UniversalImageLoader.Core;

namespace Wavio.Adapters
{
    internal class NotifAdapterWrapper : Java.Lang.Object
    {
        public TextView Title { get; set; }

        public TextView Details { get; set; }

        public ImageView Art { get; set; }
    }

    class NotifAdapter : BaseAdapter
    {
        private readonly Activity context;
        private IEnumerable<Notif> notifs;
        private bool relativeTime = false;
        private bool hideIcons = false;
        private bool largeCards = false;


        public ImageLoader ImageLoader { get; set; }

        public NotifAdapter(Activity Context, IEnumerable<Notif> Notifs)
        {
            this.context = Context;
            this.notifs = Notifs;
            OrganizeNotifs();
            ImageLoader = ImageLoader.Instance;

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            relativeTime = prefs.GetBoolean("notif_time_format", false);
            hideIcons = prefs.GetBoolean("notif_hide_icons", false);

            string layoutStyle = prefs.GetString("notif_layout", "0");
            if (layoutStyle == "2" || layoutStyle == "3")
            {
                largeCards = true;
            }
            else
            {
                largeCards = false;
            }
        }

        public void UpdateNotifs(IEnumerable<Notif> Notifs)
        {
            this.notifs = Notifs;
            OrganizeNotifs();
            NotifyDataSetChanged();
            
            
        }

       private void OrganizeNotifs()
        {
            notifs = notifs.OrderByDescending(e => e.Id);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position < 0)
                return null;


            int notifType = Resource.Layout.item_notif;
            if (largeCards)
            {
                notifType = Resource.Layout.item_notif_large;
            }

            var view = (convertView
                       ?? context.LayoutInflater.Inflate(
                           notifType, parent, false)
                       );
                       
            //var view = context.LayoutInflater.Inflate(Resource.Layout.item_notif, parent, false);
            
            if (view == null)
                return null;

            var wrapper = view.Tag as NotifAdapterWrapper;
            if (wrapper == null)
            {
                wrapper = new NotifAdapterWrapper
                {
                    Title = view.FindViewById<TextView>(Resource.Id.item_title),
                    Art = view.FindViewById<ImageView>(Resource.Id.item_image),
                    Details = view.FindViewById<TextView>(Resource.Id.item_details)
                };
                view.Tag = wrapper;
            }

            var notif = notifs.ElementAt(position);

            wrapper.Title.Text = notif.Name;

            wrapper.Details.Text = notif.Sender + "  -  " + GetTimeText(notif.Date);

            if (hideIcons)
            {
                wrapper.Art.Visibility = ViewStates.Gone;
            }
            else
            {
                wrapper.Art.SetImageResource(Android.Resource.Color.Transparent);
            }

            ImageLoader.DisplayImage(notif.Image, wrapper.Art);
            return view;
        }

        public string GetTimeText(string date)
        {
            string returnTime;
            var receivedAt = DateTime.Parse(date);

            TimeSpan difference = DateTime.Now - receivedAt;

            if (relativeTime)
            {
                if (difference.TotalSeconds < 60)
                {
                    returnTime = (int)difference.TotalSeconds + " seconds ago";
                }
                else if (difference.TotalMinutes < 60)
                {
                    returnTime = (int)difference.TotalMinutes + " minutes ago";
                }
                else if (difference.TotalHours < 24)
                {
                    returnTime = (int)difference.TotalHours + " hours ago";
                }
                else
                {
                    returnTime = (int)difference.TotalDays + " days ago";
                }
            }
            else
            {
                if (difference.TotalHours > 12)
                {
                    returnTime = receivedAt.ToString("d");
                }
                else
                {
                    returnTime = receivedAt.ToString("t");
                }
            }
            

            return returnTime;
        }

        public override int Count
        {
            get { return notifs.Count(); }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override bool HasStableIds
        {
            get
            {
                return true;
            }
        }
    }
}