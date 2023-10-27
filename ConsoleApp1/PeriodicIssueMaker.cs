using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Properties;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using System.Data;

namespace ConsoleApp1
{
    internal class PeriodicIssueMaker
    {
        private static async Task Main(string[] args)
        {
            using (StreamReader read = new StreamReader(Settings.Default.FilePath))
            {
                while (read.ReadLine() != null)
                {
                    string inputLine = read.ReadLine();
                    if(!(inputLine.Substring(0,1) == Settings.Default.CommentString) && !string.IsNullOrWhiteSpace(inputLine))
                    {
                        string[] inputArr = inputLine.Split(Settings.Default.StringSplitArg);
                        string dueDayInput = inputArr[12];
                        string dueDay = dueDayInput.Substring(inputArr[12].IndexOf(" ") + 1).ToLower();
                        bool first = false;
                        string prefix = dueDayInput.Substring(0, dueDayInput.IndexOf(" "));
                        DateTime dueDate;
                        if (prefix == "first")
                        {
                            first = true;
                        }
                        if (first)
                        {
                            dueDate = FirstDay(DateTime.Now.Year, DateTime.Now.Month, dueDay);

                        }
                        else
                        {
                            dueDate = LastDay(DateTime.Now.Year, DateTime.Now.Month, dueDay);
                        }
                        //bot starts to run
                        try
                        {   
                            DataSet ds = CallSproc(inputArr, dueDate);
                            await SendEmail(inputArr[15], Settings.Default.RunMessage, Settings.Default.RunMessage);
                        }
                        //bot failed to insert issue
                        catch (SqlException ex)
                        {
                            await SendEmail(inputArr[15], Settings.Default.ErrorSubject, inputLine + Settings.Default.NewLine + inputArr[4]);
                        }
                        catch (Exception ex)
                        {
                            await SendEmail(inputArr[15], Settings.Default.ErrorSubject, ex + Settings.Default.NewLine + inputLine + Settings.Default.NewLine + inputArr[4]);
                        }
                        finally
                        {
                            DataSet ds = CallSproc(inputArr, dueDate);
                            int issueNumber = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                            await SendEmail(inputArr[15], Settings.Default.Subject + issueNumber, read.ReadLine() + Settings.Default.TestingConnection +
                                issueNumber);
                        }
                    } 
                }
            }
        }

        //helper method to call the stored procedure
        private static DataSet CallSproc(string[] input, DateTime dueDate)
        {
            DataSet ds = SqlHelper.ExecuteDataset(Settings.Default.IssueTrackerConnectionString, CommandType.StoredProcedure, "InsertBugAndComment",
                                new SqlParameter("@bg_short_desc", input[4]), 
                                new SqlParameter("@bg_reported_user", input[5]), 
                                new SqlParameter("@bg_priority", input[6]),
                                new SqlParameter("bg_category", input[7]),
                                new SqlParameter("@bg_project", input[8]),
                                new SqlParameter("@bg_status", input[9]),
                                new SqlParameter("bg_assigned_to_user", input[10]),
                                new SqlParameter("bg_org", input[11]),
                                new SqlParameter("Date_Due", dueDate),
                                new SqlParameter("bg_tags", input[13]),
                                new SqlParameter("@Comment", input[14]));
            return ds;
        }

        //helper method to send the email
        private static async Task SendEmail(string emailAddress, string Subject, string EmailMessage)
        {
            SendEmailArgs sendEmailArgs = new SendEmailArgs(emailAddress, Subject, EmailMessage);
            EmailHelper emailHelper = new EmailHelper();
            await emailHelper.SendEmail(sendEmailArgs);
        }
        //helper method to compare the input date to current date and return a DateTime based off input string
        public static DateTime LastDay(int year, int month, string day)
        {
            DateTime dt;
            if (month < 12)
                dt = new DateTime(year, month + 1, 1);
            else
                dt = new DateTime(year + 1, 1, 1);
            dt = dt.AddDays(-1);
            switch (day)
            {
                case "monday":
                    while (dt.DayOfWeek != DayOfWeek.Monday) dt = dt.AddDays(-1);
                    return dt;
                case "tuesday":
                    while (dt.DayOfWeek != DayOfWeek.Tuesday) dt = dt.AddDays(-1);
                    return dt;
                case "wednesday":
                    while (dt.DayOfWeek != DayOfWeek.Wednesday) dt = dt.AddDays(-1);
                    return dt;
                case "thursday":
                    while (dt.DayOfWeek != DayOfWeek.Thursday) dt = dt.AddDays(-1);
                    return dt;
                case "friday":
                    while (dt.DayOfWeek != DayOfWeek.Friday) dt = dt.AddDays(-1);
                    return dt;
                case "saturday":
                    while (dt.DayOfWeek != DayOfWeek.Saturday) dt = dt.AddDays(-1);
                    return dt;
                case "sunday":
                    while (dt.DayOfWeek != DayOfWeek.Sunday) dt = dt.AddDays(-1);
                    return dt;
            }
            return dt;
        }
        public static DateTime FirstDay(int year, int month, string day)
        {
            DateTime dt;
            if (month < 12)
                dt = new DateTime(year, month + 1, 1);
            else
                dt = new DateTime(year + 1, 1, 1);
            dt = dt.AddDays(-1);
            switch (day)
            {
                case "monday":
                    while (dt.DayOfWeek != DayOfWeek.Monday) dt = dt.AddDays(1);
                    return dt;
                case "tuesday":
                    while (dt.DayOfWeek != DayOfWeek.Tuesday) dt = dt.AddDays(1);
                    return dt;
                case "wednesday":
                    while (dt.DayOfWeek != DayOfWeek.Wednesday) dt = dt.AddDays(1);
                    return dt;
                case "thursday":
                    while (dt.DayOfWeek != DayOfWeek.Thursday) dt = dt.AddDays(1);
                    return dt;
                case "friday":
                    while (dt.DayOfWeek != DayOfWeek.Friday) dt = dt.AddDays(1);
                    return dt;
                case "saturday":
                    while (dt.DayOfWeek != DayOfWeek.Saturday) dt = dt.AddDays(1);
                    return dt;
                case "sunday":
                    while (dt.DayOfWeek != DayOfWeek.Sunday) dt = dt.AddDays(1);
                    return dt;
            }
            return dt;
        }
    }
}

