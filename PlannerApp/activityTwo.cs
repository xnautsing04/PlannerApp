using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;


namespace PlannerApp
{
    [Activity(Label = "activityTwo")]

    public class Reminder
    {
        public Reminder(string newName, string newDescription, string newStart, string newEnd, int newImage)
        {
            name = newName;
            description = newDescription;
            timeStart = newStart;
            timeEnd = newEnd;
            image = newImage;
        }

        public string name { get; }
        public string description { get; }
        public string timeStart { get; }
        public string timeEnd { get; }
        public int image { get; }
    }

    public class ReminderGroup
    {
        static Reminder[] exampleReminders =
        {
            new Reminder("Cardio Exercise", "Complete 20 minutes of Cardio", "12:00PM", "1:00PM", Resource.Drawable.Exercise),
            new Reminder("Posture Strecthes", "Complete 10 minutes of Posture-focused stretches", "1:00PM", "1:30PM", Resource.Drawable.health),
            new Reminder("LeetCode Problem", "Complete 1 hard LeetCode Problem", "1:30PM", "3:00PM", Resource.Drawable.Computer),
            new Reminder("Dentist Apointment", "Arrive on time for my apointment for 4:00", "3:00PM", "5:00PM", Resource.Drawable.apointment),
            new Reminder("Posture Strength Exercises", "Complete 10 minutes of Posture-focused strength training", "5:00PM", "5:30PM", Resource.Drawable.health),
            new Reminder("Play Breath of the Wild", "Complete 30 minutes of progress in Breath of the Wild", "5:30PM", "6:00PM", Resource.Drawable.Computer),
            new Reminder("Make Dinner", "Cook and eat a healthy dinner", "6:00PM", "6:30PM", Resource.Drawable.health),
            new Reminder("Play Minecraft", "Play with friends for 30 minutes", "6:30PM", "7:00PM", Resource.Drawable.Computer),
            new Reminder("Strength Training Workout", "Complete 20 minutes of Strength Training", "7:00PM", "7:30PM", Resource.Drawable.Exercise),
            new Reminder("App Development", "Complete 30 minutes of PlannerApp Development", "7:30PM", "8:00PM", Resource.Drawable.Computer),
            new Reminder("Watch Jojo", "Watch 4 episodes of Jojo", "8:00PM", "10:00PM", Resource.Drawable.Computer),
            new Reminder("Before Bed Routine", "Complete all lotion, teeth, and face care routines", "10:00PM", "11:00PM", Resource.Drawable.health)
        };

        private Reminder[] mReminders;

        public ReminderGroup()
        {
            mReminders = exampleReminders;
        }

        public int numReminders
        {
            get { return mReminders.Length; }
        }

        public Reminder this[int i]
        {
            get { return mReminders[i]; }
        }
    }

    public class ReminderGroupHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Description { get; private set; }
        public TextView startTime { get; private set; }
        public TextView endTime { get; private set; }
        public ReminderGroupHolder(View itemView) : base(itemView)
        {
            Image = itemView.FindViewById<ImageView>(Resource.Id.imageView);
            Title = itemView.FindViewById<TextView>(Resource.Id.textViewTitle);
            Description = itemView.FindViewById<TextView>(Resource.Id.textViewDescription);
            startTime = itemView.FindViewById<TextView>(Resource.Id.textViewStartTime);
            endTime = itemView.FindViewById<TextView>(Resource.Id.textViewEndTime);
        }
    }

    public class ReminderGroupAdapter : RecyclerView.Adapter
    {
        public ReminderGroup mReminderGroup;

        public ReminderGroupAdapter(ReminderGroup reminderGroup)
        {
            mReminderGroup = reminderGroup;
        }

        public override RecyclerView.ViewHolder
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemholder = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.row, parent, false);
            ReminderGroupHolder vh = new ReminderGroupHolder(itemholder);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ReminderGroupHolder vh = holder as ReminderGroupHolder;
            vh.Image.SetImageResource(mReminderGroup[position].image);
            vh.Title.Text = mReminderGroup[position].name;
            vh.Description.Text = mReminderGroup[position].description;
            vh.startTime.Text = mReminderGroup[position].timeStart;
            vh.endTime.Text = mReminderGroup[position].timeEnd;
        }

        public override int ItemCount
        {
            get { return mReminderGroup.numReminders; }
        }
    }

    public class activityTwo : AppCompatActivity
    {

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        ReminderGroupAdapter mAdapter;
        ReminderGroup mReminderGroup;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activityTwo);

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mReminderGroup = new ReminderGroup();
            

            

            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            mAdapter = new ReminderGroupAdapter(mReminderGroup);
            mRecyclerView.SetAdapter(mAdapter);


        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}