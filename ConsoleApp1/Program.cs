using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;
using ConsoleApp1.Properties;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;


namespace ConsoleApp1
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            DataSet ds = SqlHelper.ExecuteDataset(Settings.Default.IssueTrackerConnectionString, CommandType.StoredProcedure, "GetUsers", new SqlParameter("@InternalOnly", true), new SqlParameter("@ActiveOnly", true));
            //create log file and write start log if doesnt exist already
            try
            {
                LogMessage(DateTime.Now.ToString(Settings.Default.DateTimeFormat), Settings.Default.ProgramStart, Settings.Default.ProgramStartAndEnd);
                CreateTempDirs(args);
            }
            catch (IOException)
            {
                LogMessage(DateTime.Now.ToString(Settings.Default.DateTimeFormat), Settings.Default.ProgramStart, Settings.Default.ProgramStartAndEnd);
                await SendEmail(Settings.Default.Email, Settings.Default.Subject, DateTime.Now.ToString(Settings.Default.DateTimeFormat) + Settings.Default.Colon + Settings.Default.Error);
            }
            finally
            {
                LogMessage(DateTime.Now.ToString(Settings.Default.DateTimeFormat), Settings.Default.ProgramStart, Settings.Default.ProgramStartAndEnd);
            }
        }

        //create temp dir
        private static void CreateTempDirs(string[] args)
        {
            if (!Directory.Exists(Settings.Default.TempPath))
            {
                Directory.CreateDirectory(Settings.Default.TempPath);
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(Settings.Default.TempPath, Settings.Default.IssueIDColumn)))
            {
                foreach (string arg in args)
                {
                    outputFile.WriteLine(arg);
                }
            }
            //create tempout directory and copy file
            if (!Directory.Exists(Settings.Default.TempOutPath))
            {
                Directory.CreateDirectory(Settings.Default.TempOutPath);
            }
            File.Copy(Path.Combine(Settings.Default.TempPath, Settings.Default.IssueIDColumn),
        Path.Combine(Settings.Default.TempOutPath, Settings.Default.IssueIDColumn), Settings.Default.OverwriteEvenIfFileExists);
        }

        //helper method to make log messages
        private static void LogMessage(string dateTime, string message, int logLevel)
        {
            if(logLevel >= Settings.Default.LogLevel)
            {
                using (StreamWriter streamWriter = new StreamWriter(Settings.Default.LogPath, true))
                {
                    {
                        streamWriter.WriteLine(DateTime.Now.ToString(Settings.Default.DateTimeFormat) + Settings.Default.Colon + message);
                    }
                }
            }
            Console.WriteLine(dateTime + Settings.Default.Colon + message);
        }

        //helper method to send the email
        private static async Task SendEmail(string emailAddress, string Subject, string EmailMessage)
        {
            SendEmailArgs sendEmailArgs = new SendEmailArgs(emailAddress, Subject, EmailMessage);
            EmailHelper emailHelper = new EmailHelper();
            await emailHelper.SendEmail(sendEmailArgs);
        }
        private static async Task IssueMaker()
        {
            PeriodicIssueMaker issueMaker = new PeriodicIssueMaker();
            issueMaker.createIssue();
        }
    }
}





