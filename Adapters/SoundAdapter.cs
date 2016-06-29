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
using Java.Lang;
using Wavio.Models;
using Android.Graphics.Drawables;
using Com.Nostra13.Universalimageloader.Core;

namespace Wavio.Adapters
{
    public class SoundAdapter : BaseAdapter<MicSound>
    {
        Activity context;
        List<MicSound> sounds;

        public ImageLoader ImageLoader { get; set; }

        public SoundAdapter(Activity _context, List<MicSound> _list)
            : base()
        {

            ImageLoader = ImageLoader.Instance;
            this.context = _context;
            this.sounds = _list;
        }

        public override int Count
        {
            get { return sounds.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override MicSound this[int index]
        {
            get { return sounds[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            // re-use an existing view, if one is available
            // otherwise create a new one

            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.item_sound, parent, false);

            MicSound item = this[position];
            view.FindViewById<TextView>(Resource.Id.soundNameText).Text = item.sound_name;
            //view.FindViewById<TextView>(Resource.Id.micIdText).Text = item.WavioId;
            var imageView = view.FindViewById<Helpers.SquareImageView>(Resource.Id.item_image);

            ImageLoader.DisplayImage(item.sound_image, imageView);
            /*
            using (var imageView = view.FindViewById<ImageView>(Resource.Id.Thumbnail))
            {
                string url = Android.Text.Html.FromHtml(item.thumbnail).ToString();

                //Download and display image
                Koush.UrlImageViewHelper.SetUrlDrawable(imageView,
                    url, Resource.Drawable.Placeholder);
            }
            */

            return view;
        }

        public void ClearAndAdd(List<MicSound> newSounds)
        {
            sounds = newSounds;            

        }
    }
}