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

namespace Cycle_Reporter_2.Resources
{
    [Activity(Label = "Settings", Icon = "@drawable/settings")]
    public class Settings : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            Switch offlineSupport = FindViewById<Switch>(Resource.Id.offlineSupportSwitch);
            if (Application.Context.GetString(Resource.String.offlineSupport) == "false"){
                offlineSupport.Checked = false;
            }
            else
            {
                offlineSupport.Checked = true;
            };

            //Handle GPS Settings Button
            Button viewGPSBtn = FindViewById<Button>(Resource.Id.viewGPSButton);
            viewGPSBtn.Click += delegate
            {
                StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings));
            };

            //Handle Save Button
            Button settingsSaveButton = FindViewById<Button>(Resource.Id.settingsSaveButton);
            settingsSaveButton.Click += delegate
            {
                Finish();
            };
        }
    }
}