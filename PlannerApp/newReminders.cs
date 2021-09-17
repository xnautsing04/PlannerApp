using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using PlannerApp.Models;
using PlannerApp.Data;

namespace PlannerApp
{
    [Activity(Label = "reminder", MainLauncher = false, Theme = "@style/AppTheme")]
    
    //This class is used to deal with new reminders that are added when the user clicks the plus button on the MainActivity.
    public class newReminders : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.newReminder);

            //Find the EditText views for all the elements that are typed in and stored as a reminder.
            var editName = FindViewById<EditText>(Resource.Id.editTextName);
            var editDesc = FindViewById<EditText>(Resource.Id.editTextDesc);
            var editDate = FindViewById<EditText>(Resource.Id.editTextDate);
            var editTime = FindViewById<EditText>(Resource.Id.editTextTime);

            //Find both the save and back button views.
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);
            var backButton = FindViewById<Button>(Resource.Id.backButton);

            //If the saveButton is clicked, the values in the editText boxes must be added to the database.
            saveButton.Click += async(e, o) =>
            {
                //Create a new reminder, using the values currently in the text boxes inputted by the user.
                Reminder newReminder = new Reminder();
                newReminder.name = editName.Text;
                newReminder.description = editDesc.Text;
                newReminder.date = editDate.Text;
                newReminder.time = editTime.Text;
                newReminder.selected = false;

                //Gather the current instance of the database and save the item asynchronously.
                ReminderDatabase database = await ReminderDatabase.Instance;
                await database.SaveItemAsync(newReminder);

                //Return back to the MainActivity.
                Intent nextActivity = new Intent(this, typeof(InitiateCalendar));
                StartActivity(nextActivity);
            };

            //If the backButton is clicked, return to MainActivity without accessing anything to the database.
            backButton.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(InitiateCalendar));
                StartActivity(nextActivity);
            };
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}