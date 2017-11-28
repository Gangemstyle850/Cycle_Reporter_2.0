using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using System.Runtime.Remoting.Contexts;
using System;
using Android.Content;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Json;

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
            String apiUrl = serverUrl + "/api/mobile.php";

            // Set View To Main Layout
            SetContentView(Resource.Layout.Main);

            //Take User To Report View Page, On Button Click
            Button viewReports = FindViewById<Button>(Resource.Id.viewReportsButton);
            viewReports.Click += async delegate {
                Intent i = new Intent(Intent.ActionView);
                i.SetData(Android.Net.Uri.Parse(serverUrl + "/view"));
                StartActivity(i);
            };

            //Submit Report On Button Click
            Button submitButton = FindViewById<Button>(Resource.Id.submitButton);
            TextView reportTextview = FindViewById<TextView>(Resource.Id.reportText);
            submitButton.Click += async delegate{
                string reportText = reportTextview.Text;
                if (reportText == "") {
                    string apiUrlFinal = apiUrl + "?report=" + reportText;
                    JsonValue json = await SubmitToServer(apiUrlFinal);
                };
            };
        }
        private async Task<JsonValue> SubmitToServer(string url)
        {
            // Create an HTTP web request using the URL:
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "POST";

            // Send the request to the server and wait for the response:
            using (WebResponse response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());

                    // Return the JSON document:
                    return jsonDoc;
                }
            }
        }
    }
}

