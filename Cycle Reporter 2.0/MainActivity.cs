using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Json;

using Xamarin.Geolocation;

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
        string lat = null;
        string lon = null;

        int usingCurrentDate = 0;
        double latDbl;
        double lonDbl;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set View To Main Layout
            SetContentView(Resource.Layout.Main);

            String serverIp = "www.bikereporter.org";
            String serverUrl = "http://" + serverIp;
            String apiUrl = serverUrl + "/api/mobile.php";

            TextView latDisplay = FindViewById<TextView>(Resource.Id.latDisplay);
            TextView lonDisplay = FindViewById<TextView>(Resource.Id.lonDisplay);
            TextView statusText = FindViewById<TextView>(Resource.Id.statusText);

            latDisplay.Text = "Lat: Is GPS Turned On?";
            lonDisplay.Text = "Lon: Is GPS Turned On?";

            //Handle Settings Button
            Button settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
            settingsButton.Click += delegate
            {
                StartActivity(typeof(Resources.Settings));
            };


            //Take User To Report View Page, On Button Click
            Button viewReports = FindViewById<Button>(Resource.Id.viewReportsButton);
            viewReports.Click += async delegate
            {

                Intent i = new Intent(Intent.ActionView);
                i.SetData(Android.Net.Uri.Parse(serverUrl + "/view"));
                await Task.Run(() => StartActivity(i));
            };


            //Handle Use Date Button
            Button useDateBtn = FindViewById<Button>(Resource.Id.useDateBtn);
            useDateBtn.Click += delegate
            {
                DateTime date = DateTime.Today;
                usingCurrentDate = 1;
            };


            //Handle Use GPS Button
            Button useGPSBtn = FindViewById<Button>(Resource.Id.gpsButton);
            useGPSBtn.Click += delegate
            {
                //LocationManager locationManager = (LocationManager) GetSystemService(LocationService);

                //if (locationManager.IsProviderEnabled(LocationService) == true)
                //{
                    var locator = new Geolocator(this) { DesiredAccuracy = 50 };
                    locator.GetPositionAsync(timeout: 10000).ContinueWith(t =>
                    {
                        latDbl = t.Result.Latitude;
                        lonDbl = t.Result.Longitude;
                        Console.WriteLine("Position Latitude: {0}", t.Result.Latitude);
                        Console.WriteLine("Position Longitude: {0}", t.Result.Longitude);
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                    lat = latDbl.ToString("R");
                    lon = lonDbl.ToString("R");
                    latDisplay.Text = "Lat: " + lat;
                    lonDisplay.Text = "Lon: " + lon;
                //}else if(locationManager.IsProviderEnabled(LocationService) == false)
                //{
                //    StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                //}
                //else
                //{
                //    Console.WriteLine("Status: What The Fuck???");
                //    statusText.Text = "Status: What The Fuck???";
                //}
            };


            //Submit Report On Button Click
            Button submitButton = FindViewById<Button>(Resource.Id.submitButton);
            TextView reportTextview = FindViewById<TextView>(Resource.Id.reportText);
            TextView usrName = FindViewById<TextView>(Resource.Id.usrName);
            TextView usrMail = FindViewById<TextView>(Resource.Id.usrMail);
            TextView perpName = FindViewById<TextView>(Resource.Id.perpName);
            TextView perpMail = FindViewById<TextView>(Resource.Id.perpMail);
            TextView plateId = FindViewById<TextView>(Resource.Id.plateBox);
            CheckBox accTick = FindViewById<CheckBox>(Resource.Id.accTick);
            Switch faultSwtch = FindViewById<Switch>(Resource.Id.faultSwch);
            submitButton.Click += async delegate{
                if (String.IsNullOrEmpty(reportTextview.Text))
                {
                    statusText.Text = "Status: Error: No Report!";
                    Console.WriteLine("Status: Submition Error: No Report Text!");
                }else if (String.IsNullOrEmpty(usrMail.Text)){
                    statusText.Text = "Status: Error: No User Name!";
                    Console.WriteLine("Status: Error: No User Name Entered");
                }else if (String.IsNullOrEmpty(usrMail.Text))
                {
                    statusText.Text = "Status: Error: No User Email!";
                    Console.WriteLine("Status: Error: No User Email Entered!");
                }else{
                    string acc = null;
                    string fault = null;
                    if(accTick.Checked == true){
                        acc = "true";
                    }
                    else if(accTick.Checked == false){
                        acc = "false";
                    }

                    if(faultSwtch.Checked == true){
                        fault = "user";
                    }
                    else if(faultSwtch.Checked == false){
                        fault = "perp";
                    }

                    string apiUrlFinal = apiUrl+"?Reprt="+reportTextview.Text+"?plateID="+plateId.Text+"?plateState="+plateState+"?incDay="+day+"?incMonth="+month+"?incYear="+year+"?perpName="+perpName.Text+"?perpMail="+perpMail.Text+"?usrName="+usrName.Text+"?usrMail="+usrMail.Text+"?incLat="+lat+"?incLon="+lon+"?acc="+acc+"?fault="+fault;

                    statusText.Text = "Status: Submiting...";
                    Console.WriteLine("Status: Submiting...");

                    JsonValue jsonReturn = await SubmitToServer(apiUrlFinal);

                    if (String.IsNullOrEmpty(jsonReturn) == false)
                    {
                        Console.WriteLine("Status: Recieved Reply From Server");
                        statusText.Text = "Status: Submitited!";
                    }else if(String.IsNullOrEmpty(jsonReturn) == true)
                    {
                        Console.WriteLine("Status: Error: No Reply From Server!");
                        statusText.Text = "Status: Error! No Reply From Server!";
                    }else{
                        Console.WriteLine("Status: Unknown Error: Problem With Detecting JSON Reply Status!");
                        statusText.Text = "Status: Error? Something Went Wrong!";
                    }
                };
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
                    this, Resource.Array.days, Android.Resource.Layout.SimpleSpinnerItem);

            daySpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            daySpinner.Adapter = daySpinnerAdapter;



            //Month Spinner
            Spinner monthSpinner = FindViewById<Spinner>(Resource.Id.monthSpnr);

            stateSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(monthSpinner_ItemSelected);
            var monthSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.months, Android.Resource.Layout.SimpleSpinnerItem);

            monthSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            monthSpinner.Adapter = monthSpinnerAdapter;



            //Year Spinner
            Spinner yearSpinner = FindViewById<Spinner>(Resource.Id.yearSpnr);

            yearSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(yearSpinner_ItemSelected);
            var yearSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.years, Android.Resource.Layout.SimpleSpinnerItem);

            yearSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            yearSpinner.Adapter = yearSpinnerAdapter;
        }

        //Set Spinner Variables
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
        
        //Submition Code!
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