using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Gms.Gcm;
using Android.Gms.Gcm.Iid;
using Android.Preferences;
using Android.Support.V4.Content;

namespace Wavio.Activities
{
    [Service(Exported = false)]
    class RegistrationIntentService : IntentService
    {


        static object locker = new object();

        public RegistrationIntentService() : base("RegistrationIntentService") { }


        protected override void OnHandleIntent(Intent intent)
        {
            try
            {
                //todo force reregister on new version

                var prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
                ISharedPreferencesEditor editor = prefs.Edit();
                


                //var activity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
                //var activity = Application.Context;
                //var activity = intent.c

                Log.Info("RegistrationIntentService", "Calling InstanceID.GetToken");
                lock (locker)
                {
                    var instanceID = InstanceID.GetInstance(this);

                    //-Check for make new instance id
                    var reInstance = prefs.GetBoolean("clear_instanceid", false);
                    if (reInstance)
                    {
                        try
                        {
                            instanceID.DeleteInstanceID();
                            editor.Remove("clear_instanceid");
                            editor.Apply();

                            instanceID = InstanceID.GetInstance(this);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    //-----------------------------

                    var token = instanceID.GetToken(
                        "45985897448", GoogleCloudMessaging.InstanceIdScope, null);// com.jmprog.hzinnovations.wavio Sender ID


                    Log.Info("RegistrationIntentService", "GCM Registration Token: " + token);
                    SendRegistrationToAppServer(token);
                    Subscribe(token);


                }
            }
            catch (Exception e)
            {
                Log.Debug("RegistrationIntentService", "Failed to get a registration token: " + e.ToString());

                var registrationComplete = new Intent("registrationComplete");
                registrationComplete.PutExtra("gcm_success", false);
                LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(registrationComplete);
                return;
            }
        }

        void SendRegistrationToAppServer(string token)
        {
            //var activity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
            Log.Debug("RegistrationIntentService", "SendRegistrationToAppServer");

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("GCMID", token);
            editor.Apply();

            Log.Debug("RegistrationIntentService", "SendRegistrationToAppServer SUCCESS");
        }

        private void RegisterResponse(String response)
        {
            //var activity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
            var registrationComplete = new Intent("registrationComplete");
            LocalBroadcastManager.GetInstance(Android.App.Application.Context).SendBroadcast(registrationComplete);
        }
        void Subscribe(string token)
        {
            var pubSub = GcmPubSub.GetInstance(Android.App.Application.Context);
            pubSub.Subscribe(token, "/topics/global", null);

            var registrationComplete = new Intent("registrationComplete");
            registrationComplete.PutExtra("gcm_success", true);

            LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(registrationComplete);
            Log.Debug("RegistrationIntentService", "Subscribe complete");
        }
    }
}