using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using VaderSharp;

namespace FeedbackWebApp
{
    /// <summary>
    /// Summary description for AppServices
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class AppServices : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
        public bool LogOn(string uid, string pass)
        {
            //we return this flag to tell them if they logged in or not
            //
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            //NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
            string sqlSelect = "SELECT UserID, UserAdmin FROM users WHERE UserFirstName=@idValue and UserPassword=@passValue";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //if we found an account, store the id and admin status in the session
                //so we can check those values later on other method calls to see if they 
                //are 1) logged in at all, and 2) and admin or not
                //Session["id"] = sqlDt.Rows[0]["UserID"];
                Session["id"] = sqlDt.Rows[0]["UserID"];
                Session["admin"] = sqlDt.Rows[0]["UserAdmin"];
                success = true;

                success = true;
            }
            //return the result!
            return success;
        }


        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            //if they log off, then we remove the session.  That way, if they access
            //again later they have to log back on in order for their ID to be back
            //in the session!
            Session.Abandon();
            return true;
        }

        [WebMethod(EnableSession = true)]
        public User GetUser()
        {
            User user = new User();
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("users");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select UserName, UserAdmin, UserFirstName, UserLastName, UserDepartment from users where UserID=@uid";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                // Add the uid value that we get from the login page
                sqlCommand.Parameters.AddWithValue("@uid", HttpUtility.UrlDecode(Session["id"].ToString()));

                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);

                sqlDa.Fill(sqlDt);
                

                if (sqlDt.Rows.Count == 1)
                {
                    user.userName = sqlDt.Rows[0]["UserName"].ToString();
                    user.firstName = sqlDt.Rows[0]["UserFirstName"].ToString();
                    user.lastName = sqlDt.Rows[0]["UserLastName"].ToString();
                    user.admin = sqlDt.Rows[0]["UserAdmin"].ToString();
                    user.department = sqlDt.Rows[0]["UserDepartment"].ToString();
                }

            }

            return user;
        }

        ////EXAMPLE OF AN UPDATE QUERY WITH PARAMS PASSED IN
        //[WebMethod(EnableSession = true)]
        //public void UpdateAccount(string UserID, string UserPassword, string UserAdmin, string UserFirstName,  string UserLastName, string UserEmpID, string UserDepartment, string UserDirectReport)
        //{

        //    string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
        //    //this is a simple update, with parameters to pass in values
        //    string sqlSelect = "update user_data set UserFirstName=@userFirstName, UserPassword=@passValue, UserAdmin=@userAdmin, UserLastName=@userLastName, " +
        //        "UserEmpID=@userEmpID, UserDepartment=@userDepartment, UserDirectReport=@userDirectReport where UserID=@uID";

        //    MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //    sqlCommand.Parameters.AddWithValue("@userFirstName", HttpUtility.UrlDecode(UserFirstName));
        //    sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(UserPassword));
        //    sqlCommand.Parameters.AddWithValue("@userAdmin", HttpUtility.UrlDecode(UserAdmin));
        //    sqlCommand.Parameters.AddWithValue("@userLastName", HttpUtility.UrlDecode(UserLastName));
        //    sqlCommand.Parameters.AddWithValue("@userEmpID", HttpUtility.UrlDecode(UserEmpID));
        //    sqlCommand.Parameters.AddWithValue("@userDepartment", HttpUtility.UrlDecode(UserDepartment));
        //    sqlCommand.Parameters.AddWithValue("@userDirectReport", HttpUtility.UrlDecode(UserDirectReport));
        //    sqlCommand.Parameters.AddWithValue("@uID", HttpUtility.UrlDecode(UserID));

        //    sqlConnection.Open();
        //    //we're using a try/catch so that if the query errors out we can handle it gracefully
        //    //by closing the connection and moving on
        //    try
        //    {
        //        sqlCommand.ExecuteNonQuery();
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //    sqlConnection.Close();

        //}

        //EXAMPLE OF A DELETE QUERY
        [WebMethod(EnableSession = true)]
        public void DeleteAccount(string id)
        {


            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //this is a simple update, with parameters to pass in values
            string sqlSelect = "delete from users where UserId=@idValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();

        }

        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        //public void CreateAccount(string UserName, string UserPassword, string UserAdmin, string UserFirstName, string UserLastName, string UserEmpID, string UserDepartment, string UserDirectReport)
        [WebMethod(EnableSession = true)]        
        public void CreateAccount(string UserName, string UserPassword, string UserAdmin, string UserFirstName, string UserLastName, string UserDepartment)
        {
            int accountID = -1;
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into users (UserName, UserPassword, UserAdmin, UserFirstName, UserLastName, UserEmpID, UserDepartment, UserDirectReport)" +
                "values(@nameValue, @passValue, @userAdmin, @userFirstName, @userLastName, 0, @userDepartment,  0); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@nameValue", HttpUtility.UrlDecode(UserName));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(UserPassword));
            sqlCommand.Parameters.AddWithValue("@userAdmin", HttpUtility.UrlDecode(UserAdmin));
            sqlCommand.Parameters.AddWithValue("@userFirstName", HttpUtility.UrlDecode(UserFirstName));
            sqlCommand.Parameters.AddWithValue("@userLastName", HttpUtility.UrlDecode(UserLastName));
            //sqlCommand.Parameters.AddWithValue("@userEmpID", HttpUtility.UrlDecode(UserEmpID));
            sqlCommand.Parameters.AddWithValue("@userDepartment", HttpUtility.UrlDecode(UserDepartment));
            //sqlCommand.Parameters.AddWithValue("@userDirectReport", HttpUtility.UrlDecode(UserDirectReport));
            //qlCommand.Parameters.AddWithValue("@uID", HttpUtility.UrlDecode(UserID));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
                                
            }
            catch (Exception e)
            {
                throw e;                
            }
            sqlConnection.Close();
            //return accountID;

        }

        public double GetSentiment(string text, string sentimentType = "Compound")
        {
            /// Receives a text to be analyzed,
            /// sentimentType parameter will be what sentiment analysis to return:
            ///     Sentiment Types are "Positive", "Negative", "Neutral", or "Compound"
            ///     Type defaults to compound score

            // Creates an analyzer object
            SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();

            var results = analyzer.PolarityScores(text);

            switch(sentimentType)
            {
                case "Positive":
                    return Convert.ToDouble(results.Positive);
                case "Negative":
                    return Convert.ToDouble(results.Negative);
                case "Neutral":
                    return Convert.ToDouble(results.Neutral);
                default:
                    return Convert.ToDouble(results.Compound);
            }
                
             

        }

        //EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
        [WebMethod(EnableSession = true)]
        public Dashboard[] GetData()
        {
            //check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
            //just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
            //sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
            //Keeps everything simple.

            //WE ONLY SHARE ACCOUNTS WITH LOGGED IN USERS!
             if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("dashboard");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select totalResponses, totalEmployees, responseRate from dashboard_view where active=1";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                //loop through each row in the dataset, creating instances
                //of our container class Account.  Fill each acciount with
                //data from the rows, then dump them in a list.
                List<Dashboard> dashboard = new List<Dashboard>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    //only share user id and pass info with admins!
                    if (Convert.ToInt32(Session["admin"]) == 1)
                    {
                        dashboard.Add(new Dashboard
                        {

                            totalResponses = sqlDt.Rows[i]["totalResponses"].ToString(),
                            totalEmployees = sqlDt.Rows[i]["totalEmployees"].ToString(),
                            responseRate = sqlDt.Rows[i]["responseRate"].ToString(),
                        });
                    }
                    else
                    {
                        dashboard.Add(new Dashboard
                        {
                            totalResponses = sqlDt.Rows[i]["totalResponses"].ToString(),
                            totalEmployees = sqlDt.Rows[i]["totalEmployees"].ToString(),
                            responseRate = sqlDt.Rows[i]["responseRate"].ToString(),
                        });
                    }
                }
                //convert the list of accounts to an array and return!
                return dashboard.ToArray();
            }
            else
            {
                //if they're not logged in, return an empty array
                return new Dashboard[0];
            }
        }

        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        //public void CreateAccount(string UserName, string UserPassword, string UserAdmin, string UserFirstName, string UserLastName, string UserEmpID, string UserDepartment, string UserDirectReport)
        // Receives an array of survey responses, then passes them individually to the Store Responses function to be stored in the database
        [WebMethod(EnableSession = true)]
        public void SurveyResponses(string r1, string r2, string r3)
        {
            string[] surveyResponses = new string[] { r1, r2, r3 };
            for(int i = 0; i < surveyResponses.Length; i++)
            {
                StoreResponse(i + 1, surveyResponses[i]);
            }

        }

        // Method to store the response data into the database
        // Will be called from 
        [WebMethod(EnableSession = true)]
        private void StoreResponse(int questionNumber, string response)
        {
            double responseSentiment = GetSentiment(response);
            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into response (userID, surveyResponse, questionID, surveyID, sentiment)" +
                "values(@userID, @surveyResponse, @questionID, @surveyID, @sentiment); SELECT LAST_INSERT_ID();";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@responseID", HttpUtility.UrlDecode(ResponseID));
            sqlCommand.Parameters.AddWithValue("@userID", HttpUtility.UrlDecode(Session["id"].ToString()));
            sqlCommand.Parameters.AddWithValue("@surveyResponse", HttpUtility.UrlDecode(response));
            sqlCommand.Parameters.AddWithValue("@questionID", HttpUtility.UrlDecode(questionNumber.ToString()));
            sqlCommand.Parameters.AddWithValue("@surveyID", HttpUtility.UrlDecode("1"));
            sqlCommand.Parameters.AddWithValue("@sentiment", HttpUtility.UrlDecode(responseSentiment.ToString()));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!

            }
            catch (Exception e)
            {
                throw e;
            }
            sqlConnection.Close();

        }

    }
}
