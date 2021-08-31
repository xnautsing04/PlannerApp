using SQLite;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using PlannerApp.Models;

namespace PlannerApp.Data
{
    public class ReminderDatabase
    {
        static SQLiteAsyncConnection Database; //a connection to the database for access to the data stored there

        //this creates a table for the database, storing objects of type reminder
        public static readonly AsyncLazy<ReminderDatabase> Instance = new AsyncLazy<ReminderDatabase>(async () =>
        {
            var instance = new ReminderDatabase();
            CreateTableResult result = await Database.CreateTableAsync<Reminder>();
            return instance;
        });

        //constructor, uses the constants defined in Constants.cs to create the connection
        public ReminderDatabase()
        {
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        //retreives all the items asynchronously, by converting the items in the table to a list and returning that list
        public Task<List<Reminder>> GetItemsAsync()
        {
            return Database.Table<Reminder>().ToListAsync();
        }

        //this retreieves one specified item based on the name of the item, retrieving the first one that is applicable
        public Task<Reminder> GetItemAsync(string name)
        {
            return Database.Table<Reminder>().Where(i => i.name == name).FirstOrDefaultAsync();
        }

        //save a new item to the database, can possibly be altered to save edits of a previous entry to the table
        public Task<int> SaveItemAsync(Reminder item)
        {
            //possbily add a if(id != 0) equiv
            return Database.InsertAsync(item);
        }
        
        //given an item, remove it from the database table
        public Task<int> DeleteItemAsync(Reminder item)
        {
            return Database.DeleteAsync(item);
        }
    }

    //as defined by Microsoft.Docs:
    // "The AsyncLazy class combines the Lazy<T> and Task<T> types to create a lazy-initialized task
    // that represents the initialization of a resource."
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/databases
    public class AsyncLazy<T>
    {
        readonly Lazy<Task<T>> instance;

        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();
        }
    }
}
