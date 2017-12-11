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
    [Activity(Label = "Cycle Reporter 2", MainLauncher = true, Icon = "@drawable/icons/icon")]
    public class MainActivity : Activity
    {



        //Date Picker Code
        public class DatePickerFragment : DialogFragment,
                                  DatePickerDialog.IOnDateSetListener
        {
            // TAG can be any string of your choice.
            public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

            // Initialize this value to prevent NullReferenceExceptions.
            Action<DateTime> _dateSelectedHandler = delegate { };

            public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected)
            {
                DatePickerFragment frag = new DatePickerFragment();
                frag._dateSelectedHandler = onDateSelected;
                return frag;
            }

            public override Dialog OnCreateDialog(Bundle savedInstanceState)
            {
                DateTime currently = DateTime.Now;
                DatePickerDialog dialog = new DatePickerDialog(Activity,
                                                               this,
                                                               currently.Year,
                                                               currently.Month - 1,
                                                               currently.Day);
                return dialog;
            }

            public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
            {
                // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
                DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
                Console.Write("Debug: " + (TAG, selectedDate.ToLongDateString()));
                _dateSelectedHandler(selectedDate);
            }
        }



        string plateState = null;
        string day = null;
        string month = null;
        string year = null;
        string lat = null;
        string lon = null;

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
            TextView dateDisplay = FindViewById<TextView>(Resource.Id.dateDisplay);
            Button useGPSBtn = FindViewById<Button>(Resource.Id.gpsButton);

            var locator = new Geolocator(this) { DesiredAccuracy = 50 };

            latDisplay.Text = "Lat: ";
            lonDisplay.Text = "Lon: ";



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



            //Set Date Automaticly
            DateTime autoDateResult = DateTime.Today;
            day = "" + autoDateResult.Day;
            month = "" + autoDateResult.Month;
            year = "" + autoDateResult.Year;
            dateDisplay.Text = "Date: " + month + "/" + day + "/" + year;



            //Handle Set Date Button
            Button manualDateBtn = FindViewById<Button>(Resource.Id.manualDateBtn);
            manualDateBtn.Click += delegate
            {
                DatePickerFragment dateFrag = DatePickerFragment.NewInstance(delegate (DateTime dateResult)
                {
                    day = "" + dateResult.Day;
                    month = "" + dateResult.Month;
                    year = "" + dateResult.Year;
                    dateDisplay.Text = "Date: " + month + "/" + day + "/" + year;
                });
                dateFrag.Show(FragmentManager, DatePickerFragment.TAG);
            };


            //Make An Attempt To Get GPS Location, If Not, Ask If GPS Is On...
            locator.GetPositionAsync(timeout: 10000).ContinueWith(t =>
            {
                latDbl = t.Result.Latitude;
                lonDbl = t.Result.Longitude;
                lat = latDbl.ToString("R");
                lon = lonDbl.ToString("R");
                latDisplay.Text = "Lat: " + lat;
                lonDisplay.Text = "Lon: " + lon;
                Console.WriteLine("Automaticly Finding Location By GPS...");
            }, TaskScheduler.FromCurrentSynchronizationContext());
            if (String.IsNullOrEmpty(lat) == true)
            {
                useGPSBtn.Text = "Retry GPS";
                latDisplay.Text = "Lat: Is GPS Enabled?";
                lonDisplay.Text = "Lon: Is GPS Enabled?";
            }else{
                useGPSBtn.Text = "Update GPS";
            }

            //Handle Use GPS Button
            useGPSBtn.Click += delegate
            {
                LocationManager locationManager = (LocationManager) GetSystemService(LocationService);

                locator.GetPositionAsync(timeout: 10000).ContinueWith(t =>
                {
                    lat = "" + t.Result.Latitude;
                    lon = "" + t.Result.Longitude;
                    latDisplay.Text = "Lat: " + lat;
                    lonDisplay.Text = "Lon: " + lon;
                    Console.WriteLine("Request Made For Current Location By User... Finding...");
                }, TaskScheduler.FromCurrentSynchronizationContext());
                if (String.IsNullOrEmpty(lat) == true) {
                    useGPSBtn.Text = "Retry GPS";
                } else{
                    useGPSBtn.Text = "Update GPS";
                }
            };


            //Submit Report On Button Click
            Button submitButton = FindViewById<Button>(Resource.Id.submitButton);
            TextView reportTextview = FindViewById<TextView>(Resource.Id.reportText);
            TextView usrName = FindViewById<TextView>(Resource.Id.usrName);
            TextView usrMail = FindViewById<TextView>(Resource.Id.usrMail);
            TextView plateId = FindViewById<TextView>(Resource.Id.plateBox);
            CheckBox contTick = FindViewById<CheckBox>(Resource.Id.contTick);
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
                    string cont = null;
                    if(contTick.Checked == true){
                        cont = "true";
                    }else if(contTick.Checked == false){
                        cont = "true";
                    }

                    string apiUrlFinal = apiUrl+"?Reprt="+reportTextview.Text+"&plateID="+plateId.Text+"&plateState="+plateState+"&incDay="+day+"&incMonth="+month+"&incYear="+year+"&usrName="+usrName.Text+"&usrMail="+usrMail.Text+"&incLat="+lat+"&incLon="+lon+"&cont="+cont;

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


/*
            //Day Spinner
            stateSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(daySpinner_ItemSelected);
            var daySpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.days, Android.Resource.Layout.SimpleSpinnerItem);

            daySpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            daySpinner.Adapter = daySpinnerAdapter;



            //Month Spinner
            stateSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(monthSpinner_ItemSelected);
            var monthSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.months, Android.Resource.Layout.SimpleSpinnerItem);

            monthSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            monthSpinner.Adapter = monthSpinnerAdapter;



            //Year Spinner
            yearSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(yearSpinner_ItemSelected);
            var yearSpinnerAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.years, Android.Resource.Layout.SimpleSpinnerItem);

            yearSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            yearSpinner.Adapter = yearSpinnerAdapter;
*/
        }



        //Set Spinner Variables
        //State Spinner
        void stateSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner stateSpinner = (Spinner)sender;
            plateState = stateSpinner.SelectedItem.ToString();
        }

/*
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
*/

            
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