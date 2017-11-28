using Android.App;
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

            String serverIp = "www.cyclereporter.net";
            String serverUrl = "http://" + serverIp;
            String apiUrl = serverUrl + "/api/mobile.php?";

            // Set View To Main Layout
            SetContentView(Resource.Layout.Main);

            //Take User To Report View Page, On Button Click
            Button viewReports = FindViewById<Button>(Resource.Id.viewReportsButton);
            viewReports.Click += async (sender, e) => {
                Intent i = new Intent(Intent.ActionView);
                i.SetData(Android.Net.Uri.Parse(serverUrl + "/view"));
                StartActivity(i);
            };

            Button submitButton = FindViewById<Button>(Resource.Id.submitButton);
            submitButton.Click += async (sender, e) => {
                //Submition Code Here
            };
        }
    }
}

