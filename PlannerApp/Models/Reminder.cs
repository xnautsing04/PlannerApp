using SQLite;

namespace PlannerApp.Models
{
    //class that stores the individual items to be used for each reminder
    public class Reminder
    {

        [PrimaryKey] //this means that name is the main identiier from retrieving items from the database 
        public string name { get; set; } //the name of the reminder
        public string description { get; set; } //a description of what is to be done
        public string date { get; set; } //the date of the reminder
        public string time { get; set; } //the time that the reminder will go off on the above day
        public bool selected { get; set; } //this stores whether the item is checked or not, which decides if it is going to be
                                           //deleted if the user decides to hit the delete button
    }

}