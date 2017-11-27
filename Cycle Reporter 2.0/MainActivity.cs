﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using System.Runtime.Remoting.Contexts;
using System;
using Android.Content;




namespace Cycle_Reporter_2._0
{
    [Activity(Label = "Cycle Reporter 2", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);

            String serverIp = "192.168.43.133`";
            String serverUrl = "http://192.168.43.133";

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button viewReports = FindViewById<Button>(Resource.Id.viewReportsButton);
            viewReports.Click += delegate
            {
                Intent i = new Intent(Intent.ActionView);
                i.SetData(Android.Net.Uri.Parse(serverUrl));
                StartActivity(i);
            };
        }
    }
}

