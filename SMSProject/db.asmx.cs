using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            using (SqlConnection sqlCon = new SqlConnection(@"Data Source=DESKTOP-N3SBF80\SQLEXPRESS;Initial Catalog=DB_A4A060_cs;Integrated Security=True"))
            {
                sqlCon.Open();
                using (SqlDataAdapter sqlDA = new SqlDataAdapter("select * from Z_AlertLogs", sqlCon))
                {
                    DataTable outputTable = new DataTable();
                    sqlDA.Fill(outputTable);
                    foreach (DataRow row in outputTable.Rows)
                    {
                        Context.Response.Output.Write(row["email"] + "\n");
                    }
                }
                sqlCon.Close();
            }
            Context.Response.End();
            return string.Empty;
        }
    }
}
