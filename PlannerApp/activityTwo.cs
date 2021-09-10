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

namespace PlannerApp
{
    //this class holds the views that are to be used by individual elements in the recyclerview
    public class ReminderGroupHolder : RecyclerView.ViewHolder
    {
        public TextView Title { get; private set; }
        public TextView Description { get; private set; }
        public TextView startTime { get; private set; }
        public TextView endTime { get; private set; }
        public CheckBox selected { get; private set; }

        //constructor that sends the listener for the itemClicks and the individual view to be applied
        public ReminderGroupHolder(View itemView, Action<int> listener) : base(itemView)
        {
            Title = itemView.FindViewById<TextView>(Resource.Id.textViewTitle);
            Description = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
            startTime = itemView.FindViewById<TextView>(Resource.Id.textViewDate);
            endTime = itemView.FindViewById<TextView>(Resource.Id.textViewEndTime);
            selected = itemView.FindViewById<CheckBox>(Resource.Id.checkBoxDelete);

            selected.Click += (sender, e) => listener(base.LayoutPosition); //button that listens for the CheckBox attached to each element
        }




    }

    //this class is the adapter used by the recyclerview; this is used to both track itemClicks and
    //apply the individual reminders to the views
    public class ReminderGroupAdapter : RecyclerView.Adapter
    {
        public ReminderGroup mReminderGroup; //stores all the current reminders to be loaded
        public event EventHandler<int> itemClick; //eventHandler for checking which view is checked

        //constructor that stores the given reminderGroup to the mReminderGroup variable
        public ReminderGroupAdapter(ReminderGroup reminderGroup)
        {
            mReminderGroup = reminderGroup;
        }

        // creates an instance of a reminderGroupHolder so it is able to apply all of the elements in the view to the
        // given parameters of the each Reminder element
        public override RecyclerView.ViewHolder
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemholder = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.row, parent, false);
            ReminderGroupHolder vh = new ReminderGroupHolder(itemholder, OnClick);
            return vh; //return the new view to be displayed 
        }

        //this function puts the current reminder onto the current view holder; as the views are re-used, this is called so
        //that the views display the next (or previous) reminder to create the illusion of a continuous list of items
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ReminderGroupHolder vh = holder as ReminderGroupHolder;
            vh.Title.Text = mReminderGroup[position].name;
            vh.Description.Text = mReminderGroup[position].description;
            vh.startTime.Text = mReminderGroup[position].date;
            vh.endTime.Text = mReminderGroup[position].time;
        }

        //this function returns the amount of reminders within mReminderGroup
        public override int ItemCount
        {
            get { return mReminderGroup.numReminders; }
        }

        //if one of the views is clicked (and the click is not null), it will call the itemClick function to either check or uncheck
        //the checkbox on the view and in the reminderGroup itself
        void OnClick (int position)
        {
            if (itemClick != null)
                itemClick(this, position);
        }
    }

    [Activity(Label = "activityTwo", Theme = "@style/AppTheme")]

    //the main class for this activity, deals with the individual pages that lists the reminders for a specific day
    public class activityTwo : AppCompatActivity
    {
        RecyclerView mRecyclerView; //the recyclerView object that is displayed in the .xml file
        RecyclerView.LayoutManager mLayoutManager; //the layoutManager that is used by the recyclerView
        ReminderGroupAdapter mAdapter; //the adapter that is used by the recyclerView; this class is defined above
        public ReminderGroup mReminderGroup; //the reminderGroup that is created to store the individual reminders to be displayed
        
        //function that begins when the activity starts
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the activityTwo resource
            SetContentView(Resource.Layout.activityTwo);

            //gather the stored date information given by the mainActivity activity
            string month = Intent.GetStringExtra("month");
            string day = Intent.GetStringExtra("day");
            string year = Intent.GetStringExtra("year");

            //convert the number form into a word form
            string monthText = MainActivity.findMonthText(month);

            //combine the strings and use it to display atop the screen
            string fullDate = monthText + " " + day + ", " + year;

            var clickedDate = FindViewById<TextView>(Resource.Id.viewDate);
            clickedDate.Text = fullDate;

            //find the recyclerView to be displayed
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mReminderGroup = new ReminderGroup(month, day, year); //create the reminderGroup, the date is used to decide which reminders
                                                                  //will show up
                
            //set the layoutManager to a linearLayout
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            //create the adapter, and subscrbe the click function to the necessary functions in mReminderGroup and
            //then set the adapter of the recyclerView to this item
            mAdapter = new ReminderGroupAdapter(mReminderGroup);
            mAdapter.itemClick += mReminderGroup.HandleCustomEvent;
            mRecyclerView.SetAdapter(mAdapter);


            //if the backButton is clicked, go back to mainActivity
            var backButton = FindViewById<Button>(Resource.Id.backButton);
            backButton.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(InitiateCalendar));
                StartActivity(nextActivity);
            };

            //if the delete button is clicked, run an asynchronous function to delete the checked items
            var deleteButton = FindViewById<Button>(Resource.Id.deleteButton);
            deleteButton.Click += (s, e) =>
            {
                AsyncContext.Run(mReminderGroup.deleteCheckmarked);

                //reload the activity to show the database with the new changes
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", month);
                nextActivity.PutExtra("day", day);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);

            };

        }

        //deal with the permissions for the activity
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}