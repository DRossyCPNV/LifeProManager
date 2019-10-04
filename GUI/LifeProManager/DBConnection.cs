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
        //Déclaration de l'attribut privé de type SQLiteConnection
        SQLiteConnection sqliteConn;
        public DBConnection()
        {
            // Create a new database connection:
            sqliteConn = new SQLiteConnection("Data Source=LPM_DB.db; Version=3; Compress=True;");
            // Open the connection:
            sqliteConn.Open();

        }

        /// <summary>
        /// Crée la table Tasks dans la BD
        /// </summary>
        public void CreateTable()
        {
            //Déclaration de la variable cmd, appel de la méthode publique CreateCommand
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            //Déclaration de la variable createSQL, y stocke la requête SQL
            string createSql = "BEGIN TRANSACTION; " +
                                //-- Create table Lists 
                                "DROP TABLE IF EXISTS `Lists`; " +
                                "CREATE TABLE IF NOT EXISTS `Lists` (`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `title` VARCHAR(50) NOT NULL);" +
                                //-- Create table Properties 
                                "DROP TABLE IF EXISTS `Properties`; " +
                                "CREATE TABLE IF NOT EXISTS `Properties` (`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `denomination` VARCHAR(25) NOT NULL, " +
                                "`priority_level` INTEGER NOT NULL ); " +
                                //-- Create table Status 
                                "DROP TABLE IF EXISTS `Status`; " +
                                "CREATE TABLE IF NOT EXISTS `Status` (`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `denomination`	VARCHAR(50) NOT NULL); " +
                                //-- Create table Tasks
                                "DROP TABLE IF EXISTS `Tasks`; " +
                                "CREATE TABLE IF NOT EXISTS `Tasks` (`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `title` VARCHAR(50) NOT NULL, `description` " + 
                                "VARCHAR(500) DEFAULT NULL, `deadline` DATE DEFAULT NULL, `Properties_id` INTEGER NOT NULL, `Lists_id` INTEGER NOT NULL, `Status_id` " + 
                                "INTEGER NOT NULL, FOREIGN KEY (`Properties_id`) REFERENCES Properties(`id`), FOREIGN KEY (`Lists_id`) REFERENCES Lists(`id`), " + 
                                "FOREIGN KEY (`Status_id`) REFERENCES Status(`id`)); " + 
                                "COMMIT; ";
           
            //Affectation de la propriété CommandText avec la variable createSQL
            cmd.CommandText = createSql;
            //Appel la méthode ExecuteNonQuery qui exécute la requête
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// Insertion de données dans la table
        /// </summary>
        public void InsertData()
        {
            //Déclaration de la variable cmd, appel de la méthode publique CreateCommand
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            //Déclaration de la variable createSQL, y stocke la requête SQL. On insère des exemples de tâches.
            //Attention à échapper les guillemets des requêtes SQL (\" au lieu de \), afin de ne pas rompre la chaîne de caractères affectée à cmd
            cmd.CommandText = "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Réviser un examen\", /*Description*/NULL, /*Echeance*/'05-10-2019', /*Properties*/2, /*Lists*/1,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Boire un godet entre amis\", /*Description*/NULL, /*Echeance*/'03-10-2019', /*Properties*/2, /*Lists*/1,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Trier ses notes de cours\", /*Description*/NULL, /*Echeance*/'03-10-2019', /*Properties*/2, /*Lists*/1,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Faire les courses\", /*Description*/NULL, /*Echeance*/'05-10-2019', /*Properties*/2, /*Lists*/2,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Organiser un rendez-vous de projet\", /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/2, /*Lists*/2,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Regarder toute la saison d'une série TV\", /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/2, /*Lists*/2,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Faire le ménage\", /*Description*/NULL, /*Echeance*/'06-10-2019', /*Properties*/2, /*Lists*/3,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Appeler M. Jean Neymar\", /*Description*/NULL, /*Echeance*/'06-10-2019', /*Properties*/2, /*Lists*/3,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Installer l'extension Git pour Visual Studio sur son PC personnel\", /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/1,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Acheter des agrafes\", /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/1,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Réviser une présentation orale\", /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/1,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Boire un café\", /*Description*/NULL, /*Echeance*/'09-10-2019', /*Properties*/3, /*Lists*/2,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Trier sa chambre\", /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/3, /*Lists*/2,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Faire du shopping\", /*Description*/NULL, /*Echeance*/'05-10-2019', /*Properties*/3, /*Lists*/2,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Organiser une fête\", /*Description*/NULL, /*Echeance*/'09-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/1); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Jouer avec le chat du voisin\", /*Description*/NULL, /*Echeance*/'06-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/2); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Faire la lessive\", /*Description*/NULL, /*Echeance*/'07-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Appeler un ami\", /*Description*/NULL, /*Echeance*/'04-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/3); " +
            "INSERT INTO Tasks VALUES(/*Id*/NULL, /*Titre*/\"Installer des mises à jour de sécurité\", /*Description*/NULL, /*Echeance*/'09-10-2019', /*Properties*/3, /*Lists*/3,/*Status*/3);";


            //Appel de la méthode ExecuteNonQuery qui exécute la requête
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// Lecture des données de la table
        /// </summary>
        /// <returns>liste de tâches contenant le résultat de la requête</returns>
        public List<string> ReadData()
        {
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            //On récupère la liste des tâches
             cmd.CommandText = "SELECT title FROM Tasks WHERE deadline = '06-10-2019'";
            //Déclaration et instanciation de la liste de string
            List<string> taskList = new List<string>();

            //Déclaration d'un SQLiteDataReader qui contient la liste des résultats
            SQLiteDataReader dataReader = cmd.ExecuteReader();
            //Parcours de la liste des résultats
            while (dataReader.Read())
            {
                //Déclaration de la variable de type string, affectation avec la valeur de la colonne title
                string myReader = dataReader["title"].ToString();
                //Ajout de la colonne title dans la liste
                taskList.Add(myReader);
            }
            //Retourne la liste une fois construite
            return taskList;
        }


        /// <summary>
        /// Fermeture de la connection à la base de données
        /// </summary>
        public void Close()
        {
            //Ferme la connection à la BD
            sqliteConn.Close();
        }
    }
}
