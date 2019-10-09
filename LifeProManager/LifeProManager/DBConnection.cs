using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace LifeProManager
{
    class DBConnection
    {
        SQLiteConnection SQLiteConn;

        public void DBConn()
        {
            SQLiteConn = new SQLiteConnection("Data source=LifeProManager.db; version=3; Compress=true;");
            SQLiteConn.Open();
        }

        public void CreateTables()
        {
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "CREATE TABLE Priorities (id INTEGER PRIMARY KEY AUTOINCREMENT, Denomination VARCHAR(15), PriorityLvl INTEGER)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE TABLE Lists (id INTEGER PRIMARY KEY AUTOINCREMENT, Title VARCHAR(45))";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE TABLE Status (id INTEGER PRIMARY KEY AUTOINCREMENT, Denomination VARCHAR(45))";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE TABLE Tasks (id INTEGER PRIMARY KEY AUTOINCREMENT, Title VARCHAR(45), Description VARCHAR(500), Deadline DATE, Priorities_id INTEGER, Lists_id INTEGER, Status_id INTEGER, FOREIGN KEY ('Priorities_id') REFERENCES Priorities('Priorities_id'), FOREIGN KEY ('Lists_id') REFERENCES Lists('Lists_id'), FOREIGN KEY ('Status_id') REFERENCES Status('Status_id'))";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataPriorities(string Denomination, int PriorityLvl)
        {
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO Priorities (Denomination, PriorityLvl) VALUES ('"+Denomination+"','"+PriorityLvl+"');";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataLists(string Title)
        {
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO Lists (Title) VALUES ('"+Title+"');";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataStatus(string Denomination)
        {
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO Status (Denomination) VALUES('"+Denomination+"');";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataStatus(string Title, string Description, DateTime Deadline)
        {
            SQLiteCommand cmd = SQLiteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO TASKS(Title, Description, Deadline) VALUES ('"+Title+"','"+Description+"','"+Deadline+"');";
            cmd.ExecuteNonQuery();
        }


    }
}
