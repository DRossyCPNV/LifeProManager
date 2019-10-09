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

        /// <summary>
        /// Create the table Tasks into the database
        /// </summary>
        public void CreateTable()
        {
            // Declaration of the cmd variable, call of the public method CreateCommand
            SQLiteCommand cmd = sqliteConn.CreateCommand();

            // Declaration of the createSQL variable, stocking in it the SQL request
            string createSql = "BEGIN TRANSACTION; " +
                                //-- Create table Lists 
                                "DROP TABLE IF EXISTS 'Lists'; " +
                                "CREATE TABLE IF NOT EXISTS 'Lists' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL);" +
                                //-- Create table Properties 
                                "DROP TABLE IF EXISTS 'Properties'; " +
                                "CREATE TABLE IF NOT EXISTS 'Properties' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination' VARCHAR(25) NOT NULL, " +
                                "'priority_level' INTEGER NOT NULL ); " +
                                //-- Create table Status 
                                "DROP TABLE IF EXISTS 'Status'; " +
                                "CREATE TABLE IF NOT EXISTS 'Status' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'denomination'	VARCHAR(50) NOT NULL); " +
                                //-- Create table Tasks
                                "DROP TABLE IF EXISTS 'Tasks'; " +
                                "CREATE TABLE IF NOT EXISTS 'Tasks' ('id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'title' VARCHAR(50) NOT NULL, 'description' " + 
                                "VARCHAR(500) DEFAULT NULL, 'deadline' DATE DEFAULT NULL, 'Properties_id' INTEGER NOT NULL, 'Lists_id' INTEGER NOT NULL, 'Status_id' " + 
                                "INTEGER NOT NULL, FOREIGN KEY ('Properties_id') REFERENCES Properties('id'), FOREIGN KEY ('Lists_id') REFERENCES Lists('id'), " + 
                                "FOREIGN KEY ('Status_id') REFERENCES Status('id')); " + 
                                "COMMIT; ";

            // Allocation of the createSQL variable to the CommandText property 
            cmd.CommandText = createSql;

            // Call of the ExecuteNonQuery method, which execute the request
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// Insertion de données dans la table
        /// </summary>
        public void InsertData()
        {
            // Declaration of the cmd variable, call of the public method CreateCommand
            SQLiteCommand cmd = sqliteConn.CreateCommand();

            // Declaration of the createSQL variable, stocking in it the SQL request. Inserting tasks exemples.
            // Beware to write the SQL values single quoted (') instead of double quoted ("). 
            // If single quotes are used for the title of a task, it's mandatory to escape them by typing another single quote ('') to avoid SQL syntax error.
            cmd.CommandText = "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Réviser un examen', /*Description*/NULL, /*Echeance*/'05-10-2019', /*Properties*/2, /*Lists*/1,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Boire un godet entre amis', /*Description*/NULL, /*Echeance*/'03-10-2019', /*Properties*/2, /*Lists*/1,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Trier ses notes de cours', /*Description*/NULL, /*Echeance*/'03-10-2019', /*Properties*/2, /*Lists*/1,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Faire les courses', /*Description*/NULL, /*Echeance*/'05-10-2019', /*Properties*/2, /*Lists*/2,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Organiser un rendez-vous de projet', /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/2, /*Lists*/2,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Regarder toute la saison d''une série TV', /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/2, /*Lists*/2,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Faire le ménage', /*Description*/NULL, /*Echeance*/'08-10-2019', /*Properties*/2, /*Lists*/3,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Appeler M. Jean Neymar', /*Description*/NULL, /*Echeance*/'06-10-2019', /*Properties*/2, /*Lists*/3,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Installer l''extension Git pour Visual Studio sur son PC personnel', /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/1,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Acheter des agrafes', /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/1,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Réviser une présentation orale', /*Description*/NULL, /*Echeance*/'08-10-2019', /*Properties*/3, /*Lists*/1,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Boire un café', /*Description*/NULL, /*Echeance*/'09-10-2019', /*Properties*/3, /*Lists*/2,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Trier sa chambre', /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/3, /*Lists*/2,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Faire du shopping', /*Description*/NULL, /*Echeance*/'05-10-2019', /*Properties*/3, /*Lists*/2,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Organiser une fête', /*Description*/NULL, /*Echeance*/'09-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Jouer avec le chat du voisin', /*Description*/NULL, /*Echeance*/'06-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Faire la lessive', /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Appeler un ami', /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/'Installer des mises à jour de sécurité', /*Description*/NULL, /*Echeance*/'09-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/3);";


            // Call of the ExecuteNonQuery method, which execute the request
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// Reads and return the data of the table for a selected day
        /// </summary>
        /// <returns>Tasklist containing the result of the request</returns></returns>
        public List<string> ReadDataForADay(string daySelected)
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            // Getting the list of the tasks
             cmd.CommandText = "SELECT title FROM Tasks WHERE deadline = '"+daySelected+"';";

            // Declaration and instanciation of the list of string
            List<string> taskList = new List<string>();

            // Declaration of a SQLiteDataReader object which contains the results list
            SQLiteDataReader dataReader = cmd.ExecuteReader();

            // Browsing the results list
            while (dataReader.Read())
            {
                // Converting the value of the title column from the database to a string and allocating it to a string variable.
                string myReader = dataReader["title"].ToString();

                // Adding the values of the column title into the reader object
                taskList.Add(myReader);
            }
            // Return the list when it's built 
            return taskList;
        }

        /// <summary>
        /// Reads and return the data of the table for all days that have deadlines assigned to one or more task(s)
        /// </summary>
        /// <returns>Tasklist containing the result of the request</returns></returns>
        public List<DateTime> ReadDataForDeadlines()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            // Getting the list of the deadlines
            cmd.CommandText = "SELECT DISTINCT deadline FROM Tasks;";

            // Declaration and instanciation of the list of DateTime
            List<DateTime> deadlinesList = new List<DateTime>();

            // Declaration of a SQLiteDataReader object which contains the results list
            SQLiteDataReader dataReader = cmd.ExecuteReader();

            // Browsing the results list
            while (dataReader.Read())
            {
                // Reading the value of the deadline column from the database and allocating it to a string variable.
                // object myReader = dataReader["deadline"];

                // Converting it to DataTime format 
                DateTime myDateTime = new DateTime();
                //myDateTime = Convert.ToDateTime(myReader);

                // Adding the values of the column title into the reader object
                deadlinesList.Add(myDateTime);
            }
            // Return the list when it's built 
            return deadlinesList;
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
