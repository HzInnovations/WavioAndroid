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
        
        public string sound_name
        {
            get;
            set;
        }
        public string sound_image
        {
            get;
            set;
        }
        public string sound_id
        {
            get;
            set;
        }
        public string sound_requester
        {
            get;
            set;
        }
        public Dictionary<String, String> sound_settings;
    }


}