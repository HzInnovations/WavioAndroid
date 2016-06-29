using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Wavio.Models;
//using UniversalImageLoader.Core;

namespace Wavio.Adapters
{
    internal class IconAdapterWrapper : Java.Lang.Object
    {
        public TextView Title { get; set; }

        public ImageView Art { get; set; }
    }

    class IconAdapter : BaseAdapter
    {
        private readonly Activity context;
        private  IEnumerable<Icon> icons;

        public ImageLoader ImageLoader { get; set; }

        public IconAdapter(Activity Context, IEnumerable<Icon> Icons)
        {
            this.context = Context;
            this.icons = Icons;
            ImageLoader = ImageLoader.Instance;
        }

        public void SetItems(IEnumerable<Icon> Icons)
        {
            icons = Icons;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position < 0)
                return null;

            var view = (convertView
                       ?? context.LayoutInflater.Inflate(
                           Resource.Layout.item_icon, parent, false)
                       );

            if (view == null)
                return null;

            var wrapper = view.Tag as IconAdapterWrapper;
            if (wrapper == null)
            {
                wrapper = new IconAdapterWrapper
                {
                    Title = view.FindViewById<TextView>(Resource.Id.item_title),
                    Art = view.FindViewById<ImageView>(Resource.Id.item_image)
                };
                view.Tag = wrapper;
            }

            var icon = icons.ElementAt(position);

            //wrapper.Title.Text = icon.Name;

            wrapper.Art.SetImageResource(Android.Resource.Color.Transparent);
            ImageLoader.DisplayImage(icon.url, wrapper.Art);
            return view;
        }

        public override int Count
        {
            get { return icons.Count(); }
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