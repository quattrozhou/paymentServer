using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;

//define user profile data structure
//THIS ENUMERATION MUST ALIGN WITH THE CURRENT DATABASE MODEL
//DO NOT ADD OR MODIFY THIS ENUM UNLESS THE DATABASE MODEL HAS BEEN MODIFIED ACCORDINGLY
//DOING SO WILL YIELD CATASTROPHIC RESULTS
public enum UserProfileEnum
{
    userNo = 0,
    email = 1,
    username = 2,
    password = 3,
    userType = 4,
    firstName = 5,
    middleName = 6,
    lastName = 7,
    DOBDay = 8,
    DOBMonth = 9,
    DOBYear = 10,
    occupation = 11,
    SIN = 12,
    address1 = 13,
    address2 = 14,
    city = 15,
    province = 16,
    country = 17,
    postalCode = 18,
    phoneNumber = 19,
    receiveCommunication = 20,
    bankCode = 21,             //base64-encoded
    accountNum = 22,          //base64-encoded
    accountPWD = 23,      //base64-encoded
    acctBalance = 24,          //base64-encoded
    transactionHistory = 25,
    POSHWID = 26,
    currentDK = 27,            //base64-encoded
    nextD = 28,              //base64-encoded
    authenticationString = 29,  //base64-encoded
    createTime = 30,
    //All additions sould come above this line
    NUM_PROFILE_DATA_ITEMS
}

//define user profile data structure
public struct UserProfile
{
    public int userNo;
    public string email;
    public string username;
    public string password;             //base64-encoded
    public string userType;
    public string firstName;
    public string middleName;
    public string lastName;
    public int DOBDay;
    public int DOBMonth;
    public int DOBYear;
    public string occupation;
    public int SIN;
    public string address1;
    public string address2;
    public string city;
    public string province;
    public string country;
    public string postalCode;
    public int phoneNumber;
    public int receiveCommunication;
    public string bankCode;             //base64-encoded
    public string accountNum;           //base64-encoded
    public string accountPWD;      //base64-encoded
    public double acctBalance;          //base64-encoded
    public string transactionHistory;
    public int POSHWID;
    public string currentDK;            //base64-encoded
    public string nextDK;               //base64-encoded
    public string authenticationString;  //base64-encoded
    public string createTime;
};

public struct transaction{

};

namespace PaymentServer
{
    class paymentServer_dataBase
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public paymentServer_dataBase()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "localhost";
            database = "paymentserver";
            uid = "root";
            password = "root";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connected to database server.");
                return true;
            }
            catch (MySqlException ex)
            {
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to database server.");
                        break;

                    case 1045:
                        Console.WriteLine("Could not access database. Invalid username/password");
                        break;
                    default:
                        Console.WriteLine("Cannot connect to database server. Unknown exception");
                        break;
                }
                return false;
            }
        }

        //Close connection to database
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public int Count(string criteria1, string criteria2 )
        {
            string query = "SELECT Count(" + criteria1 + ") FROM " + criteria2;
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        //Insert statement
        public void Insert(string table, string columns, string values)
        {
            string query = "INSERT INTO "+table+" "+columns+" VALUES "+values; //(name, age) VALUES('John Smith', '33')";

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Update statement
        public void Update(string table, string items, string column)
        {
            string query = "UPDATE "+table+" SET "+items+" WHERE "+column; //name='Joe', age='22' WHERE name='John Smith'

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete(string table, string column , string value)
        {
            string query = "DELETE FROM " + table + " WHERE " + column + "='" + value + "'"; //name='John Smith'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        //Find statement
        public bool Find(string table, string column, string value)
        {
            bool itemFound = false;
            string query = "SELECT * FROM "+table+" WHERE "+column+"='"+value+"'";
            if(this.OpenConnection()==true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                if (cmd.ExecuteNonQuery() == 1)
                {
                    itemFound = true;
                }
                this.CloseConnection();
            }
            return itemFound;
        }

        //Select statement
        public List<string>[] Select(string table, string column, string value)
        {
            string query = "SELECT * FROM " +table+ " WHERE " +column+ "='" + value + "'"; 

            //Create a list to store the result
            int numRecords = Count("*", table);
            Console.WriteLine(numRecords);
            List<string>[] list = new List<string>[numRecords];
            for (int i = 0; i < numRecords; i++)
            {
                list[i] = new List<string>();
            }

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                int numReads = 0;
                string read = "";
                while (dataReader.Read())
                {
                    if (dataReader[numReads] != null)
                        read = ""+dataReader[numReads].ToString();
                    Console.WriteLine(list.Length+ " "+numReads+" "+read);
                    list[numReads].Add(read);
                    numReads++;
                    read = "";
                }
                if (numReads != numRecords)
                {
                    Console.WriteLine("MySQLDataHandler::Select - Warning! {0} columns expected, but only {1} were read", numRecords, numReads);
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Backup
        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                //Save file to the working directory with the current date as a filename
                string path;
                path = "" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to backup!");
            }
        }

        //Restore database
        public void Restore()
        {
            try
            {
                //Read file from C:\
                string path;
                path = "SQLDatabaseBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
                psi.UseShellExecute = false;


                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to Restore!");
            }
        }


    }
}
