﻿/// <file>DBConnection.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.0</version>
/// <date>November 7th, 2019</date>

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    class DBConnection
    {
        // Declaration of a private attribute of type SQLiteConnection
        SQLiteConnection sqliteConn;

        public DBConnection()
        {
            // Creates a new database connection :
            sqliteConn = new SQLiteConnection("Data Source=LPM_DB.db; Version=3; Compress=True;");
            // Opens the connection :
            sqliteConn.Open();
        }

        /// <summary>
        /// Creates the DB tables
        /// </summary>
        public void CreateTable()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            string createSql = "BEGIN TRANSACTION; " +
                                
                                //-- Creates table Lists 
                                "CREATE TABLE IF NOT EXISTS 'Lists' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL);" +
                                //-- Creates table Priorities 
                                "DROP TABLE IF EXISTS 'Priorities'; " +
                                "CREATE TABLE IF NOT EXISTS 'Priorities' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination' VARCHAR(25) NOT NULL);" +
                                //-- Creates table Status 
                                "DROP TABLE IF EXISTS 'Status'; " +
                                "CREATE TABLE IF NOT EXISTS 'Status' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination'	VARCHAR(50) NOT NULL); " +
                                //-- Creates table Tasks
                                "CREATE TABLE IF NOT EXISTS 'Tasks' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL, 'description' " +
                                "VARCHAR(500) DEFAULT NULL, 'deadline' DATE DEFAULT NULL, 'validationDate' DATE DEFAULT NULL, 'Priorities_id' INTEGER NOT NULL, 'Lists_id' INTEGER NOT NULL, 'Status_id' " + 
                                "INTEGER NOT NULL, FOREIGN KEY ('Priorities_id') REFERENCES Priorities('id'), FOREIGN KEY ('Lists_id') REFERENCES Lists('id'), " + 
                                "FOREIGN KEY ('Status_id') REFERENCES Status('id')); " + 
                                "COMMIT; ";
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
            /* 2 is id for "done" status*/
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
            /* 1 is id for "To do" status*/
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
        /// Inserts the priorities denominations into the database
        /// </summary>
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

        /// <summary>
        /// Reads the priorities denominations from the database
        /// </summary>
        /// <returns>Prioritieslist containing the result of the request</returns>
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

        /// <summary>
        /// Inserts the status denominations into the database
        /// </summary>
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

        /// <summary>
        /// Inserts the status denominations into the database
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
        /// Closes the connection to the database
        /// </summary>
        public void Close()
        {
            // Close the connection to the database
            sqliteConn.Close();
        }
    }
}