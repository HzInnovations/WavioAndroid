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
    public class SavedMic
    {
        public string WavioId { get; set; } //Unique ID for each wavio
        public string Name { get; set; }//Name the user gave their wavio
        public Dictionary<string, string> LocalSettings { get; set; }//Personal setting, not shared with other users who have this same wavio
    }


}