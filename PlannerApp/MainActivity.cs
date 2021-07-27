using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;


namespace PlannerApp
{
    [Activity(Label = "Main", Theme = "@style/AppTheme", MainLauncher = true)]

    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}