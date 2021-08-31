using System;
using System.IO;


//this class stores the constants and flags to be used by the ReminderDatabase at all times
public static class Constants
{
    //this will store the name of the database to be used within ReminderDatabase
    public const string DatabaseFilename = "savedReminders.db3";

    public const SQLite.SQLiteOpenFlags Flags =

        //apply read/write mode for the database
        SQLite.SQLiteOpenFlags.ReadWrite |
        //create the database if it is currently not in existence
        SQLite.SQLiteOpenFlags.Create |
        //enable multi-threaded access
        SQLite.SQLiteOpenFlags.SharedCache;

        //this represents the path to where the data is stored
        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
}