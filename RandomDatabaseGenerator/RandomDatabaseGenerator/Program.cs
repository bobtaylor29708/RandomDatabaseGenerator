//This Sample Code is provided for the purpose of illustration only and is not
//    intended to be used in a production environment.THIS SAMPLE CODE AND ANY
//   RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//	EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED

//    WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//We grant You a nonexclusive, royalty-free right to use and modify the Sample

//    Code and to reproduce and distribute the object code form of the Sample

//    Code, provided that You agree: 

//    (i) to not use Our name, logo, or trademarks to market Your software

//        product in which the Sample Code is embedded;
//(ii) to include a valid copyright notice on Your software product in 
//		which the Sample Code is embedded; and
//    (iii) to indemnify, hold harmless, and defend Us and Our suppliers from
//        and against any claims or lawsuits, including attorneys fees, that
//        arise or result from the use or distribution of the Sample Code.

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;


namespace RandomDatabaseGenerator
{
    class Program
    {
        #region Local variables
        public static bool CreateSQLScript;
        public static string connectionString;
        public static bool SQLServerDestination;

        public static int NumberOfTablesToCreate;
        public static int MaxNumberOfColumnsPerTable;
        public static int MaxNumberOfRowsPerTable;

        public static string DatabaseName;
        public static Random r;
        public static FileStream script = null;
        public static string ScriptFolderPath;
        public static FileStream logFile = null;
        #endregion  
        
        static void Main(string[] args)
        {
            GetConfigurationValues();
            logFile = File.Create(ScriptFolderPath + "Activity.log");
            r = new Random();
            DateTime startTime = DateTime.Now;
            WriteInColor(ConsoleColor.Yellow, string.Format("Started at :{0}", startTime),true);
            if (CreateSQLScript == true)
            {
                script = File.Create(ScriptFolderPath + "Output.sql");
            }
            InitializeDB();
            CreateTables();
            Console.WriteLine("Complete");
            DateTime endTime = DateTime.Now;
            WriteInColor(ConsoleColor.Yellow, string.Format("Finished at :{0}", endTime),true);
            WriteInColor(ConsoleColor.Yellow, string.Format("Elapsed Time {0}", endTime - startTime),true);
            WriteInColor(ConsoleColor.Red, "Press any key to end.",true);
            script.Flush();
            script.Close();
            logFile.Flush();
            logFile.Close();
            Console.ReadKey(true);
        }
        
        // Read all configuration items from App.Config and populate local variables
        public static void GetConfigurationValues()
        {
            if (ConfigurationManager.AppSettings["NumberOfTablesToCreate"] != null)
            {
                int value = 1000;
                int.TryParse(ConfigurationManager.AppSettings["NumberOfTablesToCreate"], out value);
                NumberOfTablesToCreate = value;
            }
            if (ConfigurationManager.AppSettings["MaxNumberOfColumnsPerTable"] != null)
            {
                int value = 8;
                int.TryParse(ConfigurationManager.AppSettings["MaxNumberOfColumnsPerTable"], out value);
                MaxNumberOfColumnsPerTable = value;
            }
            if (ConfigurationManager.AppSettings["MaxNumberOfRowsPerTable"] != null)
            {
                int value = 10;
                int.TryParse(ConfigurationManager.AppSettings["MaxNumberOfRowsPerTable"], out value);
                MaxNumberOfRowsPerTable = value;
            }
            if (ConfigurationManager.AppSettings["CreateSQLScript"] != null)
            {
                bool value = false;
                bool.TryParse(ConfigurationManager.AppSettings["CreateSQLScript"], out value);
                CreateSQLScript = value;
            }
            if (ConfigurationManager.AppSettings["DatabaseName"] != null)
            {
                DatabaseName = ConfigurationManager.AppSettings["DatabaseName"].ToString();
            }
            if (ConfigurationManager.AppSettings["ScriptFolderPath"] != null)
            {
                ScriptFolderPath = ConfigurationManager.AppSettings["ScriptFolderPath"].ToString();
            }
            if (ConfigurationManager.AppSettings["SQLServerDestination"] != null)
            {
                bool value = false;
                bool.TryParse(ConfigurationManager.AppSettings["SQLServerDestination"], out value);
                SQLServerDestination = value;
            }
            if (ConfigurationManager.ConnectionStrings["Destination"] != null)
            {
                connectionString = ConfigurationManager.ConnectionStrings["Destination"].ToString();
            }
        }

        // Will remove the database specified by the DatabaseName in the App.Config file
        // and recreate it
        private static void InitializeDB()
        {

            WriteInColor(ConsoleColor.White, "Re-initilizing database",true);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    //command.CommandText = "USE master; ALTER DATABASE " + DatabaseName + " SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    //command.ExecuteNonQuery();
                    //WriteToFile(command.CommandText);
                    command.CommandText = "USE master; DROP DATABASE IF EXISTS " + DatabaseName + ";";
                    command.ExecuteNonQuery();
                    WriteToFile(command.CommandText);
                    command.CommandText = "USE master; CREATE DATABASE " + DatabaseName + ";";
                    command.ExecuteNonQuery();
                    WriteToFile(command.CommandText);
                    command.CommandText = "USE " + DatabaseName + ";";
                    command.ExecuteNonQuery();
                    WriteToFile(command.CommandText);
                }
            }
        }

        // Based on the configurations values we will create X tables with a random number
        // of rows up to the maximum specified in App.Config
        private static void CreateTables()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataTypes t = new DataTypes(SQLServerDestination);
                StringBuilder sql = new StringBuilder();
                StringBuilder insert = new StringBuilder();
                StringBuilder insert2 = new StringBuilder();
                connection.Open();
                connection.ChangeDatabase(DatabaseName);
                using (SqlCommand command = new SqlCommand(sql.ToString(), connection))
                {
                    for (int tables = 0; tables < NumberOfTablesToCreate; tables++)
                    {
                        sql = new StringBuilder("CREATE TABLE Test" + tables.ToString() + "(");
                        insert2 = new StringBuilder();
                        sql.Append("id  int NOT NULL,");
                        int randomColumnCount = r.Next(3, MaxNumberOfColumnsPerTable);
                        for (int columns = 0; columns < randomColumnCount; columns++)
                        {
                            int index = r.Next(0, 13);
                            sql.Append(string.Format("Col{0} {1}", columns.ToString(), t.GetDataTypeString(index)));
                            insert2.Append("," + t.GetDataTypeValue(index));
                        }
                        sql.Append(")");
                        command.CommandText = sql.ToString();
                        WriteToFile(command.CommandText);
                        command.ExecuteNonQuery();
                        WriteInColor(ConsoleColor.Green, string.Format("Creating table: {0} with {1} columns.\t",tables.ToString(),randomColumnCount));
                        int randomRowCount = r.Next(1, MaxNumberOfRowsPerTable);
                        for (int count = 0; count < randomRowCount; count++)
                        {
                            insert = new StringBuilder("Insert into Test" + tables.ToString() + " VALUES(");
                            insert.Append(count.ToString());
                            insert.Append(insert2.ToString());
                            insert.Append(")");
                            command.CommandText = insert.ToString();
                            command.ExecuteNonQuery();
                            WriteToFile(command.CommandText);
                        }
                        WriteInColor(ConsoleColor.Yellow, string.Format("Adding {0} rows.", randomRowCount),true);
                    }
                }
            }
        }

        #region Helper Functions
        // Writes a message in color to the console. Also duplicates that message 
        // in the activity log
        private static void WriteInColor(ConsoleColor c, string message, bool addReturn = false)
        {
            Console.ForegroundColor = c;
            if (addReturn)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ResetColor();
            WriteToLog(message, addReturn);
        }
        // Writes a message in color to the console. Also duplicates that message 
        // in the activity log
        //private static void WriteLineInColor(ConsoleColor c, string message)
        //{
        //    Console.ForegroundColor = c;
        //    Console.WriteLine(message);
        //    Console.ResetColor();
        //    WriteToLog(message ,true);
        //}
        // As we create the CREATE TABLE and INSERT statements, save them to the script file.
        private static void WriteToFile(string message)
        {
            if (script != null)
            {
                Byte[] info = new UTF8Encoding(false).GetBytes(message + "\r\n");
                script.Write(info, 0, info.Length);
            }
        }
        // Method to localize all writes to the activity log. When called from WriteInColor 
        // we will not append a cararige return.
        private static void WriteToLog(string message, bool addReturn = false)
        {
            if (logFile != null)
            {
                message += (addReturn) ? "\r\n" : "";
                Byte[] info = new UTF8Encoding(false).GetBytes(message);
                logFile.Write(info, 0, info.Length);
            }
        }
        #endregion

    }
}
