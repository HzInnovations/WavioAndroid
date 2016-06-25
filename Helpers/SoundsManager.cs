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

namespace Wavio.Helpers
{
    class SoundsManager
    {
        public static Dictionary<string, string>  NewSoundSettings()
        {
            var newSettings = new Dictionary<string, string>();
            newSettings.Add("Push", true.ToString());
            newSettings.Add("Vibrate", true.ToString());
            newSettings.Add("ShowMessage", false.ToString());

            return newSettings;
        }
    }
}