using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Wavio.Models;
using Wavio.Activities;
//using UniversalImageLoader.Core;

namespace Wavio.Adapters
{
    internal class QuestionAdapterWrapper : Java.Lang.Object
    {
        public TextView TitleA { get; set; }

        public TextView TitleB { get; set; }

        //public ImageView Art { get; set; }
    }

    class QuestionAdapter : BaseAdapter
    {
        private readonly Activity context;
        private IEnumerable<Question> questions;

        public ImageLoader ImageLoader { get; set; }

        public void SetItems(IEnumerable<Question> Questions)
        {
            questions = Questions;
        }


        public QuestionAdapter(Activity Context, IEnumerable<Question> Questions)
        {
            this.context = Context;
            this.questions = Questions;
            ImageLoader = ImageLoader.Instance;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (position < 0)
                return null;

            var view = (convertView
                       ?? context.LayoutInflater.Inflate(
                           Resource.Layout.item_question, parent, false)
                       );

            if (view == null)
                return null;

            var wrapper = view.Tag as QuestionAdapterWrapper;
            if (wrapper == null)
            {
                wrapper = new QuestionAdapterWrapper
                {
                    TitleA = view.FindViewById<TextView>(Resource.Id.item_title),
                    TitleB = view.FindViewById<TextView>(Resource.Id.item_details)
                    //Art = view.FindViewById<ImageView>(Resource.Id.item_image)
                };
                view.Tag = wrapper;
            }

            var question = questions.ElementAt(position);

            wrapper.TitleA.Text = question.question;
            wrapper.TitleB.Text = question.answer;

            //wrapper.Art.SetImageResource(Android.Resource.Color.Transparent);
            //ImageLoader.DisplayImage(friend.Image, wrapper.Art);
            return view;
        }

        public override int Count
        {
            get { return questions.Count(); }
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