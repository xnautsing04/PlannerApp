using System.Threading.Tasks;
using System.Collections.Generic;
using Nito.AsyncEx;
using PlannerApp.Models;
using PlannerApp.Data;
using System;

namespace PlannerApp
{
    //this class stores the elements to be used on a given instance of the defined recyclerView in activityTwo
    public class ReminderGroup
    {
        //the list that will store all current reminders from the database
        public static List<Reminder> exampleReminders;
       
        //an array that will store on the reminders that fall on the given date
        private Reminder[] mReminders;

        private string month;
        private string date;
        private string year;
        

        //this function gathers all of the reminders from the database and stores them in exampleReminders to be filtered later
        async Task initializeExample()
        {
            ReminderDatabase database = await ReminderDatabase.Instance;
            exampleReminders = await database.GetItemsAsync();
        }

        async Task completeReminderInit()
        {
            //create an array of newReminders, and make it as big as exampleRemiders as a precaution. This will store the reminders
            //that fall on the given date selected
            Reminder[] newReminders = new Reminder[exampleReminders.Count];

            //store the amount of reminders that get added to newReminders, as it will be used as a new size in a later array initialization
            int count = 0;

            //iterate through all the elements
            for (int i = 0; i < exampleReminders.Count; ++i)
            {
                //split the date element in the reminder into month, date, and year
                string total = month + "/" + date + "/" + year +  " " + exampleReminders[i].time;
                string[] currDate = exampleReminders[i].date.Split('/');
                string time = currDate[0] + "/" + currDate[1] + "/" + currDate[2] + " " + exampleReminders[i].time;
                DateTime currDT = DateTime.Parse(time,
                                          System.Globalization.CultureInfo.InvariantCulture);

                DateTime selectedDate = DateTime.Parse(total,
                                          System.Globalization.CultureInfo.InvariantCulture);



                //if the elements all match, add it to newReminders
                if (currDT ==selectedDate && currDT >= DateTime.Now)
                {
                    newReminders[count++] = exampleReminders[i];
                }
                else if (currDT < DateTime.Now)
                {
                    await deleteOutdated(exampleReminders[i]);
                }
            }
            mReminders = new Reminder[count]; //initialize mReminders with count size, as to make sure it has the correct size
            for (int j = 0; j < count; ++j)
            {
                mReminders[j] = newReminders[j];
            }

            if (numReminders > 1)
                sortReminders(mReminders, 0, numReminders);
        }

        //constructor for the class; will call initializeExample and filter all reminders to find the ones that fall on the given
        //date, which is sent as three strings storing the month, the date, and the year
        public ReminderGroup(string month, string date, string year)
        {
            AsyncContext.Run(initializeExample); //asynchronously run the initializeExample function to fill the exampleReminders fucntion

            this.month = month;
            this.date = date;
            this.year = year;

            AsyncContext.Run(completeReminderInit);
        }

        //return the length of mReminders
        public int numReminders
        {
            get { return mReminders.Length; }
        }

        //return an individual element in mReminders
        public Reminder this[int i]
        {
            get { return mReminders[i]; }
        }

        //this will be called when a checkmark on any of the current views is selected, based on what the current state is
        //it will either be changed to true or false
        public void HandleCustomEvent(object sender, int a)
        {
            if (mReminders[a].selected == false)
            {
                mReminders[a].selected = true;
            }

            else if (mReminders[a].selected == true)
            {
                mReminders[a].selected = false;
            }
        }

        //if the deleteButton is selected in activityTwo, remove all reminders that have a check from the database
        public async Task deleteCheckmarked()
        {
            ReminderDatabase database = await ReminderDatabase.Instance;

            //is the selected bool is true, then go ahead and delete that element
            for (int i = 0; i < mReminders.Length; ++i)
            {
                if (mReminders[i].selected == true)
                {
                    await database.DeleteItemAsync(mReminders[i]);
                }
            }

        }

        static void sortMerge(Reminder[] remindArr, int low, int mid, int high)
        {
            int n = high - low;
            Reminder[] b = new Reminder[n];
            int left = low;
            int right = mid;
            int bInd = 0;

            while (left < mid && right < high)
            {
                string[] currDateL = remindArr[left].date.Split('/');
                string timeL = currDateL[0] + "/" + currDateL[1] + "/" + currDateL[2] + " " + remindArr[left].time;
                DateTime elemDTLeft = DateTime.Parse(timeL,
                                         System.Globalization.CultureInfo.InvariantCulture);

                string[] currDateR = remindArr[right].date.Split('/');
                string timeR = currDateR[0] + "/" + currDateR[1] + "/" + currDateR[2] + " " + remindArr[right].time;
                DateTime elemDTRight = DateTime.Parse(timeR,
                                         System.Globalization.CultureInfo.InvariantCulture);

                if (elemDTLeft <= elemDTRight)
                    b[bInd++] = remindArr[left++];

                else
                    b[bInd++] = remindArr[right++];
            }

            while (left < mid)
            {
                b[bInd++] = remindArr[left++];
            }

            while (right < high)
            {
                b[bInd++] = remindArr[right++];
            }

            for (int i = 0; i < n; ++i)
            {
                remindArr[low + i] = b[i];
            }
        }

        public static void sortReminders(Reminder[] remindArr, int low, int high)
        {
            if (low < high)
            {
                int mid = (low + high) / 2;
                sortReminders(remindArr, low, mid);
                sortReminders(remindArr, mid + 1, high);

                sortMerge(remindArr, low, mid, high);
            }
        }

        public async Task deleteOutdated(Reminder elem)
        {
            ReminderDatabase database = await ReminderDatabase.Instance;
            await database.DeleteItemAsync(elem);
        }
    }

}
