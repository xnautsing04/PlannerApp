using System.Threading.Tasks;
using System.Collections.Generic;
using Nito.AsyncEx;
using PlannerApp.Models;
using PlannerApp.Data;

namespace PlannerApp
{
    //this class stores the elements to be used on a given instance of the defined recyclerView in activityTwo
    public class ReminderGroup
    {
        //the list that will store all current reminders from the database
        public static List<Reminder> exampleReminders;
       
        //an array that will store on the reminders that fall on the given date
        private Reminder[] mReminders;

        

        //this function gathers all of the reminders from the database and stores them in exampleReminders to be filtered later
        async Task initializeExample()
        {
            ReminderDatabase database = await ReminderDatabase.Instance;
            exampleReminders = await database.GetItemsAsync();
        }

        //constructor for the class; will call initializeExample and filter all reminders to find the ones that fall on the given
        //date, which is sent as three strings storing the month, the date, and the year
        public ReminderGroup(string month, string date, string year)
        {
            AsyncContext.Run(initializeExample); //asynchronously run the initializeExample function to fill the exampleReminders fucntion

            //create an array of newReminders, and make it as big as exampleRemiders as a precaution. This will store the reminders
            //that fall on the given date selected
            Reminder[] newReminders = new Reminder[exampleReminders.Count];

            //store the amount of reminders that get added to newReminders, as it will be used as a new size in a later array initialization
            int count = 0;

            //iterate through all the elements
            for (int i = 0; i < exampleReminders.Count; ++i)
            {
                //split the date element in the reminder into month, date, and year
                string[] currDate = exampleReminders[i].date.Split('/');

                //if the elements all match, add it to newReminders
                if (currDate[0] == month && currDate[1] == date && currDate[2] == year)
                {
                    newReminders[count++] = exampleReminders[i];
                }
            }
            mReminders = new Reminder[count]; //initialize mReminders with count size, as to make sure it has the correct size
            for (int j = 0; j < count; ++j)
            {
                mReminders[j] = newReminders[j];
            }

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
    }

}
