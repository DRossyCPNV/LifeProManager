using Microsoft.VisualStudio.TestTools.UnitTesting;
using LifeProManager;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace LifeProManager.Tests
{
    [TestClass()]
    public class frmMainTests
    {
        [TestMethod()]
        public void LoadTopicsTest_InsertTopic_TopicInsertedOK()
        {
            //Arrange
            frmMain frmMainTest = new frmMain();

            Lists expectedTopic = new Lists();
            expectedTopic.Title = "Expected Title";
            expectedTopic.Id = 1;

            Lists resultTopic = new Lists();

            //Act
                //Create test database
                SQLiteConnection sqliteConnTest;
                string filename = Application.StartupPath + @"\LPM_DB_Test.db";//@"C:\LifeProManager\App\LifeProManagerTests3\bin\Debug\LPM_DB_Test.db";
                sqliteConnTest = new SQLiteConnection("Data Source=" + filename + "; Version=3;");
                sqliteConnTest.Open();

                //Create test tables
                SQLiteCommand cmd = sqliteConnTest.CreateCommand();
                string createSql = "BEGIN TRANSACTION; " +
                                    //-- Creates table Lists 
                                    "DROP TABLE IF EXISTS 'Lists'; " +
                                    "CREATE TABLE IF NOT EXISTS 'Lists' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL);" +
                                    //-- Creates table Priorities 
                                    "DROP TABLE IF EXISTS 'Priorities'; " +
                                    "CREATE TABLE IF NOT EXISTS 'Priorities' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination' VARCHAR(25) NOT NULL);" +
                                    //-- Creates table Status 
                                    "DROP TABLE IF EXISTS 'Status'; " +
                                    "CREATE TABLE IF NOT EXISTS 'Status' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination'	VARCHAR(50) NOT NULL); " +
                                    //-- Creates table Tasks
                                    "DROP TABLE IF EXISTS 'Tasks'; " +
                                    "CREATE TABLE IF NOT EXISTS 'Tasks' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL, 'description' " +
                                    "VARCHAR(500) DEFAULT NULL, 'deadline' DATE DEFAULT NULL, 'validationDate' DATE DEFAULT NULL, 'Priorities_id' INTEGER NOT NULL, 'Lists_id' INTEGER NOT NULL, 'Status_id' " +
                                    "INTEGER NOT NULL, FOREIGN KEY ('Priorities_id') REFERENCES Priorities('id'), FOREIGN KEY ('Lists_id') REFERENCES Lists('id'), " +
                                    "FOREIGN KEY ('Status_id') REFERENCES Status('id')); " +
                                    "COMMIT; ";
                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                //Insert tested topic
                createSql = "INSERT INTO Lists VALUES(" + expectedTopic.Id + ", '" + expectedTopic.Title + "')";
                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                //Load the topic list
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

                //Load tested tasks from data base

                frmMainTest.cboTopics.Items.Clear();
                foreach (Lists topic in topicList)
                {
                    frmMainTest.cboTopics.Items.Add(topic);
                    frmMainTest.cboTopics.DisplayMember = "Title";
                    frmMainTest.cboTopics.ValueMember = "Id";
                }

                //Get the result
                resultTopic = frmMainTest.cboTopics.Items[0] as Lists;

                //Assert
                Assert.AreEqual(expectedTopic.Title, resultTopic.Title);
        }
    }
}