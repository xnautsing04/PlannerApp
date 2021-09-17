using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nito.AsyncEx;
using PlannerApp.Models;
using PlannerApp.Data;
using Android.Support.V4.App;
using Android.Media;

//This is the script that plays when the app starts. This loads the calendar asset for the current month, as found by DateTime.Now(). 
//It also lists the three (or less if three is not applicable) upcoming reminders under the calendar, as well as arrows to change the month
//currently being displayed and a plus button to add new reminders.

//Clicking one of the valid calendar entries (one with an associated date) will link to a list of all reminders for that day.

namespace PlannerApp
{
    [Activity(Label = "Main", MainLauncher = false, Theme = "@style/AppTheme")] 

    public class MainActivity : Activity
    {

        //This is a list of all reminders to be sorted to be used for the upcoming three reminders
        public static List<Reminder> reminderList = new List<Reminder>();
        
        //This is the sorted array, storing the three closest upcoming reminders.
        Reminder[] upcomingArray;

        //This is the CHANNEL_ID for the notification channel, for sending the notifications when a reminder goes off.
        public const string CHANNEL_ID = "pLannerAppNotifs9102021";

        
        //This function creates a notification channel to be used for sending out notifications at the time specified for a reminder.
        //It retrieves the channel name and description stored in Resource, and uses then to create a channel. Then, it creates a
        //notification manager, applying the channel to it.
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }
            var channelName = Resources.GetString(Resource.String.channel_name);
            var channelDescription = Resources.GetString(Resource.String.channel_description);

            var channel = new NotificationChannel(CHANNEL_ID, channelName, NotificationImportance.Default);
            channel.Description = channelDescription;

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        //This will check every minute (if there is an element in the array) for upcomingReminders, and ensure that a notification will
        //be sent if the time and day matches the current time.
        void checkRemindersForNotif()
        {
            for (int i = 0; i < upcomingArray.Length; ++i)
            {
                string[] upcomingDate = upcomingArray[i].date.Split('/');
                string upTime = upcomingDate[0] + "/" + upcomingDate[1] + "/" + upcomingDate[2] + " " + upcomingArray[i].time;
                DateTime upDTime = DateTime.Parse(upTime,
                                         System.Globalization.CultureInfo.InvariantCulture);

                if (upDTime.Date == DateTime.Now.Date && upDTime.Hour == DateTime.Now.Hour && upDTime.Minute == DateTime.Now.Minute)
                {
                    //This is creating a notification once a match has been found between the element and the current time.
                    //Clicking on the notification sends the user to an instance of activtyTwo.
                    Intent notifIntent = new Intent(this, typeof(activityTwo));

                    string month = DateTime.Now.ToString("MM");
                    string day = DateTime.Now.ToString("dd");
                    string year = DateTime.Now.ToString("yyyy");
                    string monthText = findMonthText(month);

                    notifIntent.PutExtra("month", month);
                    notifIntent.PutExtra("day", day);
                    notifIntent.PutExtra("year", year);

                    //This ensures that when the user hits the back button, it sends them back to the home screen, and not an instance
                    //of MainActivity.
                    Android.App.TaskStackBuilder stackBuilder = Android.App.TaskStackBuilder.Create(this);
                    stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(activityTwo)));
                    stackBuilder.AddNextIntent(notifIntent);


                    //Now, the NotifCompat.Builder function is used to create and send the notification to the user's device.
                    const int pendingInt = 0;
                    PendingIntent pendingIntent = stackBuilder.GetPendingIntent(pendingInt, PendingIntentFlags.OneShot);

                    NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                        .SetContentIntent(pendingIntent)
                        .SetContentTitle(upcomingArray[i].name)
                        .SetContentText(upcomingArray[i].description)
                        .SetDefaults(0)
                        .SetSmallIcon(Resource.Drawable.abc_ic_arrow_drop_right_black_24dp);

                    Notification notification = builder.Build();

                    Android.Net.Uri uri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
                    builder.SetSound(uri);

                    NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);

                    int notificationID = DateTime.Now.Millisecond;
                    notificationManager.Notify(notificationID, notification);
                }
            }
        }

        //This function retrieves the elements of the SQLite database asynchronously, which will then be used to sort and display.
        async Task findUpcomingReminders()
        {
            ReminderDatabase database = await ReminderDatabase.Instance;
            reminderList = await database.GetItemsAsync();
        }

        //This function will use a mergeSort algorithm (found in the ReminderGroup class) on the elements in the reminderList, so that
        //the most 3 (or less, is there are not 3 reminders) closest elements to the current time.
        void sortUpcoming()
        {
            upcomingArray = new Reminder[reminderList.Count];

            for (int i = 0; i < reminderList.Count; ++i)
            {
                upcomingArray[i] = reminderList[i];
            }

            ReminderGroup.sortReminders(upcomingArray, 0, upcomingArray.Length);


            if (upcomingArray.Length > 3)
            {
                Reminder[] newArray = new Reminder[3];

                for (int j = 0; j < 3; ++j)
                {
                    newArray[j] = upcomingArray[j];
                }
                upcomingArray = newArray;
            }

            populateReminders();
        }

        //This function will be called from the sortUpcoming function, in which it will display the 3 closest reminders under the 
        //"Upcoming" text. If less than 3 reminders exist, it will only display however many are present.
        void populateReminders()
        {
            var upcomingOne = FindViewById<TextView>(Resource.Id.upcoming1);
            var upcomingTwo = FindViewById<TextView>(Resource.Id.upcoming2);
            var upcomingThree = FindViewById<TextView>(Resource.Id.upcoming3);
            switch(upcomingArray.Length)
            { case 3:
                    upcomingThree.Text = upcomingArray[2].name + ": " + upcomingArray[2].date + " @ " + upcomingArray[2].time;
                    goto case 2;
              case 2:
                    upcomingTwo.Text = upcomingArray[1].name + ": " + upcomingArray[1].date + " @ " + upcomingArray[1].time;
                    goto case 1;
              case 1:
                    upcomingOne.Text = upcomingArray[0].name + ": " + upcomingArray[0].date + " @ " + upcomingArray[0].time;
                    break;
            }

                    
        }

        //This function listens for each of the buttons that are represented by the dates on the calendar. Each button will listen
        //to call a different instance of activityTwo, showing the reminders that are listed for that day.
        public void ButtonListen(string monthText, string year)
        {
            
            Button button0 = FindViewById<Button>(Resource.Id.date0); //this represents the first button, and gathers the ID for it
            if (button0.Text != "") //this is one of the possible buttons that may be empty on a standard calendar,
                                    //so it checks if it is empty or not
            {
                button0.Click += (s, e) => //on the click
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button0.Text); //load the next activity and send the necessary information
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }

            // repeat for the remainder of the buttons
            Button button1 = FindViewById<Button>(Resource.Id.date1);
            if(button1.Text != " ")
            {
                button1.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button1.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            
            Button button2 = FindViewById<Button>(Resource.Id.date2);
            if(button2.Text != "")
            {
                button2.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button2.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }

            Button button3 = FindViewById<Button>(Resource.Id.date3);
            if (button3.Text != "")
            {
                button3.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button3.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button4 = FindViewById<Button>(Resource.Id.date4);
            if(button4.Text != "")
            {
                button4.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button4.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button5 = FindViewById<Button>(Resource.Id.date5);
            if (button5.Text != "")
            {
                button5.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button5.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }

            Button button6 = FindViewById<Button>(Resource.Id.date6); //at this point, this spot in the calendar is guarenteed to be filled
                                                                      //so the if statement is gone
            button6.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button6.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button7 = FindViewById<Button>(Resource.Id.date7);
            button7.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button7.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button8 = FindViewById<Button>(Resource.Id.date8);
            button8.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button8.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button9 = FindViewById<Button>(Resource.Id.date9);
            button9.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button9.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button10 = FindViewById<Button>(Resource.Id.date10);
            button10.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button10.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button11 = FindViewById<Button>(Resource.Id.date11);
            button11.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button11.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button12 = FindViewById<Button>(Resource.Id.date12);
            button12.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button12.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button13 = FindViewById<Button>(Resource.Id.date13);
            button13.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button13.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button14 = FindViewById<Button>(Resource.Id.date14);
            button14.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button14.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button15 = FindViewById<Button>(Resource.Id.date15);
            button15.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button15.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button16 = FindViewById<Button>(Resource.Id.date16);
            button16.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button16.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button17 = FindViewById<Button>(Resource.Id.date17);
            button17.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button17.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button18 = FindViewById<Button>(Resource.Id.date18);
            button18.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button18.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button19 = FindViewById<Button>(Resource.Id.date19);
            button19.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button19.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button20 = FindViewById<Button>(Resource.Id.date20);
            button20.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button20.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button21 = FindViewById<Button>(Resource.Id.date21);
            button21.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button21.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button22 = FindViewById<Button>(Resource.Id.date22);
            button22.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button22.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button23 = FindViewById<Button>(Resource.Id.date23);
            button23.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button23.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button24 = FindViewById<Button>(Resource.Id.date24);
            button24.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button24.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button25 = FindViewById<Button>(Resource.Id.date25);
            button25.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button25.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button26 = FindViewById<Button>(Resource.Id.date26);
            button26.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button26.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button27 = FindViewById<Button>(Resource.Id.date27);
            button27.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button27.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button28 = FindViewById<Button>(Resource.Id.date28);
            button28.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button28.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button29 = FindViewById<Button>(Resource.Id.date29);
            button29.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button29.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button30 = FindViewById<Button>(Resource.Id.date30);
            button30.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(activityTwo));
                nextActivity.PutExtra("month", monthText);
                nextActivity.PutExtra("day", button30.Text);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);
            };

            Button button31 = FindViewById<Button>(Resource.Id.date31);
            if (button31.Text != "") //the if statement returns for the same reason as before
            {
                button31.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button31.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }

            Button button32 = FindViewById<Button>(Resource.Id.date32);
            if (button32.Text != "")
            {
                button32.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button32.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button33 = FindViewById<Button>(Resource.Id.date33);
            if (button33.Text != "")
            {
                button33.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button33.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button34 = FindViewById<Button>(Resource.Id.date34);
            if (button34.Text != "")
            {
                button34.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button34.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button35 = FindViewById<Button>(Resource.Id.date35);
            if (button35.Text != "")
            {
                button35.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button35.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button36 = FindViewById<Button>(Resource.Id.date36);
            if (button36.Text != "")
            {
                button36.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button36.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }

            Button button37 = FindViewById<Button>(Resource.Id.date37);
            if (button37.Text != "")
            {
                button37.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button37.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button38 = FindViewById<Button>(Resource.Id.date38);
            if (button38.Text != "")
            {
                button38.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button38.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
          

            Button button39 = FindViewById<Button>(Resource.Id.date39);
            if (button39.Text != "")
            {
                button39.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button39.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button40 = FindViewById<Button>(Resource.Id.date40);
            if (button40.Text != "")
            {
                button40.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button1.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

            Button button41 = FindViewById<Button>(Resource.Id.date41);
            if (button41.Text != "")
            {
                button1.Click += (s, e) =>
                {
                    Intent nextActivity = new Intent(this, typeof(activityTwo));
                    nextActivity.PutExtra("month", monthText);
                    nextActivity.PutExtra("day", button1.Text);
                    nextActivity.PutExtra("year", year);
                    StartActivity(nextActivity);
                };
            }
            

        }

        //This function is meant to populate all the buttons in the calendar based on the current month.
        //Date represents the date to be populated.
        //Num represents the number of the button to be filled (so if January 1st was on a Tuesday, date would be 1 and num would be 2)
        public void PopulateElement(int num, int date)
        {
            switch(num)
            {
                case 0:
                    var button0 = FindViewById<Button>(Resource.Id.date0);
                    button0.Text = date.ToString();
                    break;
                case 1:
                    var button1 = FindViewById<Button>(Resource.Id.date1);
                    button1.Text = date.ToString();
                    break;
                case 2:
                    var button2 = FindViewById<Button>(Resource.Id.date2);
                    button2.Text = date.ToString();
                    break;
                case 3:
                    var button3 = FindViewById<Button>(Resource.Id.date3);
                    button3.Text = date.ToString();
                    break;
                case 4:
                    var button4 = FindViewById<Button>(Resource.Id.date4);
                    button4.Text = date.ToString();
                    break;
                case 5:
                    var button5 = FindViewById<Button>(Resource.Id.date5);
                    button5.Text = date.ToString();
                    break;
                case 6:
                    var button6 = FindViewById<Button>(Resource.Id.date6);
                    button6.Text = date.ToString();
                    break;
                case 7:
                    var button7 = FindViewById<Button>(Resource.Id.date7);
                    button7.Text = date.ToString();
                    break;
                case 8:
                    var button8 = FindViewById<Button>(Resource.Id.date8);
                    button8.Text = date.ToString();
                    break;
                case 9:
                    var button9 = FindViewById<Button>(Resource.Id.date9);
                    button9.Text = date.ToString();
                    break;
                case 10:
                    var button10 = FindViewById<Button>(Resource.Id.date10);
                    button10.Text = date.ToString();
                    break;
                case 11:
                    var button11 = FindViewById<Button>(Resource.Id.date11);
                    button11.Text = date.ToString();
                    break;
                case 12:
                    var button12 = FindViewById<Button>(Resource.Id.date12);
                    button12.Text = date.ToString();
                    break;
                case 13:
                    var button13 = FindViewById<Button>(Resource.Id.date13);
                    button13.Text = date.ToString();
                    break;
                case 14:
                    var button14 = FindViewById<Button>(Resource.Id.date14);
                    button14.Text = date.ToString();
                    break;
                case 15:
                    var button15 = FindViewById<Button>(Resource.Id.date15);
                    button15.Text = date.ToString();
                    break;
                case 16:
                    var button16 = FindViewById<Button>(Resource.Id.date16);
                    button16.Text = date.ToString();
                    break;
                case 17:
                    var button17 = FindViewById<Button>(Resource.Id.date17);
                    button17.Text = date.ToString();
                    break;
                case 18:
                    var button18 = FindViewById<Button>(Resource.Id.date18);
                    button18.Text = date.ToString();
                    break;
                case 19:
                    var button19 = FindViewById<Button>(Resource.Id.date19);
                    button19.Text = date.ToString();
                    break;
                case 20:
                    var button20 = FindViewById<Button>(Resource.Id.date20);
                    button20.Text = date.ToString();
                    break;
                case 21:
                    var button21 = FindViewById<Button>(Resource.Id.date21);
                    button21.Text = date.ToString();
                    break;
                case 22:
                    var button22 = FindViewById<Button>(Resource.Id.date22);
                    button22.Text = date.ToString();
                    break;
                case 23:
                    var button23 = FindViewById<Button>(Resource.Id.date23);
                    button23.Text = date.ToString();
                    break;
                case 24:
                    var button24 = FindViewById<Button>(Resource.Id.date24);
                    button24.Text = date.ToString();
                    break;
                case 25:
                    var button25 = FindViewById<Button>(Resource.Id.date25);
                    button25.Text = date.ToString();
                    break;
                case 26:
                    var button26 = FindViewById<Button>(Resource.Id.date26);
                    button26.Text = date.ToString();
                    break;
                case 27:
                    var button27 = FindViewById<Button>(Resource.Id.date27);
                    button27.Text = date.ToString();
                    break;
                case 28:
                    var button28 = FindViewById<Button>(Resource.Id.date28);
                    button28.Text = date.ToString();
                    break;
                case 29:
                    var button29 = FindViewById<Button>(Resource.Id.date29);
                    button29.Text = date.ToString();
                    break;
                case 30:
                    var button30 = FindViewById<Button>(Resource.Id.date30);
                    button30.Text = date.ToString();
                    break;
                case 31:
                    var button31 = FindViewById<Button>(Resource.Id.date31);
                    button31.Text = date.ToString();
                    break;
                case 32:
                    var button32 = FindViewById<Button>(Resource.Id.date32);
                    button32.Text = date.ToString();
                    break;
                case 33:
                    var button33 = FindViewById<Button>(Resource.Id.date33);
                    button33.Text = date.ToString();
                    break;
                case 34:
                    var button34 = FindViewById<Button>(Resource.Id.date34);
                    button34.Text = date.ToString();
                    break;
                case 35:
                    var button35 = FindViewById<Button>(Resource.Id.date35);
                    button35.Text = date.ToString();
                    break;
                case 36:
                    var button36 = FindViewById<Button>(Resource.Id.date36);
                    button36.Text = date.ToString();
                    break;
                case 37:
                    var button37 = FindViewById<Button>(Resource.Id.date37);
                    button37.Text = date.ToString();
                    break;
                case 38:
                    var button38 = FindViewById<Button>(Resource.Id.date38);
                    button38.Text = date.ToString();
                    break;
                case 39:
                    var button39 = FindViewById<Button>(Resource.Id.date39);
                    button39.Text = date.ToString();
                    break;
                case 40:
                    var button40 = FindViewById<Button>(Resource.Id.date40);
                    button40.Text = date.ToString();
                    break;
                case 41:
                    var button41 = FindViewById<Button>(Resource.Id.date41);
                    button41.Text = date.ToString();
                    break;
            }
        }

        //This function is meant to find out what day of the week the first of the month falls on.
        //month and year are strings containing the current information, 08 and 2021 for example.
        public void findInitDay(string month, string year)
        {
            int mon = Int32.Parse(month);
            int y = Int32.Parse(year);
            DateTime dayOne = new DateTime(y, mon, 01);
            DayOfWeek initialDay = dayOne.DayOfWeek;
            int weekDayNum = -1;
            
            switch(initialDay) //this statement takes the classification of the initialDay variable and converts into a number to be used by
                               //the PopulateElement function 
            {
                case DayOfWeek.Sunday:
                    weekDayNum = 0;
                    break;
                case DayOfWeek.Monday:
                    weekDayNum = 1;
                    break;
                case DayOfWeek.Tuesday:
                    weekDayNum = 2;
                    break;
                case DayOfWeek.Wednesday:
                    weekDayNum = 3;
                    break;
                case DayOfWeek.Thursday:
                    weekDayNum = 4;
                    break;
                case DayOfWeek.Friday:
                    weekDayNum = 5;
                    break;
                case DayOfWeek.Saturday:
                    weekDayNum = 6;
                    break;
            }

            int date = 1;
            for (int i = weekDayNum; i < DateTime.DaysInMonth(y, mon) + weekDayNum; ++i)
            {
                PopulateElement(i, date++); //populates the month by using the previously found first button
            }

        }

        //This is a simple function to convert the int of a month to a string of the word equivalent.
        public static String findMonthText(String month)
        {
            switch(month)
            {
                case "01":
                    return "January";
                case "02":
                    return "February";
                case "03":
                    return "March";
                case "04":
                    return "April";
                case "05":
                    return "May";
                case "06":
                    return "June";
                case "07":
                    return "July";
                case "08":
                    return "August";
                case "09":
                    return "September";
                case "10":
                    return "October";
                case "11":
                    return "November";
                case "12":
                    return "December";
            };
            return "ERROR";
        }

        //This is the first screen to display when the app starts, creating the main screen,
        //showing the calendar and other important information. This is called from InitiateCalendar.
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            AsyncContext.Run(findUpcomingReminders);

            sortUpcoming();

            CreateNotificationChannel();

            //Create a timer so that the reminders will be checked every minute so that notifications will send out.

            var startTime = TimeSpan.Zero;
            var periodSpan = TimeSpan.FromMinutes(1);

            var timer = new System.Threading.Timer((e) =>
            {
                checkRemindersForNotif();
            }, null, startTime, periodSpan);

            //The Textview above the main calendar that displays the month and year.
            var dateOnScreen = FindViewById<TextView>(Resource.Id.monthCurrent);

            //This finds the month/date/year based on the DateTime.Now function, which has been sent from InitiateCalendar.
            string month = Intent.GetStringExtra("month");
            string date = Intent.GetStringExtra("date");
            string year = Intent.GetStringExtra("year");

            //This displays the date on the screen with the month (switched to a String).
            string monthText = findMonthText(month);
            string monthYear = monthText.Substring(0, 3) + " " + year;
            dateOnScreen.Text = monthYear;

            //This will populate the calendar.
            findInitDay(month, year);


            //This listens for all the calendar buttons to switch the layout.
            ButtonListen(month, year);

            //This listens for the add button to switch to the add screen.
            var ButtonAdd = FindViewById<Button>(Resource.Id.addButton);
            ButtonAdd.Click += (s, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(newReminders));
                StartActivity(nextActivity);
            };

            //This button listens for the upCalendar button to move the calendar up one month.
            var upCalendar = FindViewById<Button>(Resource.Id.UpCalendar);
            upCalendar.Click += (s, e) =>
            {
                int monthNum = int.Parse(month);
                int yearNum = int.Parse(year);
                monthNum++;
                if (monthNum == 13)
                {
                    monthNum = 1;
                    yearNum++;
                }
                
                if (monthNum < 10)
                    month = "0" + monthNum.ToString();
                else
                    month = monthNum.ToString();

                year = yearNum.ToString();

                Intent nextActivity = new Intent(this, typeof(MainActivity));
                nextActivity.PutExtra("month", month);
                nextActivity.PutExtra("date", date);
                nextActivity.PutExtra("year", year);
                StartActivity(nextActivity);

            };

            //This button listens for the downCalendar to move the calendar down one month.
            var downCalendar = FindViewById<Button>(Resource.Id.DownCalendar);
            downCalendar.Click += (s, e) =>
            {
                int monthNum = int.Parse(month);
                int yearNum = int.Parse(year);
                monthNum--;
                if (monthNum == 0)
                {
                    monthNum = 12;
                    yearNum--;
                }

                if (monthNum < 10)
                    month = "0" + monthNum.ToString();
                else
                    month = monthNum.ToString();

                year = yearNum.ToString();

                Intent nextActivity = new Intent(this, typeof(MainActivity));
                nextActivity.PutExtra("month", month);
                nextActivity.PutExtra("date", date);
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