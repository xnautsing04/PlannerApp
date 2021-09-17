using System;
using System.IO;


//This class stores the constants and flags to be used by the ReminderDatabase at all times.
public static class Constants
{
    //This will store the name of the database to be used within ReminderDatabase.
    public const string DatabaseFilename = "savedReminders.db3";

    public const SQLite.SQLiteOpenFlags Flags =

        //Apply read/write mode for the database.
        SQLite.SQLiteOpenFlags.ReadWrite |
        //Create the database if it is currently not in existence.
        SQLite.SQLiteOpenFlags.Create |
        //Enable multi-threaded access.
        SQLite.SQLiteOpenFlags.SharedCache;

        //This represents the path to where the data is stored.
        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
}