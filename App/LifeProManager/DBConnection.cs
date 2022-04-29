/// <file>DBConnection.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.4</version>
/// <date>April 29th, 2022</date>

using System;
using System.Collections.Generic;
using System.Data.SQLite;


namespace LifeProManager
{
    public class DBConnection
    {
        // Declaration of a private attribute of type SQLiteConnection
        private SQLiteConnection sqliteConn;

        // Constructor
        public DBConnection()
        {
            // Creates a new database connection :
            sqliteConn = new SQLiteConnection("Data Source=LPM_DB.db; Version=3; Compress=True;");
            // Opens the connection :
            sqliteConn.Open();
        }
       
        /// <summary>
        /// Checks the database integrity
        /// </summary>
        /// <returns>The status of the database. 
        /// True means correct, false means corrupted.</returns>
        public bool CheckDBIntegrity()
        {
            // Checks the database integrity
            try
            {
                // Tries to do a transaction and at once rolls it back
                using (var transaction = sqliteConn.BeginTransaction())
                {
                    transaction.Rollback();
                }
            }

            // If the database is corrupted an error is generated
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the database file in the app's installation folder
        /// </summary>
        public void CreateFile()
        {
            SQLiteConnection.CreateFile(@Environment.CurrentDirectory + "\\" + "LPM_DB.db");
        }

        /// <summary>
        /// Creates the DB tables
        /// </summary>
        public void CreateTablesAndInsertInitialData()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "BEGIN TRANSACTION;" +
                                "DROP TABLE IF EXISTS 'Status';" +
                                "CREATE TABLE IF NOT EXISTS 'Status'('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination' VARCHAR(50) NOT NULL);" +
                                "DROP TABLE IF EXISTS 'Settings';" +
                                "CREATE TABLE IF NOT EXISTS 'Settings'('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'settingName' TEXT NOT NULL, 'settingValue' INTEGER);" +
                                "DROP TABLE IF EXISTS 'Priorities';" +
                                "CREATE TABLE IF NOT EXISTS 'Priorities'('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination' VARCHAR(50) NOT NULL);" +
                                "DROP TABLE IF EXISTS 'Lists';" +
                                "CREATE TABLE IF NOT EXISTS 'Lists'('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL);" +
                                "DROP TABLE IF EXISTS 'Tasks';" +
                                "CREATE TABLE IF NOT EXISTS 'Tasks' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + 
                                "'title' VARCHAR(70) NOT NULL, 'description' VARCHAR(500) DEFAULT NULL, 'deadline'  DATE DEFAULT NULL, 'validationDate' DATE DEFAULT NULL, 'Priorities_id' INTEGER NOT NULL, 'Lists_id'  INTEGER NOT NULL, 'Status_id' INTEGER NOT NULL;" +
                                "FOREIGN KEY('Status_id') REFERENCES 'Status'('id')," +
                                "FOREIGN KEY('Priorities_id') REFERENCES 'Priorities'('id')," +
                                "FOREIGN KEY('Lists_id') REFERENCES 'Lists'('id')" + 
                                ")" +
                                "INSERT INTO 'Settings'('id', 'settingName', 'settingValue') VALUES(1, 'appNativeLanguage', 0);" +
                                "INSERT INTO 'Priorities'('id', 'denomination') VALUES(0, '');" +
                                "INSERT INTO 'Priorities'('id', 'denomination') VALUES(1, 'Important');" +
                                "INSERT INTO 'Priorities'('id', 'denomination') VALUES(2, 'Repeatable');" +
                                "INSERT INTO 'Priorities'('id', 'denomination') VALUES(3, 'ImportantAndRepeatable');" +
                                "COMMIT;";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts a topic in the database
        /// </summary>
        public void InsertTopic(String title)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "INSERT INTO Lists VALUES(NULL, '" + title.Replace("'", "''") + "')";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Reads the status of a given setting in the database
        /// </summary>
        /// <param name="idSetting"></param>the id of the setting
        /// <returns>The status of the setting</returns>
        public int ReadSetting(int idSetting)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "SELECT settingValue FROM 'Settings' WHERE id ='" + idSetting + "';";

            int settingValueFound = 0;

            SQLiteDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                int.TryParse(dataReader["settingValue"].ToString(), out settingValueFound);
            }
            return settingValueFound;
        }

        /// <summary>
        /// Reads the topics from the database
        /// </summary>
        /// <returns>Topiclist containing the result of the request</returns>
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

        /// <summary>
        /// Deletes a topic, given by its id, from the database
        /// </summary>
        /// <param name="id">The id number of the task</param>
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

        /// <summary>
        /// Inserts a task into the database
        /// </summary>
        /// <param name="title">The title of the task</param>
        /// <param name="description">The description of the task</param>
        /// <param name="deadline">The date for which the task is due</param>
        /// <param name="priorities_id">The level of priority for the task</param>
        /// <param name="lists_id">The id of the list to which the task was assigned</param>
        /// <param name="status_id">The id of the status to which the task was assigned</param>
        public void InsertTask(string title, string description, string deadline, int priorities_id, int lists_id, int status_id)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "INSERT INTO Tasks VALUES(NULL, '" + title + "', '" + description + "', '" + deadline + "', NULL, " + priorities_id + ", " + lists_id + ", " + status_id + ")";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Approves a task, given by its id, with the status "done" in the database
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="validationDate">The date when the task status was set to done</param>
        public void ApproveTask(int id, string validationDate)
        {
            // the id of value 2 is for "done" status
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "UPDATE Tasks " +
                               "SET validationDate = '" + validationDate + "', " +
                               "Status_id = " + 2 + " " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Unapprove a task
        /// </summary>
        /// <param name="id">The id of the task to unapprove</param>
        public void UnapproveTask(int id)
        {
            // the id of value 1 is for "To do" status
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "UPDATE Tasks " +
                               "SET validationDate = NULL, " +
                               "Status_id = " + 1 + " " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Edits a task in the database
        /// </summary>
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

        /// <summary>
        /// Deletes a task, given by its id, in the database
        /// </summary>
        /// <param name="id">The id of the task to delete</param>
        public void DeleteTask(int id)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "Delete from Tasks " +
                               "WHERE id = " + id + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes all done tasks in the database
        /// </summary>
        public void DeleteAllDoneTasks()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "Delete from Tasks WHERE Status_id = " + 2 + ";";
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Reads the status denominations from the database
        /// </summary>
        /// <returns>Statuslist containing the result of the request</returns>
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
        /// Extracts all the tasks from the database where the condition, given in argument, applies
        /// </summary>
        /// <returns>Taskslist containing the result of the request</returns>
        /// <param name="condition">The WHERE condition for the SQL request to the database</param>
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
        /// Extracts the tasks from the database for a specified topic, given in argument by its Id
        /// </summary>
        /// <returns>Taskslist containing the result of the request</returns>
        /// <param name="topicId">The id of the topic whose tasks are to be read</param>
        public List<Tasks> ReadTaskForTopic(int topicId)
        {
            //Since we only want the status "To complete" (1) we add it here in the condition
            return ReadTask("WHERE Lists_id = " + topicId + " AND Status_id = 1 ORDER BY Priorities_id DESC;");
        }

        /// <summary>
        /// Extracts the tasks from the database for a specified day, given in argument
        /// </summary>
        /// <returns>Taskslist containing the result of the request</returns>
        /// <param name="deadline">The date whose tasks are to be read</param>
        public List<Tasks> ReadTaskForDate(string deadline)
        {
            // Since we only want the status "To complete" (1) we add it here in the condition
            return ReadTask("WHERE deadline = '" + deadline + "' AND Status_id = 1 ORDER BY Priorities_id DESC;");
        }

        /// <summary>
        /// Extracts the tasks for the next 7 days from the database 
        /// </summary>
        /// <returns>Taskslist containing the result of the request</returns>
        /// <param name="deadline">The date whose tasks are to be read</param>
        public List<Tasks> ReadTaskForDatePlusSeven(string[] deadline)
        {
            //Since we only want the status "To complete" (1), we add it here in the condition
            return ReadTask("WHERE deadline IN ('" + deadline[0] + "', '" + deadline[1] + "', '" + deadline[2] + "', '" + deadline[3] + "', '" + deadline[4] + "', '" + deadline[5] + "', '" + deadline[6] + "') AND Status_id = 1 ORDER BY Priorities_id DESC;");
        }

        /// <summary>
        /// Extracts the finished tasks from the database
        /// </summary>
        /// <returns>Taskslist containing the result of the request</returns>
        public List<Tasks> ReadApprovedTask() 
        {
            // Status "done" (2)
            return ReadTask("WHERE Status_id = " + 2 + " ;");
        }

        
        /// <summary>
        /// Reads and return the data of the table for all days that have deadlines assigned to one or more task(s)
        /// </summary>
        /// <returns>Tasklist containing the result of the request</returns>
        public List<string> ReadDataForDeadlines()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            // Gets the list of the deadlines. Since we only want the ones with status "To complete" (1), we add it here in the condition.
            cmd.CommandText = "SELECT DISTINCT deadline FROM Tasks WHERE Status_id = 1;";

            // Declaration and instanciation of the list of DateTime
            List<string> deadlinesList = new List<string>();

            // Declaration of a SQLiteDataReader object which contains the results list
            SQLiteDataReader dataReader = cmd.ExecuteReader();

            // Browses the results list
            while (dataReader.Read())
            {
                // Reads the value of the deadline column from the database and allocating it to a string variable
                string myReader = dataReader["deadline"].ToString();

                // Adds the values of the column deadline into the reader object
                deadlinesList.Add(myReader);

            }
            // Returns the list when it's built 
            return deadlinesList;
        }

        /// <summary>
        /// Reads given topic id and returns the name of that topic
        /// </summary>
        /// <returns>The name of the topic</returns>
        public string ReadTopicName(int listId)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            
            // Gets the name of the topic by its id
            cmd.CommandText = "SELECT title FROM Lists WHERE id = '" + listId + "';";

            // Declaration of a SQLiteDataReader object which contains the results list
            SQLiteDataReader dataReader = cmd.ExecuteReader();

            string nameTopic = "";

            // Browses the results list
            while (dataReader.Read())
            {
                nameTopic = dataReader["title"].ToString();
            }

            return nameTopic;
        }

        /// <summary>
        /// Updates the value for a given setting
        /// </summary>
        /// <param name="idSetting">the id of the setting</param>
        /// <param name="valueSetting">the value of the setting to write</param>
        public void UpdateSetting(int idSetting, int valueSetting)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = "UPDATE 'Settings' SET settingValue ='" + valueSetting + "' WHERE id ='" + idSetting + "';";

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Closes the connection to the database
        /// </summary>
        public void Close()
        {
            // Close the connection to the database
            sqliteConn.Close();
        }
    }
}
