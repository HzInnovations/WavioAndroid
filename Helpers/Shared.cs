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
    public class Shared
    {
        public static string SERVERURL = "http://www.jmprog.com/hzinnovations/wavio_testing/userrequests.php";

        public class ParamType
        {
            public static string REQUEST = "request"; // The entire request, in json format
            public static string REQUEST_CODE = "request_code"; //What they want to do
            public static string WAVIO_ID = "wavio_id"; // WAVIO ID. Everyone WAVIO has it's own unique WAVIOID to identify it
            public static string HWID = "phone_hwid";//phones unique hwid
            public static string GCM_ID = "gcm_id"; // The phones GCM ID
            public static string VERSION = "version";//The version of the requester (probably the APP)
            public static string SOUND_INFO = "sound_info";
            public static string SOUND_NAME = "sound_name";
            public static string SHARED_SETTINGS = "shared_settings";
            public static string NOTIFS_SINCE = "notifs_since";//get all notifications since this time. Should be time since epoch as an int.
            public static string FEEDBACK = "feedback";
        }

        public class QueuedDeviceRequestType
        {
            public static string NEW_SOUND = "new_sound";
            public static string DELETE_SOUND = "delete_sound";
            public static string CHANGE_SENSITIVITY = "change_sensitivity";
        }

        public class RequestCode
        {
            public static int GET_LAST_ALIVE = 201;
            public static int RECORD_NEW_SOUND = 202;
            public static int EDIT_SOUND = 203;
            public static int DELETE_SOUND = 204;
            public static int GET_SOUND_LIST = 205;
            public static int GET_SHARED_SETTINGS = 206;
            public static int SET_SHARED_SETTINGS = 207;
            public static int GET_NOTIFICATIONS = 208;
            public static int GET_ICON_LIST = 209;

            public static int REGISTER_MIC = 210;
            public static int UNREGISTER_MIC = 211;

            public static int REQUEST_TEST_NOTIF = 212;
            public static int GET_QUESTION_LIST = 213;
            public static int SUBMIT_FEEDBACK = 214;
        }
        public class ServerResponsecode
        {
            //Do not set any of these to = 0

            //errors
            public static int DATABASE_ERROR = -7;
            public static int PARSING_ERROR = -6;
            public static int DOES_NOT_EXIST = -5;
            public static int ALREADY_EXISTS = -4;
            public static int NO_SUCH_DEVICE = -3;
            public static int IMPROPERLY_FORMATTED = -2;
            public static int GENERIC_ERROR = -1;

            //success
            public static int OK = 1;

            //server initiated
            public static int NEW_NOTIFICATION = 101;
            public static int STARTED_RECORDING = 102;
            public static int DONE_RECORDING = 103;
        }

        public class WavioSharedSettings
        {
            public static string SENSITIVITY = "sensitivity";//integer
        }

        class Notification
        {
            const string ID = "id";
            const string NAME = "name";
            const string SENDER = "sender";
            const string IMAGE = "image";
            const string DATE = "date";
        }

        public class Notif
        {
            public string id;
            public string name;
            public string sender;
            public string image;
            public string date;
        }

        public class SoundInfo
        {
            public string sound_name;
            public string sound_image;
            public string sound_id;
            public string sound_requester;
        }

        public class ServerResponse
        {
            public int request
            {
                get;
                set;
            }
            public int error
            {
                get;
                set;
            }
            public string data
            {
                get;
                set;
            }
        }

        public class BroadcastReceiver : Android.Content.BroadcastReceiver
        {
            public EventHandler<BroadcastEventArgs> Receive { get; set; }

            public override void OnReceive(Context context, Intent intent)
            {
                if (Receive != null)
                    Receive(context, new BroadcastEventArgs(intent));
            }
        }

        public class BroadcastEventArgs : EventArgs
        {
            public Intent Intent { get; private set; }

            public BroadcastEventArgs(Intent intent)
            {
                Intent = intent;
            }
        }
    }
}