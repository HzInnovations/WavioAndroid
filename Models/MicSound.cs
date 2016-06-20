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

namespace Wavio.Models
{
    public class MicSound
    {
        public string type
        {
            get;
            set;
        }

        public string image
        {
            get;
            set;
        }
        public string name
        {
            get;
            set;
        }
        public string imageUrl
        {
            get;
            set;
        }
        public string id
        {
            get;
            set;
        }
        public string requester
        {
            get;
            set;
        }
        public Dictionary<String, String> settings;
    }


}