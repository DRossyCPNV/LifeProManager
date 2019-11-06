using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeProManager
{
    class DBConnection
    {
        // Declaration of a private attribute of type SQLiteConnection
        SQLiteConnection sqliteConn;

        public DBConnection()
        {
            // Create a new database connection :
            sqliteConn = new SQLiteConnection("Data Source=LPM_DB.db; Version=3; Compress=True;");
            // Open the connection :
            sqliteConn.Open();
        }


        public void CreateTable()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "BEGIN TRANSACTION; " +
                                //-- Create table Lists 
                                //"DROP TABLE IF EXISTS 'Lists'; " +
                                "CREATE TABLE IF NOT EXISTS 'Lists' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL);" +
                                //-- Create table Priorities 
                                "DROP TABLE IF EXISTS 'Priorities'; " +
                                "CREATE TABLE IF NOT EXISTS 'Priorities' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination' VARCHAR(25) NOT NULL);" +
                                //-- Create table Status 
                                "DROP TABLE IF EXISTS 'Status'; " +
                                "CREATE TABLE IF NOT EXISTS 'Status' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination'	VARCHAR(50) NOT NULL); " +
                                //-- Create table Tasks
                                //"DROP TABLE IF EXISTS 'Tasks'; " +
                                "CREATE TABLE IF NOT EXISTS 'Tasks' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL, 'description' " +
                                "VARCHAR(500) DEFAULT NULL, 'deadline' DATE DEFAULT NULL, 'validationDate' DATE DEFAULT NULL, 'Priorities_id' INTEGER NOT NULL, 'Lists_id' INTEGER NOT NULL, 'Status_id' " + 
                                "INTEGER NOT NULL, FOREIGN KEY ('Priorities_id') REFERENCES Priorities('id'), FOREIGN KEY ('Lists_id') REFERENCES Lists('id'), " + 
                                "FOREIGN KEY ('Status_id') REFERENCES Status('id')); " + 
                                "COMMIT; ";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public void InsertTopic(String title)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "INSERT INTO Lists VALUES(NULL, '" + title.Replace("'", "''") + "')";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public List<Lists> ReadTopics()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "SELECT id, title FROM Lists";
            List<Lists> topicList = new List<Lists>();
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                Lists currentList = new Lists();

                int id;

                if (int.TryParse(dataReader["id"].ToString(), out id))
                {
                    currentList.Id = id;
                }

                currentList.Title = dataReader["title"].ToString();

                topicList.Add(currentList);
            }
            return topicList;
        }

        public void DeleteTopic(int id)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "Delete from Tasks " +
                               "WHERE Lists_id = " + id + "; " +
                               "Delete from Lists " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public void InsertTask(string title, string description, string deadline, int priorities_id, int lists_id, int status_id)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "INSERT INTO Tasks VALUES(NULL, '" + title.Replace("'", "''") + "', '" + description.Replace("'", "''") + "', '" + deadline + "', NULL, " + priorities_id + ", " + lists_id + ", " + status_id + ")";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public void ApproveTask(int id, string validationDate)
        {
            /* 2 is id for "done" status*/
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "UPDATE Tasks " +
                               "SET validationDate = '" + validationDate + "', " +
                               "Status_id = " + 2 + " " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public void UnapproveTask(int id)
        {
                /* 1 is id for "To do" status*/
                SQLiteCommand cmd = sqliteConn.CreateCommand();
                string createSql = "UPDATE Tasks " +
                                   "SET validationDate = NULL, " +
                                   "Status_id = " + 1 + " " +
                                   "WHERE id = " + id + ";";
                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();
        }

        public void EditTask(int id, string title, string description, string deadline, int priorities_id, int lists_id)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "UPDATE Tasks " +
                               "SET title = '" + title.Replace("'", "''") + "', " +
                               "description = '" + description.Replace("'", "''") + "', " +
                               "deadline = '" + deadline + "', " +
                               "Priorities_id = " + priorities_id + ", " +
                               "Lists_id = " + lists_id + " " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public void DeleteTask(int id)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "Delete from Tasks " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public void InsertPriorities()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "BEGIN TRANSACTION; " +
                               "INSERT INTO Priorities VALUES(NULL, 'Faible');" +
                               "INSERT INTO Priorities VALUES(NULL, 'Moderée');" +
                               "INSERT INTO Priorities VALUES(NULL, 'Elevée');" +
                               "COMMIT; ";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public List<string> ReadPrioritiesDenomination()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "SELECT denomination FROM Priorities";
            List<string> prioritiesList = new List<string>();
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                string myReader = dataReader["denomination"].ToString();
                prioritiesList.Add(myReader);
            }
            return prioritiesList;
        }

        public void InsertStatus()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "BEGIN TRANSACTION; " +
                               "INSERT INTO Status VALUES(NULL, 'A faire');" +
                               "INSERT INTO Status VALUES(NULL, 'Terminée');" +
                               "COMMIT; ";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        public List<string> ReadStatusDenomination()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "SELECT denomination FROM Status";
            List<string> statusList = new List<string>();
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                string myReader = dataReader["denomination"].ToString();
                statusList.Add(myReader);
            }
            return statusList;
        }

        /// <summary>
        /// Extracts all the tasks from the database where the condition, given in argument, applies.
        /// </summary>
        /// <returns>tasksList containing the result of the request</returns>
        public List<Tasks> ReadTask(string condition)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "SELECT id, title, description, deadline, validationDate, Priorities_id, Lists_id, Status_id FROM Tasks " + condition;
            List<Tasks> tasksList = new List<Tasks>();
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                Tasks currentTask = new Tasks();

                int id;
                int priorities_id;
                int lists_id;
                int status_id;

                if (int.TryParse(dataReader["id"].ToString(), out id))
                {
                    currentTask.Id = id;
                }
                if (int.TryParse(dataReader["Priorities_id"].ToString(), out priorities_id))
                {
                    currentTask.Priorities_id = priorities_id;
                }
                if (int.TryParse(dataReader["Lists_id"].ToString(), out lists_id))
                {
                    currentTask.Lists_id = lists_id;
                }
                if (int.TryParse(dataReader["Status_id"].ToString(), out status_id))
                {
                    currentTask.Status_id = status_id;

                    //Only reads the validation value if the task status' is "done" (2) because only approved tasks have a validation date
                    if (status_id == 2)
                    { 
                        currentTask.ValidationDate = dataReader["validationDate"].ToString();
                    }
                }

                currentTask.Title = dataReader["title"].ToString();
                currentTask.Description = dataReader["description"].ToString();
                currentTask.Deadline = dataReader["deadline"].ToString();

                tasksList.Add(currentTask);
            }
            return tasksList;
        }

        /// <summary>
        /// Extracts the tasks from the database for a specified topic
        /// </summary>
        /// <returns>tasksList containing the result of the request</returns>
        public List<Tasks> ReadTaskForTopic(int topicId)
        {
            //Since we only want the status "To complete" (1) we add it here in the condition
            return ReadTask("WHERE Lists_id = " + topicId + " AND Status_id = 1 ORDER BY Priorities_id DESC;");
        }

        /// <summary>
        /// Extracts the tasks from the database for a specified day
        /// </summary>
        /// <returns>tasksList containing the result of the request</returns>
        public List<Tasks> ReadTaskForDate(string deadline)
        {
            //Since we only want the status "To complete" (1) we add it here in the condition
            return ReadTask("WHERE deadline = '" + deadline + "' AND Status_id = 1 ORDER BY Priorities_id DESC;");
        }

        /// <summary>
        /// Extracts the tasks from the database for the next 7 days
        /// </summary>
        /// <returns>tasksList containing the result of the request</returns>
        public List<Tasks> ReadTaskForDatePlusSeven(string[] deadline)
        {
            //Since we only want the status "To complete" (1) we add it here in the condition
            return ReadTask("WHERE deadline IN ('" + deadline[0] + "', '" + deadline[1] + "', '" + deadline[2] + "', '" + deadline[3] + "', '" + deadline[4] + "', '" + deadline[5] + "', '" + deadline[6] + "') AND Status_id = 1 ORDER BY Priorities_id DESC;");
        }

        /// <summary>
        /// Extracts the finished tasks from the database
        /// </summary>
        /// <returns>tasksList containing the result of the request</returns>
        public List<Tasks> ReadApprovedTask() 
        {
            //Status "done" (2)
            return ReadTask("WHERE Status_id = " + 2 + ";");
        }

        
        /// <summary>
        /// Reads and return the data of the table for all days that have deadlines assigned to one or more task(s)
        /// </summary>
        /// <returns>Tasklist containing the result of the request</returns>
        public List<string> ReadDataForDeadlines()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            // Getting the list of the deadlines
            cmd.CommandText = "SELECT DISTINCT deadline FROM Tasks;";

            // Declaration and instanciation of the list of DateTime
            List<string> deadlinesList = new List<string>();

            // Declaration of a SQLiteDataReader object which contains the results list
            SQLiteDataReader dataReader = cmd.ExecuteReader();

            // Browsing the results list
            while (dataReader.Read())
            {
                // Reading the value of the deadline column from the database and allocating it to a string variable.
                string myReader = dataReader["deadline"].ToString();

                // Adding the values of the column deadline into the reader object
                deadlinesList.Add(myReader);
            }
            // Return the list when it's built 
            return deadlinesList;
        }

        public void InsertDataPriorities(string Denomination, int PriorityLvl)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO Priorities (Denomination, PriorityLvl) VALUES ('" + Denomination + "','" + PriorityLvl + "');";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataLists(string Title)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO Lists (Title) VALUES ('" + Title + "');";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataStatus(string Denomination)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO Status (Denomination) VALUES('" + Denomination + "');";
            cmd.ExecuteNonQuery();
        }

        public void InsertDataStatus(string Title, string Description, DateTime Deadline)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "INSERT INTO TASKS(Title, Description, Deadline) VALUES ('" + Title + "','" + Description + "','" + Deadline + "');";
            cmd.ExecuteNonQuery();
        }


        /// <summary>
        /// Closing connection to the database
        /// </summary>
        public void Close()
        {
            // Close the connection to the database
            sqliteConn.Close();
        }
    }
}
