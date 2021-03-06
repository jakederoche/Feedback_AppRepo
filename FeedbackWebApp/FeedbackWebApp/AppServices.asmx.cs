﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
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
        User user = new User();

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
            string sqlSelect = "SELECT UserID, UserAdmin FROM users WHERE UserName=@idValue and UserPassword=@passValue";

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
            
            if (Session["id"] != null)
            {
                if (Session["user"] == null)
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

                        Session["user"] = user;
                    }
                }
                else
                {
                    user = Session["user"] as User;
                }
            }
            return user;
        }

        //views: 
        //UserCount --> NumUsers
        //ResponseCount --> NumResponses



        [WebMethod(EnableSession = true)]
        public Dashboard GetDashValues()
        {
           
            Dashboard dashboard = new Dashboard();
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("dashboard");
                
                //DataRow sqlDr = new DataRow("dashboard");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect1 = "select * from UserCount";
                string sqlSelect2 = "select * from ResponseCount";
                string sqlSelect3 = "select * from avesentimentq";


                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand1 = new MySqlCommand(sqlSelect1, sqlConnection);
                MySqlCommand sqlCommand2 = new MySqlCommand(sqlSelect2, sqlConnection);
                MySqlCommand sqlCommand3 = new MySqlCommand(sqlSelect3, sqlConnection);

                // Add the uid value that we get from the login page
                //sqlCommand.Parameters.AddWithValue("@uid", HttpUtility.UrlDecode(Session["id"].ToString()));

                MySqlDataAdapter sqlDa1 = new MySqlDataAdapter(sqlCommand1);
                MySqlDataAdapter sqlDa2 = new MySqlDataAdapter(sqlCommand2);
                MySqlDataAdapter sqlDa3 = new MySqlDataAdapter(sqlCommand3);

                sqlDa3.Fill(sqlDt);


                sqlConnection.Open();
                //we're using a try/catch so that if the query errors out we can handle it gracefully
                //by closing the connection and moving on
                try
                {
                    // Execute both sqlStatements and store each to the dashboard object
                    dashboard.totalEmployees = Convert.ToInt32(sqlCommand1.ExecuteScalar());
                    dashboard.totalResponses = Convert.ToInt32(sqlCommand2.ExecuteScalar());
                    dashboard.responseRate = Convert.ToDouble(dashboard.totalResponses) / Convert.ToDouble(dashboard.totalEmployees);
                    dashboard.responseRate *= 100;
                    dashboard.q1Sentiment = Convert.ToDouble(sqlDt.Rows[0]["avg(sentiment)"]);
                    dashboard.q2Sentiment = Convert.ToDouble(sqlDt.Rows[1]["avg(sentiment)"]);
                    dashboard.q3Sentiment = Convert.ToDouble(sqlDt.Rows[2]["avg(sentiment)"]);
                }
                catch (Exception e)
                {
                    throw e;
                }
                sqlConnection.Close();
            }

            return dashboard;
        }

        [WebMethod(EnableSession = true)]
        public TreeMap[] GetTreeValues()
        {

            Dashboard dashboard = new Dashboard();
            if (Session["id"] != null)
            {
                DataTable sqlDt1 = new DataTable("treemap");
                DataTable sqlDt2 = new DataTable("treemap");

                //DataRow sqlDr = new DataRow("dashboard");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect1 = "select * from treeWords";
                string sqlSelect2 = "select * from treeResponses2";
                


                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand1 = new MySqlCommand(sqlSelect1, sqlConnection);
                MySqlCommand sqlCommand2 = new MySqlCommand(sqlSelect2, sqlConnection);
                

                // Add the uid value that we get from the login page
                //sqlCommand.Parameters.AddWithValue("@uid", HttpUtility.UrlDecode(Session["id"].ToString()));

                MySqlDataAdapter sqlDa1 = new MySqlDataAdapter(sqlCommand1);
                MySqlDataAdapter sqlDa2 = new MySqlDataAdapter(sqlCommand2);
                

                sqlDa1.Fill(sqlDt1);
                sqlDa2.Fill(sqlDt2);


                sqlConnection.Open();
                //we're using a try/catch so that if the query errors out we can handle it gracefully
                //by closing the connection and moving on
                
                List<TreeMap> treeMaps = new List<TreeMap>();
                for (int i = 0; i < sqlDt1.Rows.Count; i++)
                {
                    treeMaps.Add(new TreeMap
                    {
                        response = sqlDt1.Rows[i]["WordsWord"].ToString(),
                        parent = sqlDt1.Rows[i]["Word Dictionary"].ToString(),
                        size = Convert.ToDouble(sqlDt1.Rows[i]["0"]),
                        sentiment = Convert.ToDouble(sqlDt1.Rows[i]["My_exp_0"])
                    });
                }
                for (int i = 0; i < sqlDt2.Rows.Count; i++)
                {
                    treeMaps.Add(new TreeMap
                    {
                        response = sqlDt2.Rows[i]["surveyResponse"].ToString(),
                        parent = sqlDt2.Rows[i]["wordsWord"].ToString(),
                        size = Convert.ToDouble(sqlDt2.Rows[i]["Size"]),
                        sentiment = Convert.ToDouble(sqlDt2.Rows[i]["sentiment"])
                    });
                }

                return treeMaps.ToArray();
            }
                
            else
            {
                return new TreeMap[0];
            }
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

        ////EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
        //[WebMethod(EnableSession = true)]
        //public Dashboard[] GetData()
        //{
        //    //check out the return type.  It's an array of Account objects.  You can look at our custom Account class in this solution to see that it's 
        //    //just a container for public class-level variables.  It's a simple container that asp.net will have no trouble converting into json.  When we return
        //    //sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
        //    //Keeps everything simple.

        //    //WE ONLY SHARE ACCOUNTS WITH LOGGED IN USERS!
        //     if (Session["id"] != null)
        //    {
        //        DataTable sqlDt = new DataTable("dashboard");

        //        string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
        //        string sqlSelect = "select totalResponses, totalEmployees, responseRate from dashboard_view where active=1";

        //        MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //        //gonna use this to fill a data table
        //        MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
        //        //filling the data table
        //        sqlDa.Fill(sqlDt);

        //        //loop through each row in the dataset, creating instances
        //        //of our container class Account.  Fill each acciount with
        //        //data from the rows, then dump them in a list.
        //        List<Dashboard> dashboard = new List<Dashboard>();
        //        for (int i = 0; i < sqlDt.Rows.Count; i++)
        //        {
        //            //only share user id and pass info with admins!
        //            if (Convert.ToInt32(Session["admin"]) == 1)
        //            {
        //                dashboard.Add(new Dashboard
        //                {

        //                    totalResponses = sqlDt.Rows[i]["totalResponses"].ToString(),
        //                    totalEmployees = sqlDt.Rows[i]["totalEmployees"].ToString(),
        //                    responseRate = sqlDt.Rows[i]["responseRate"].ToString(),
        //                });
        //            }
        //            else
        //            {
        //                dashboard.Add(new Dashboard
        //                {
        //                    totalResponses = sqlDt.Rows[i]["totalResponses"].ToString(),
        //                    totalEmployees = sqlDt.Rows[i]["totalEmployees"].ToString(),
        //                    responseRate = sqlDt.Rows[i]["responseRate"].ToString(),
        //                });
        //            }
        //        }
        //        //convert the list of accounts to an array and return!
        //        return dashboard.ToArray();
        //    }
        //    else
        //    {
        //        //if they're not logged in, return an empty array
        //        return new Dashboard[0];
        //    }
        //}

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
            response = HttpUtility.UrlDecode(response);
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
                int responseID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
                StoreKeyWords(response, responseID);

            }
            catch (Exception e)
            {
                throw e;
            }
            sqlConnection.Close();

        }

        [WebMethod(EnableSession = true)]
        private void StoreKeyWords(string response, int responseId)
        {
            List<string> tokenizedResponses = new List<string>();
            List<KeyWords> keyWords = new List<KeyWords>();
            tokenizedResponses = Tokenize(response).ToList<string>();

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            

            //sqlCommand.Parameters.AddWithValue("@responseID", HttpUtility.UrlDecode(ResponseID));
            //sqlCommand.Parameters.AddWithValue("@userID", HttpUtility.UrlDecode(tempKey);            
            //sqlCommand.Parameters.AddWithValue("@sentiment", HttpUtility.UrlDecode(responseSentiment.ToString()));

            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            //sqlConnection.Open();

            for (int i = 0; i < tokenizedResponses.Count; i++)
            {
                KeyWords tempKeyWord = new KeyWords();
                string word = tokenizedResponses[i];
                //// Temp List to store current cleaned response
                //List<string> tempList = new List<string>();

                // If the lowercase version of tokenized word IS NOT in Stop Words List
                if (!(StopWords.stopWordsList.Contains(word.ToLower())))
                {
                    string sqlSelect = "insert into words (wordsWord, responseID)" +
                            "values(@wordsWord, @responseId); SELECT LAST_INSERT_ID();";

                    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                   
                    //tempKeyWord.KeyWord = word;
                    //tempKeyWord.ResponseID = responseId.ToString();

                    //sqlCommand.Parameters.AddWithValue("@responseID", HttpUtility.UrlDecode(ResponseID));
                    sqlCommand.Parameters.AddWithValue("@wordsWord", HttpUtility.UrlDecode(word));
                    sqlCommand.Parameters.AddWithValue("@responseId", HttpUtility.UrlDecode(responseId.ToString()));
                                        
                    sqlConnection.Open();

                    try
                    {
                        int keyWordID = Convert.ToInt32(sqlCommand.ExecuteScalar());                     
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    sqlConnection.Close();
                }
            }

            // return keyWords.ToArray();

        }

        private static string[] Tokenize(string text)
        {
            // Strip all HTML.
            text = Regex.Replace(text, "<[^<>]+>", "");

            // Strip numbers.
            text = Regex.Replace(text, "[0-9]+", "number");

            // Strip urls.
            text = Regex.Replace(text, @"(http|https)://[^\s]*", "httpaddr");

            // Strip email addresses.
            text = Regex.Replace(text, @"[^\s]+@[^\s]+", "emailaddr");

            // Strip dollar sign.
            text = Regex.Replace(text, "[$]+", "dollar");

            // Strip usernames.
            text = Regex.Replace(text, @"@[^\s]+", "username");

            // Tokenize and also get rid of any punctuation
            return text.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
        }

        [WebMethod(EnableSession = true)]
        public Response[] GetResponses()
        {
            if(Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("responses");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "select ResponseID, surveyResponse, questionID, sentiment from response order by ResponseID";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);


                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                List<Response> responses = new List<Response>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    responses.Add(new Response
                    {
                        responseId = Convert.ToInt32(sqlDt.Rows[i]["ResponseID"]),
                        responseText = sqlDt.Rows[i]["surveyResponse"].ToString(),
                        questionId = Convert.ToInt32(sqlDt.Rows[i]["questionID"]),
                        sentiment = Convert.ToDouble(sqlDt.Rows[i]["sentiment"])
                    });
                }

                return responses.ToArray();
            }
            else
            {
                return new Response[0];
            }

            
        }

        
        private void tokenizeResponses()
        {
            Response[] responses = GetResponses();
            foreach (Response r in responses)
            {
                StoreKeyWords(r.responseText, r.responseId);
            }
        }

        [WebMethod(EnableSession = true)]
        public KeyWords[] getKeyWords()
        {
            if (Session["id"] != null)
            {
                DataTable sqlDt = new DataTable("responses");

                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "Select wordsWord, Count(wordsWord) As Frequency From words Group By wordsWord Order by 2 desc";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);


                //gonna use this to fill a data table
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                //filling the data table
                sqlDa.Fill(sqlDt);

                List<KeyWords> keyWords = new List<KeyWords>();
                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    keyWords.Add(new KeyWords
                    {
                        KeyWord = sqlDt.Rows[i]["wordsWord"].ToString(),
                        Frequency = Convert.ToInt32(sqlDt.Rows[i]["Frequency"])
                    });
                }

                return keyWords.ToArray();
            }
            else
            {
                return new KeyWords[0];
            }
        }

    }
}
