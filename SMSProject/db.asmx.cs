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
    // [System.Web.Script.Services.ScriptService
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
                                                   username = asp_users.UserName,
                                                   msgID = combined_entry.z_alerts.id,
                                                   msg = combined_entry.z_alerts.message,
                                                   date = combined_entry.z_alerts.date_emailsent,
                                                   phoneNumber = asp_users.PhoneNumber
                                               });
                int messagesSent = 0;
                List<Log> logEntries = new List<Log>();
                foreach (var row in rows)
                {
                    if (DateTime.Parse(row.date.ToString()).CompareTo(DateTime.Now.AddMinutes(-30)) >= 0)
                    {
                        string message = row.msg.Replace(';', ',');

                        // Fill in these feilds.
                        string login = "your SMS Feedback username";
                        string password = "your SMS Feedback password";
                        string url = "http://api.smsfeedback.ru/messages/v2/send/?login=" + login + "&password=" + password + "&phone=%2B" + row.phoneNumber + "&text=" + message;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "GET";
                        var response = (HttpWebResponse)request.GetResponse();
                        string note = "\' encountered an error while sending.";
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            messagesSent++;
                            note = "\' has been sent.";
                        }
                        logEntries.Add(new Log
                        {
                            id = row.msgID,
                            user_id = row.username,
                            page = HttpContext.Current.Request.Url.AbsoluteUri,
                            function_query = "SendAlerts",
                            error = response.StatusCode.ToString(),
                            note = "message:\'" + message + note,
                            datestamp = DateTime.Now
                        });
                    }
                }
                foreach (Log logRow in logEntries)
                {
                    db.Logs.Add(logRow);
                    db.SaveChanges();
                }
                Context.Response.Output.WriteLine(messagesSent + " alert(s) were sent at " + DateTime.Now.ToString() + ".");
            }
            Context.Response.End();
            return string.Empty;
        }
    }
}
