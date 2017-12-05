using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Json;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Locations;

namespace Cycle_Reporter_2._0
{
    [Activity(Label = "Cycle Reporter 2", MainLauncher = true)]
    public class MainActivity : Activity
    {
        string plateState = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            String serverIp = "www.bikereporter.org";
            String serverUrl = "http://" + serverIp;
            String apiUrl = serverUrl + "/api/mobile.php";

            // Set View To Main Layout
            SetContentView(Resource.Layout.Main);

            //Handle Settings Button
            Button settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
            settingsButton.Click += delegate
            {
                StartActivity(typeof(Resources.Settings));
            };


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
            TextView statusText = FindViewById<TextView>(Resource.Id.statusText);
            submitButton.Click += async delegate{
                string reportText = reportTextview.Text;
                if (reportText == "")
                {
                    statusText.Text = "Status: Error: No Report!";
                    Console.WriteLine("Status: Submition Error: No Report Text!");
                }
                else
                {
                    string apiUrlFinal = apiUrl + "?report=" + reportText;
                    statusText.Text = "Status: Submiting...";
                    Console.WriteLine("Status: Submiting...");
                    JsonValue json = await SubmitToServer(apiUrlFinal);
                    statusText.Text = "Status: Submitited!";
                    Console.WriteLine("Status: Submitited!");
                };
            };

            //Spinner Setup
            Spinner spinner = FindViewById<Spinner>(Resource.Id.stateSpnr);
        
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.states_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }

        //Set The State Value To Spinner Value
        void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            plateState = spinner.SelectedItem.ToString();
        }

        private async Task<JsonValue> SubmitToServer(string url)
        {
            try
            {
                // Create an HTTP web request using the URL:
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                request.ContentType = "application/json";
                request.Method = "POST";

                // Send the request to the server and wait for the response:
                Console.WriteLine("DEBUG: Contacting Server...");
                using (WebResponse response = await request.GetResponseAsync())
                {
                    // Get a stream representation of the HTTP web response:
                    Console.WriteLine("DEBUG: Fetching Response...");
                    using (Stream stream = response.GetResponseStream())
                    {
                        // Use this stream to build a JSON document object:
                        Console.WriteLine("DEBUG: Parsing Result To Json...");
                        JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                        Console.Out.WriteLine("DEBUG: Response: {0}", jsonDoc.ToString());

                        // Return the JSON document:
                        return jsonDoc;
                    }
                }
            }
            catch
            {
                Console.WriteLine("!!!HARD ERROR!!! SUBMITION FAILED: ");
                return ("!!!ERROR!!!");
            }
        }
    }
}