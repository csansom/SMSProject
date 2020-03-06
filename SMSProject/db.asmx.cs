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
            using (DB_A4A060_csEntities db = new DB_A4A060_csEntities())
            {
                var rows = db.Z_AlertLogs.Join(db.FarmCows,
                                               z_alerts => z_alerts.bolus_id,
                                               farm_cows => farm_cows.Bolus_ID,
                                               (z_alerts, farm_cows) => new { z_alerts, farm_cows })
                                               .Join(db.AspNetUsers,
                                               combined_entry => combined_entry.farm_cows.AspNetUser_ID,
                                               asp_users => asp_users.Id,
                                               (combined_entry, asp_users) => new {
                                                   msgID = combined_entry.z_alerts.id,
                                                   msg = combined_entry.z_alerts.message,
                                                   date = combined_entry.z_alerts.date_emailsent,
                                                   phoneNumber = asp_users.PhoneNumber
                                               });
                int messagesSent = 0;
                foreach (var row in rows)
                {
                    if (DateTime.Parse(row.date.ToString()).CompareTo(DateTime.Now.AddMinutes(-30)) >= 0)
                    {
                        // Fill in these feilds.
                        string login = "your sms feedback ID";
                        string password = "your sms feedback password";
                        string url = "http://api.smsfeedback.ru/messages/v2/send/?login=" + login + "&password=" + password + "&phone=%2B" + row.phoneNumber + "&text=" + row.msg;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "GET";
                        var response = request.GetResponse();
                        messagesSent++;
                    }

                        /*    
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
                            }*/
                }
                Context.Response.Output.WriteLine(messagesSent + " alert(s) were sent at " + DateTime.Now.ToString() + ".");
            }
            Context.Response.End();
            return string.Empty;
        }
    }
}
