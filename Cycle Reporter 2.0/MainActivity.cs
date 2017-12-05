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
        string day = null;
        string month = null;
        string year = null;

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

            //Handle Use Date Button
            Button useDateBtn = FindViewById<Button>(Resource.Id.useDateBtn);
            useDateBtn.Click += delegate
            {

            };

            //Spinner Setup
            //State Spinner
            Spinner stateSpinner = FindViewById<Spinner>(Resource.Id.stateSpnr);
        
            stateSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(stateSpinner_ItemSelected);
            var stateSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.states_array, Android.Resource.Layout.SimpleSpinnerItem);

            stateSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            stateSpinner.Adapter = stateSpinnerAdapter;



            //Day Spinner
            Spinner daySpinner = FindViewById<Spinner>(Resource.Id.daySpnr);

            stateSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(daySpinner_ItemSelected);
            var daySpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.states_array, Android.Resource.Layout.SimpleSpinnerItem);

            daySpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            daySpinner.Adapter = daySpinnerAdapter;



            //Month Spinner
            Spinner monthSpinner = FindViewById<Spinner>(Resource.Id.monthSpnr);

            stateSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(monthSpinner_ItemSelected);
            var monthSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.states_array, Android.Resource.Layout.SimpleSpinnerItem);

            monthSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            monthSpinner.Adapter = monthSpinnerAdapter;



            //Year Spinner
            Spinner yearSpinner = FindViewById<Spinner>(Resource.Id.yearSpnr);

            yearSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(yearSpinner_ItemSelected);
            var yearSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.states_array, Android.Resource.Layout.SimpleSpinnerItem);

            yearSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            yearSpinner.Adapter = yearSpinnerAdapter;
        }

        //Set Spinner Arrays
        //State Spinner
        void stateSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner stateSpinner = (Spinner)sender;
            plateState = stateSpinner.SelectedItem.ToString();
        }

        //Day Spinner
        void daySpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner daySpinner = (Spinner)sender;
            day = daySpinner.SelectedItem.ToString();
        }

        //Month Spinner
        void monthSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner monthSpinner = (Spinner)sender;
            month = monthSpinner.SelectedItem.ToString();
        }

        //Year Spinner
        void yearSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner yearSpinner = (Spinner)sender;
            year = yearSpinner.SelectedItem.ToString();
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