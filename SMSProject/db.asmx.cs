using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace SMSProject
{
    /// <summary>
    /// Summary description for db
    /// </summary>
    [WebService(Namespace = "http://localhost/SMSProject/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class db : System.Web.Services.WebService
    {

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SendAlerts()
        {
            using (SqlConnection sqlCon = new SqlConnection(@"Data Source=DESKTOP-N3SBF80\SQLEXPRESS;Initial Catalog=DB_A4A060_cs;Integrated Security=True;MultipleActiveResultSets=true"))
            {
                sqlCon.Open();
                using (SqlDataAdapter sqlDA = new SqlDataAdapter("select * from Z_AlertLogs left join Log on Z_AlertLogs.id = Log.id where Z_AlertLogs.date_emailsent >= convert(datetime2, \'" + DateTime.Today + "\') and Log.id is null", sqlCon))
                {
                    DataTable outputTable = new DataTable();
                    sqlDA.Fill(outputTable);
                    int messagesSent = 0;
                    foreach (DataRow row in outputTable.Rows)
                    {
                        using (SqlCommand recipientCommand = new SqlCommand("select name from FarmInfo where phone_number = " + row["phone_number"], sqlCon))
                        {
                            SqlDataReader reader = recipientCommand.ExecuteReader();
                            string recipient = "";
                            while (reader.Read())
                                recipient = reader.GetString(0);

                            // Fill in these feilds.
                            string login = "your SMSFeedback login";
                            string password = "your SMSFeedback password";

                            string url = "http://api.smsfeedback.ru/messages/v2/send/?login=" + login + "&password=" + password + "&phone=%2B" + row["phone_number"] + "&text=" + row["message"];

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            request.Method = "GET";
                            var response = request.GetResponse();

                            using (SqlCommand command = new SqlCommand("set IDENTITY_INSERT dbo.Log on; insert into Log(id, user_id, page, function_query, error, note, datestamp) VALUES(@id, @user_id, @page, @function_query, @error, @note, @datestamp); set IDENTITY_INSERT dbo.Log off;", sqlCon))
                            {
                                string note = "Hello " + recipient + ".\n" + row["message"];

                                command.Parameters.AddWithValue("@id", row["id"]);
                                command.Parameters.AddWithValue("@user_id", "SMSService");
                                command.Parameters.AddWithValue("@page", "/SMSProject/db.asmx/SendAlerts");
                                command.Parameters.AddWithValue("@function_query", "insert into log(id, user_id, page, function_query, error, note, datestamp) VALUES(@id, @user_id, @page, @function_query, @error, @note, @datestamp)");
                                command.Parameters.AddWithValue("@error", "No Error");
                                command.Parameters.AddWithValue("@note", "message:\'" + note + "\' has been sent.");
                                command.Parameters.AddWithValue("@datestamp", DateTime.Today);

                                int result = command.ExecuteNonQuery();

                                messagesSent++;
                            }
                        }
                    }
                    Context.Response.Output.WriteLine(messagesSent + " alert(s) send.");
                }
                sqlCon.Close();
            }
            Context.Response.End();
            return string.Empty;
        }
    }
}
