using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using Android.Content;
using Nito.AsyncEx;
using System;

//This activity is meant to display all of the reminders for a selected day. Each reminder will have a checkmark, so that
//clicking the Delete button will reset and remove the elements from the SQLite Database. A back button also exists, sending the user
//to MainActivity without deleting any buttons.

namespace PlannerApp
{
    //This class holds the views that are to be used by individual elements in the recyclerview. This includes the title, description
    //, Date, Time, and boolean value for whether it has been selected for deletion.
    public class ReminderGroupHolder : RecyclerView.ViewHolder
    {
        public TextView Title { get; private set; }
        public TextView Description { get; private set; }
        public TextView Date { get; private set; }
        public TextView Time { get; private set; }
        public CheckBox selected { get; private set; }

        //This is the constructor that sends the listener for the itemClicks and the individual view to be applied.
        public ReminderGroupHolder(View itemView, Action<int> listener) : base(itemView)
        {
            Title = itemView.FindViewById<TextView>(Resource.Id.textViewTitle);
            Description = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
            Date = itemView.FindViewById<TextView>(Resource.Id.textViewDate);
            Time = itemView.FindViewById<TextView>(Resource.Id.textViewEndTime);
            selected = itemView.FindViewById<CheckBox>(Resource.Id.checkBoxDelete);

            selected.Click += (sender, e) => listener(base.LayoutPosition); 
            //This is the button that listens for the CheckBox attached to each element
        }




    }

    //This class is the adapter used by the recyclerview; this is used to both track itemClicks and
    //apply the individual reminders to the views
    public class ReminderGroupAdapter : RecyclerView.Adapter
    {
        public ReminderGroup mReminderGroup; //This stores all the current reminders to be loaded.
        public event EventHandler<int> itemClick; //This is the eventHandler for checking which view is checkmarked.

        //This is the constructor that stores the given reminderGroup to the mReminderGroup variable.
        public ReminderGroupAdapter(ReminderGroup reminderGroup)
        {
            mReminderGroup = reminderGroup;
        }

        // This creates an instance of a reminderGroupHolder so it is able to apply all of the elements in the view to the
        // given parameters of each Reminder element.
        public override RecyclerView.ViewHolder
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemholder = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.row, parent, false);
            ReminderGroupHolder vh = new ReminderGroupHolder(itemholder, OnClick);
            return vh; //This returns the new view to be displayed.
        }

        //This function puts the current reminder onto the current view holder; as the views are re-used, this is called so
        //that the views display the next (or previous) reminder to create the illusion of a continuous list of items.
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ReminderGroupHolder vh = holder as ReminderGroupHolder;
            vh.Title.Text = mReminderGroup[position].name;
            vh.Description.Text = mReminderGroup[position].description;
            vh.Date.Text = mReminderGroup[position].date;
            vh.Time.Text = mReminderGroup[position].time;
        }

        //This function returns the amount of reminders within mReminderGroup.
        public override int ItemCount
        {
            get { return mReminderGroup.numReminders; }
        }

        //If one of the views is clicked (and the click is not null), it will call the itemClick function to either check or uncheck
        //the checkbox on the view and in the reminderGroup itself
        void OnClick (int position)
        {
            if (itemClick != null)
                itemClick(this, position);
        }
    }

    [Activity(Label = "activityTwo", Theme = "@style/AppTheme")]

    //The main class for this activity. It deals with the individual pages that lists the reminders for a specific day.
    public class activityTwo : AppCompatActivity
    {
        RecyclerView mRecyclerView; //The recyclerView object that is displayed in the .xml file.
        RecyclerView.LayoutManager mLayoutManager; //The layoutManager that is used by the recyclerView.
        ReminderGroupAdapter mAdapter; //The adapter that is used by the recyclerView; this class is defined above.
        public ReminderGroup mReminderGroup; //The reminderGroup that is created to store the individual reminders to be displayed.
        
        //This is the function that begins when the activity starts.
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the activityTwo resource
            SetContentView(Resource.Layout.activityTwo);

            //This gathers the stored date information given by the MainActivity activity.
            string month = Intent.GetStringExtra("month");
            string day = Intent.GetStringExtra("day");
            string year = Intent.GetStringExtra("year");

            //This converts the number form into a word form by using the function defined in MainActivity.
            string monthText = MainActivity.findMonthText(month);

            //This combines the strings and uses it to display atop the screen.
            string fullDate = monthText + " " + day + ", " + year;

            var clickedDate = FindViewById<TextView>(Resource.Id.viewDate);
            clickedDate.Text = fullDate;

            //This finds the recyclerView to be displayed.
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            //This create the reminderGroup, the date is used to decide which reminders will show up
            mReminderGroup = new ReminderGroup(month, day, year); 
                
            //This sets the layoutManager to a linearLayout.
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            //Create the adapter, and subscrbe the click function to the necessary functions in mReminderGroup and
            //then set the adapter of the recyclerView to this item.
            mAdapter = new ReminderGroupAdapter(mReminderGroup);
            mAdapter.itemClick += mReminderGroup.HandleCustomEvent;
            mRecyclerView.SetAdapter(mAdapter);


            //If the backButton is clicked, go back to mainActivity.
            var backButton = FindViewById<Button>(Resource.Id.backButton);
            backButton.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(InitiateCalendar));
                StartActivity(nextActivity);
            };

            //If the delete button is clicked, run an asynchronous function to delete the checked items. Then, reload this activity.
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            deleteButton.Click += (s, e) =>
            {
                AsyncContext.Run(mReminderGroup.deleteCheckmarked);

                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", month);
                nextActivity.PutExtra("day", day);
                nextActivity.PutExtra("year", year);
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