using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using System;

//This will initiate the calendar with the current month, date, and year to be stored for MainActivity

namespace PlannerApp
{
    [Activity(Label = "Init", MainLauncher = true, Theme = "@style/AppTheme")]

    public class InitiateCalendar : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //Find the current month, date, and year from the current time
            string month = DateTime.Now.ToString("MM");
            string date = DateTime.Now.ToString("dd");
            string year = DateTime.Now.ToString("yyyy");

            Intent nextActivity = new Intent(this, typeof(MainActivity));
            nextActivity.PutExtra("month", month);
            nextActivity.PutExtra("date", date);
            nextActivity.PutExtra("year", year);
            StartActivity(nextActivity);
        }
    }
}