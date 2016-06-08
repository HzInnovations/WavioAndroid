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
    class Notif
    {
        public int Id
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string Sender
        {
            get;
            set;
        }
        public string Image
        {
            get;
            set;
        }
        public string Date
        {
            get;
            set;
        }
    }
   
}