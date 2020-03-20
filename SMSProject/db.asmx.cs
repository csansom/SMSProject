using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
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
        public string EnableDisable()
        {
            var sendSMS = Boolean.Parse(ConfigurationManager.AppSettings["sendSMS"]);
            sendSMS = !sendSMS;
            ConfigurationManager.AppSettings["sendSMS"] = sendSMS.ToString();
            if (sendSMS)
                Context.Response.Output.WriteLine("Disable SMS Service");
            else
                Context.Response.Output.WriteLine("Enable SMS Service");
            Context.Response.End();
            return string.Empty;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SendAlerts()
        {
            if (Boolean.Parse(ConfigurationManager.AppSettings["sendSMS"]))
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
                                                   (combined_entry, asp_users) => new
                                                   {
                                                       username = asp_users.UserName,
                                                       msg = combined_entry.z_alerts.message,
                                                       date = combined_entry.z_alerts.date_emailsent,
                                                       phoneNumber = asp_users.PhoneNumber
                                                   }).Distinct();
                    int messagesSent = 0;
                    List<Log> logEntries = new List<Log>();
                    foreach (var row in rows)
                    {
                        if (DateTime.Parse(row.date.ToString()).CompareTo(DateTime.Now.AddMinutes(-30)) >= 0)
                        {
                            string message = row.msg.Replace(';', ',');

                            // Fill in these feilds.
                            string login = "csansom";
                            string password = "b8f26140e4837dc4bba68ded9504a7f3";
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
            } else
            {
                Context.Response.Output.WriteLine("SMS Service is currently disabled.");
            }
            Context.Response.End();
            return string.Empty;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStats()
        {
            using (DB_A4A060_csEntities db = new DB_A4A060_csEntities())
            {
                var timeMinusThirtyMinutes = DateTime.Now.AddMinutes(-30);
                var sevenDaysAgo = DateTime.Today.AddDays(-7);
                var thirtyDaysAgo = DateTime.Today.AddDays(-30);
                var lastHalfHour = db.Logs.Where(l => l.datestamp >= timeMinusThirtyMinutes).Select(l => l.id).Count();
                var lastDay = db.Logs.Where(l => l.datestamp >= DateTime.Today).Select(l => l.id).Count();
                var lastWeek = db.Logs.Where(l => l.datestamp >= sevenDaysAgo).Select(l => l.id).Count();
                var lastMonth = db.Logs.Where(l => l.datestamp >= thirtyDaysAgo).Select(l => l.id).Count();
                var allTime = db.Logs.Select(l => l.id).Count();

                string stats = lastHalfHour.ToString() + ";" + lastDay.ToString() + ";" + lastWeek.ToString() + ";" + lastMonth.ToString() + ";" + allTime.ToString();
                Context.Response.Output.WriteLine(stats);
            }
            Context.Response.End();
            return string.Empty;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetUsers()
        {
            using (DB_A4A060_csEntities db = new DB_A4A060_csEntities())
            {
                string response = "";
                var users = db.AspNetUsers.Select(u => new { u.UserName, u.PhoneNumber });
                foreach (var user in users)
                {
                    response = response + user.UserName + " (" + user.PhoneNumber + ");";
                }
                Context.Response.Output.WriteLine(response);
            }
            Context.Response.End();
            return string.Empty;
        }

    }
}
