using Android;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using AndroidX.Core.App;
using Firebase;
using Firebase.Messaging;
using System.Net;


namespace DollarTreeApp2
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private static HttpClient? shardClient;
        private static HttpClientHandler? shardHandler;
        private static CookieContainer? cookieContainer;
        private bool fr = false;
        private string lastAction = "";
        private string firstDay = "";
        private string lastDay = "";
        private Java.Lang.Object? idkAnymore;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            FirebaseMessagingService test = new FirebaseMessagingService();
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (CheckSelfPermission(Manifest.Permission.PostNotifications) != Android.Content.PM.Permission.Granted)
                {
                    notificationPermission();
                }
            }
            
            //FixedBufferAttribute
            ActionBar?.Hide();
            var temp = LoginAsync();

            //current week
            Button bt1 = FindViewById<Button>(Resource.Id.button1);
            bt1?.Click += async (sender, e) =>
            {

                _ = CurrentWeek();
            };
            //next week
            Button bt2 = FindViewById<Button>(Resource.Id.button2);
            bt2?.Click += async (sender, e) =>
            {
                _ = NextWeek();
            };

            Button bt3 = FindViewById<Button>(Resource.Id.button3);
            bt3.Click += async (sender, e) =>
            {
                _ = LastWeek();
            };
            CheckBox bx1 = FindViewById<CheckBox>(Resource.Id.CheckBox0);

            TextView day0 = FindViewById<TextView>(Resource.Id.textViewDay0);
            TextView day1 = FindViewById<TextView>(Resource.Id.textViewDay1);
            TextView day2 = FindViewById<TextView>(Resource.Id.textViewDay2);
            TextView day3 = FindViewById<TextView>(Resource.Id.textViewDay3);
            TextView day4 = FindViewById<TextView>(Resource.Id.textViewDay4);
            TextView day5 = FindViewById<TextView>(Resource.Id.textViewDay5);
            TextView day6 = FindViewById<TextView>(Resource.Id.textViewDay6);

            day0.Click += onDayClick;
            day1.Click += onDayClick;
            day2.Click += onDayClick;
            day3.Click += onDayClick;
            day4.Click += onDayClick;
            day5.Click += onDayClick;
            day6.Click += onDayClick;

            bx1.CheckedChange += async (sender, e) =>
            {
                fr = !fr;
                switch (lastAction)
                {
                    case "Current":
                        _ = CurrentWeek();
                        break;
                    case "Next":
                        _ = NextWeek(firstDay, lastDay);
                        break;
                    case "Last":
                        _ = LastWeek(firstDay, lastDay);
                        break;
                    default:
                        _ = LoginAsync();
                        break;
                }

            };
            Test();
        }
        
        private void onDayClick(object sender, EventArgs e)
        {
            var clickedDay = sender as TextView;
            if (clickedDay != null)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle(clickedDay.Text);
                builder.SetMessage(clickedDay.Tag.ToString());

                builder.SetNegativeButton("close", (dialog, _) =>
                {
                    //blank ?
                });
                AlertDialog alert = builder.Create();
                alert.Show();
            }
        }

        private async void Test()
        {
            FirebaseApp.InitializeApp(this);
   
            try
            {
                idkAnymore = await FirebaseMessaging.Instance.GetToken();
                string filePath = System.IO.Path.Combine("/storage/emulated/0/Download/", "Token.txt");
                if (!File.Exists(filePath))
                {
                    var intent = new Intent(Intent.ActionCreateDocument);
                    intent.AddCategory(Intent.CategoryOpenable);
                    intent.SetType("text/plain");
                    intent.PutExtra(Intent.ExtraTitle, "Token.txt");
                    StartActivityForResult(intent, 1);
                }
                else
                {
                    Console.WriteLine("we should never get here");
                    //this is blank for no reason
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
               //just have to reload not sure i can code anything in just have the user open twice
            }
            
        }
        
        //the url we get redirected to when logedout https://compassmobile.dollartree.com/pwm/action/EmpLoginMobile
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 1 && resultCode == Result.Ok && data?.Data != null)
            {
                Android.Net.Uri uri = data.Data;
                using (var stream = ContentResolver.OpenOutputStream(uri))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(idkAnymore.ToString());
                }
            }
        }
        
        public void notificationPermission()
        {
            ActivityCompat.RequestPermissions(this, [Manifest.Permission.PostNotifications], 0);
        }
        
        private async Task<int> LoginAsync()
        {
            var url = "https://compassmobile.dollartree.com/pwm/action/employeeLogin";
            var loginInfo = new Dictionary<string, string>
                {
                //change
                    { "username", "" },
                    { "password", ""}
                };
            cookieContainer = new CookieContainer();
            shardHandler = new HttpClientHandler { AllowAutoRedirect = true, UseCookies = true, CookieContainer = cookieContainer };
            shardClient = new HttpClient(shardHandler);
            var postConent = new FormUrlEncodedContent(loginInfo);
            var response = await shardClient.PostAsync(url, postConent);

            _ = CurrentWeek();
            return 0;
        }

        public async Task CurrentWeek()
        {
            var url = "https://compassmobile.dollartree.com/pwm/action/employeeHome?navID=10001";
            var response = await shardClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.RequestMessage.RequestUri.ToString() == "https://compassmobile.dollartree.com/pwm/action/EmpLoginMobile")
            {
                _ = LoginAsync();
                _ = CurrentWeek();
            }
            else
            {
                UpdateTable(responseString);
                lastAction = "Current";
            }
        }

        public async Task NextWeek(string startDate = "", string endDate = "")
        {
            var currentYear = "2026";

            var url = "https://compassmobile.dollartree.com/pwm/action/employeeHome";

            if (startDate == "")
            {
                startDate = FindViewById<TextView>(Resource.Id.textViewDay0).Text;
                startDate = startDate.Substring(3) + "/" + currentYear;
                endDate = FindViewById<TextView>(Resource.Id.textViewDay6).Text;
                endDate = endDate.Substring(3) + "/" + currentYear;
                firstDay = startDate;
                lastDay = endDate;
            }
            var nextWeek = new Dictionary<string, string>
            {
                { "startDate", startDate },
                { "chkInScopeForDateRange", startDate},
                { "endDate", endDate},
                {"changeTimePeriod", "N" },
                {"selected_empid", "3483356" }
            };

            var postConent = new FormUrlEncodedContent(nextWeek);
            var temp = await shardClient.PostAsync(url, postConent);
            var responseString = await temp.Content.ReadAsStringAsync();

            if (temp.RequestMessage.RequestUri.ToString() == "https://compassmobile.dollartree.com/pwm/action/EmpLoginMobile")
            {
                _ = LoginAsync();
                NextWeek(startDate, endDate);
            }
            else
            {
                UpdateTable(responseString);
                lastAction = "Next";
            }
        }

        public async Task LastWeek(string startDate = "", string endDate = "")
        {
            var url = "https://compassmobile.dollartree.com/pwm/action/employeeHome";
            if (startDate == "")
            {
                startDate = FindViewById<TextView>(Resource.Id.textViewDay0).Text;
                startDate = startDate.Substring(3) + "/2026";
                endDate = FindViewById<TextView>(Resource.Id.textViewDay6).Text;
                endDate = endDate.Substring(3) + "/2026";
                firstDay = startDate;
                lastDay = endDate;
            }

            var nextWeek = new Dictionary<string, string>
            {
                { "startDate", startDate },
                { "chkInScopeForDateRange", startDate},
                { "endDate", endDate},
                {"changeTimePeriod", "P" },
                {"selected_empid", "3483356" }
            };
            var postConent = new FormUrlEncodedContent(nextWeek);
            var temp = await shardClient.PostAsync(url, postConent);
            var responseString = await temp.Content.ReadAsStringAsync();
            if (temp.RequestMessage.RequestUri.ToString() == "https://compassmobile.dollartree.com/pwm/action/EmpLoginMobile")
            {
                _ = LoginAsync();
                _ = LastWeek(startDate, endDate);
            }
            else
            {
                UpdateTable(responseString);
                lastAction = "Last";
            }
        }

        private void UpdateTable(string responseString)
        {
            var startingPos = responseString.IndexOf("colHeaderPlain", 0);
            var endingPos = responseString.IndexOf("</td>", startingPos);
            int startingPosHours;
            int endingPosHours;
            var returnString = "";
            var dumddumd = "class=\"colHeader\">";
            var dumdHours = "valign=\"top\">";
            int offset;
            var replaceString = "&nbsp;";
            string location;
            string comment;
            
            offset = 4;
            startingPosHours = responseString.IndexOf("Sched:", 0);
            endingPosHours = responseString.IndexOf("</td>", startingPosHours);

            if (!fr)
            {
                startingPosHours = responseString.IndexOf("Sched:", 0);
                endingPosHours = responseString.IndexOf("</td>", startingPosHours);
            }
            else
            {
                offset = 18;
                startingPosHours = responseString.IndexOf("Actual:", 0);
                endingPosHours = responseString.IndexOf("</td>", startingPosHours);
            }
            //for getting the dates and amount of hours worked
            for (int i = 0; i < 8; i++)
            {
                string textViewNum = "textViewDay" + i.ToString();
                string textViewHours = "textViewHours" + i.ToString();
                //getting days
                startingPos = responseString.IndexOf("<td", endingPos);
                if (startingPos > 0)
                    endingPos = responseString.IndexOf("</td>", startingPos);
                if (startingPos > 0 && endingPos > 0)
                {
                    returnString = responseString.Substring(startingPos + 4, (endingPos - startingPos) - 4);
                    if (returnString.Contains(dumddumd))
                    {
                        returnString = returnString.Substring(dumddumd.Length);
                    }
                    returnString = returnString.Trim();
                    int templower = Resources.GetIdentifier(textViewNum, "id", this.PackageName);
                    var textViewerCurrent = (TextView)FindViewById(templower);

                    textViewerCurrent.Text = returnString;
                }
                //gettting total hours
                startingPosHours = responseString.IndexOf("<td", endingPosHours);
                if (startingPosHours > 0)
                    endingPosHours = responseString.IndexOf("</td>", startingPosHours);
                if (startingPosHours > 0 && endingPosHours > 0)
                {
                    int tempoffset = responseString.IndexOf(">", startingPosHours);
                    int secondTemp = tempoffset - startingPosHours;
                    offset = secondTemp + 1;

                    returnString = responseString.Substring(startingPosHours + offset, (endingPosHours - startingPosHours) - offset);
                    if (returnString.Contains(dumdHours))
                    {
                        returnString = returnString.Substring(dumdHours.Length);
                    }
                    returnString = returnString.Trim();

                    int templower = Resources.GetIdentifier(textViewHours, "id", this.PackageName);
                    var textViewerCurrent = (TextView)FindViewById(templower);
                    if (returnString.Contains(replaceString))
                    {
                        returnString = returnString.Substring(0, returnString.IndexOf(replaceString));
                        returnString = returnString.Trim();
                    }
                    if (returnString != "")
                    {
                        var temp = calucateMoney(returnString);
                        textViewerCurrent.Text = '\n' + "(" + returnString + ") " + temp;
                    }
                }
            }
            //for the actual schedule
            if (!fr)
            {
                startingPos = responseString.IndexOf("Schedule Times:", 0);
                endingPos = responseString.IndexOf("</td>", startingPos);
            }
            else
            {
                startingPos = responseString.IndexOf("Actual Times:", 0);
                endingPos = responseString.IndexOf("</td>", startingPos);
            }
            var startingACTIVITY = responseString.IndexOf("Activity:", 0);
            var endingActivityPos = responseString.IndexOf("</td>", startingACTIVITY);

            var startingLocation = responseString.IndexOf("Location:", 0);
            var endingLocation = responseString.IndexOf("</td>", startingLocation);

            var startingComment = responseString.IndexOf("Comments:", 0);
            var endingComment = responseString.IndexOf("</td>", startingComment);

            dumddumd = "<br>";

            for (int i = 0; i < 7; i++)
            {
                //this block gets position
                string textViewJob = "textViewJob" + i.ToString();
                startingACTIVITY = responseString.IndexOf("<td", endingActivityPos);
                if (startingACTIVITY > 0)
                    endingActivityPos = responseString.IndexOf("</td>", startingACTIVITY);
                if (endingActivityPos > 0 && startingACTIVITY > 0)
                {
                    returnString = responseString.Substring(startingACTIVITY + 17, (endingActivityPos - startingACTIVITY) - 17);
                    if (returnString.Contains(dumddumd))
                    {
                        returnString = returnString.Substring(0, returnString.IndexOf(dumddumd));
                    }
                    returnString = returnString.Trim();

                    int templower = Resources.GetIdentifier(textViewJob, "id", this.PackageName);

                    var textViewerCurrent = (TextView)FindViewById(templower);
                    if (returnString.Contains(replaceString))
                        textViewerCurrent.Text = "";
                    else
                        textViewerCurrent.Text = "(" + returnString[0] + ")";
                }
                //this gets hours
                string textViewNum = "textViewHours" + i.ToString();
                startingPos = responseString.IndexOf("<td", endingPos);
                if (startingPos > 0)
                    endingPos = responseString.IndexOf("</td>", startingPos);
                if (!fr)
                {
                    if (endingPos > 0 && startingPos > 0)
                    {
                        returnString = responseString.Substring(startingPos + 17, (endingPos - startingPos) - 17);
                        if (returnString.Contains(dumddumd))
                        {
                            returnString = returnString.Substring(0, returnString.IndexOf(dumddumd));
                        }
                        returnString = returnString.Trim();
                        int templower = Resources.GetIdentifier(textViewNum, "id", this.PackageName);
                        var textViewerCurrent = (TextView)FindViewById(templower);
                        if (returnString.Contains(replaceString))
                            textViewerCurrent.Text = GetString(Resource.String.app_work);
                        else
                            textViewerCurrent.Text = returnString + textViewerCurrent.Text;
                    }
                }
                else
                {
                    if (endingPos > 0 && startingPos > 0)
                    {
                        int tempoffset = responseString.IndexOf(">", startingPos);
                        int secondTemp = tempoffset - startingPos;
                        offset = secondTemp + 1;
                        returnString = responseString.Substring(startingPos + offset, (endingPos - startingPos) - offset);

                        if (returnString.Contains("<a "))
                        {
                            returnString = returnString.Substring(returnString.IndexOf(">") + 1, returnString.IndexOf("</a>") - returnString.IndexOf(">"));
                            returnString = returnString.Substring(0, returnString.Length - 1);
                            returnString = returnString.Trim();

                        }
                        returnString = returnString.Trim();
                        int templower = Resources.GetIdentifier(textViewNum, "id", this.PackageName);
                        var textViewerCurrent = (TextView)FindViewById(templower);
                        int templowerWork = Resources.GetIdentifier(textViewJob, "id", this.PackageName);

                        var textViewerCurrentWork = (TextView)FindViewById(templowerWork);
                        if (returnString.Contains(replaceString) && textViewerCurrentWork.Text == "")
                            textViewerCurrent.Text = GetString(Resource.String.no_work);
                        else if (returnString.Contains(replaceString) && textViewerCurrentWork.Text != "")
                            textViewerCurrent.Text = GetString(Resource.String.no_work_yet);
                        else
                            textViewerCurrent.Text = returnString + textViewerCurrent.Text;
                    }
                }

                string textViewDay = "textViewDay" + i.ToString();
                int newtempLower = Resources.GetIdentifier(textViewDay, "id", this.PackageName);
                var newtextViewerCurrent = (TextView)FindViewById(newtempLower);
                newtextViewerCurrent.Tag = "";
                //this block gets location
                startingLocation = responseString.IndexOf("<td", endingLocation);
                if (startingLocation > 0)
                    endingLocation = responseString.IndexOf("</td>", startingLocation);
                if (endingLocation>0 && startingLocation >0)
                {
                    returnString = responseString.Substring(startingLocation + 18, (endingLocation - startingLocation) - 18);
                    if (returnString.Contains(dumddumd))
                    {
                        returnString = returnString.Substring(0,returnString.IndexOf(dumddumd));
                    }
                    returnString = returnString.Trim();
                    int tempLower = Resources.GetIdentifier(textViewDay, "id", this.PackageName);
                    var textViewerCurrent = (TextView)FindViewById(tempLower);
                    if (returnString.Contains(replaceString))
                        textViewerCurrent.Tag = "";
                    else
                        textViewerCurrent.Tag = textViewerCurrent.Tag+ " "+returnString;
                }
                
                //this blcok gets comments
                startingComment = responseString.IndexOf("<td", endingComment);
                if (startingComment > 0)
                    endingComment = responseString.IndexOf("</td>", startingComment);
                if (endingComment > 0 && startingComment>0)
                {
                    returnString= responseString.Substring(startingComment+18,(endingComment - startingComment) -18);
                    returnString = returnString.Trim();
                    string test = "<BR>";
                    // dumddumd = "<br>";
                    if (returnString.Contains(test))
                    {
                        returnString = returnString.Substring(0, returnString.IndexOf(test));
                    }
                    returnString = returnString.Trim();
                    int tempLower = Resources.GetIdentifier(textViewDay, "id", this.PackageName);
                    var textViewerCurrent = (TextView)FindViewById(tempLower);

                    if (returnString.Contains(replaceString))
                        textViewerCurrent.Tag = textViewerCurrent.Tag + "";
                    else
                        textViewerCurrent.Tag = textViewerCurrent.Tag + "\n"+returnString;
                }
                
            }
        }
        //the input format will be 0:00
        // take hours and times in by 15.25, take min divide by 60 and times by 15.25
        private string calucateMoney(string hoursInput)
        {
            double pay = 15.50;

            int hours = int.Parse(hoursInput.Substring(0, hoursInput.IndexOf(":")));
            int tempMin = 0;
            if (hoursInput.Contains(":"))
            {
                string temp2 = hoursInput.Substring((hoursInput.IndexOf(":")) + 1);
                tempMin = int.Parse(temp2);
            }
            double Min = tempMin / 60.0;
            double returnValue = (hours * pay) + (Min * pay);

            if (returnValue > 0.0)
            {

                return ("($" + returnValue.ToString("#.00") + ")");
            }
            return "";
        }
    }
}
