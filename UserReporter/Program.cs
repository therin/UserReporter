using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Diagnostics;
using System.IO;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Cassia;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;



namespace UserReporter
{
    class Program
    {
        static void Main(string[] args)
        {

      
        
            if (args.Length == 0)
            {
                 DateTime date = DateTime.Now;
                    // get time
                    string longDate = DateTime.Now.ToString();

                    // get user name
                    string userName = Convert.ToString(WindowsIdentity.GetCurrent().Name);
 

                    // get RDP client local computer name
                    string localName = "";
                    TerminalServicesManager manager = new TerminalServicesManager();
                    localName = manager.CurrentSession.ClientName;
              


                    // get user description 
                   
                        PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                        UserPrincipal user = UserPrincipal.Current;
                        string Description = user.Description;
                        string DescriptionName = "None";
                        string DescriptionDate = "None";
                        Regex regex = new Regex(@"[0-9]{1,2}-[0-9]{1,2}-[0-9]{2}");
                        Match match = regex.Match(Description);
                        if (match.Success)
                        {
                            DescriptionDate = match.Value;
                        }
                        DescriptionName = Regex.Replace(Description, "[0-9]{1,2}-[0-9]{1,2}-[0-9]{2}", "", RegexOptions.IgnoreCase);

                
                    try
                    {
                        //Add new entry to CSV file
                        string filePath = "\\\\demo\\Reports\\demo_usage_report1.csv";
                        AddActiveSessionEntry(longDate, localName, DescriptionName, DescriptionDate,userName);
                        var csv = new StringBuilder();
                        var newLine = string.Format("{0},{1},{2},{3},{4},{5}{6}", "Login", longDate, userName, localName, DescriptionName, DescriptionDate, Environment.NewLine);
                        csv.Append(newLine);
                        File.AppendAllText(filePath, csv.ToString());
                        localName = null;
                        manager = null;
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine(ex.Message + " CSV");
                    }
                }

            else if ((args[0] == "logout"))
            {
                 DateTime date = DateTime.Now;
                    // get time
                    string longDate = DateTime.Now.ToString();

                    // get user name
                    string userName = Convert.ToString(WindowsIdentity.GetCurrent().Name);
 

                    // get RDP client local computer name
                    string localName = "";
                    TerminalServicesManager manager = new TerminalServicesManager();
                    localName = manager.CurrentSession.ClientName;
              

                    // get user description 
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                    UserPrincipal user = UserPrincipal.Current;
                    string Description = user.Description;
                    string DescriptionName = "None";
                    string DescriptionDate = "None";
                    Regex regex = new Regex(@"[0-9]{1,2}-[0-9]{1,2}-[0-9]{2}");
                    Match match = regex.Match(Description);
                    if (match.Success)
                    {
                        DescriptionDate = match.Value;
                    }
                    DescriptionName = Regex.Replace(Description, "[0-9]{1,2}-[0-9]{1,2}-[0-9]{2}", "", RegexOptions.IgnoreCase);



                    try
                    {
                        //Add new entry to CSV file
                        string filePath = "\\\\demo\\Reports\\demo_usage_report1.csv";
                        var csv = new StringBuilder();
                        checkForSessions(longDate, localName, DescriptionName, DescriptionDate);
                        var newLine = string.Format("{0},{1},{2},{3},{4},{5}{6}", "Logout", longDate, userName, localName, DescriptionName, DescriptionDate, Environment.NewLine);
                        csv.Append(newLine);
                        File.AppendAllText(filePath, csv.ToString());
                        localName = null;
                        manager = null;

                    }
                    catch (Exception ex)
                    {
                        
                        //Console.WriteLine(ex.Message + " CSV");
                    }
            }

            else if ((args[0] == "email"))
	{
        // If "email" argument is specified email the report move it to archive folder and exit with code 0    
                    sendEMailThroughGmail();
                    archiveReports();
                    Environment.Exit(0);
	}
            else if (args[0] == "print")
            {
                printTable();
                
            }
        }
          
  
    
        // Send email with report
        static void sendEMailThroughGmail()
        {
            try
            {
                //Mail Message
                MailMessage mM = new MailMessage();
                //Mail Address
                mM.From = new MailAddress("test@test.biz");
                //receiver email id
                mM.To.Add("test@test.biz");
                mM.To.Add("test@test.biz");
                mM.To.Add("test@test.biz");
                //subject of the email
                mM.Subject = "Demo Users activity report";
                //deciding for the attachment
                mM.Attachments.Add(new Attachment(@"\\\\demo\\Reports\\demo_usage_report1.csv"));
                //add the body of the email
                mM.Body = "Please find the report attached";
                mM.IsBodyHtml = true;
                //SMTP client
                SmtpClient sC = new SmtpClient("smtp.gmail.com");
                //port number for Gmail mail
                sC.Port = 587;
                //credentials to login in to Gmail account
                sC.Credentials = new NetworkCredential("test@test.biz", "123");
                //enabled SSL
                sC.EnableSsl = true;
                //Send an email
                sC.Send(mM);
                //Clean up attachments
                foreach (Attachment attachment in mM.Attachments)
                {
                    attachment.Dispose();
                }
            }//end of try block
            catch (Exception ex) { Console.WriteLine(ex.Message + " email"); }
        }


    static void checkForSessions(string logouttime, string computer, string company, string expiry)
    {
        MainDataContext myDB = new MainDataContext();
        
        var selectAll = from session in myDB.ActiveSessions
                        select session;

        bool trigger = false;
     
        foreach (var some in selectAll)
        {
            
            some.companyName = some.companyName.Replace(" ", "");
            company = company.Replace(" ", "");
            DateTime dt = some.sessionStart.Value;
            DateTime expiryT = some.expiryDate.Value;
            if (some.companyName == company && trigger == false)
            {
                AddUserSessionEntry(some.userName, some.computerName, some.companyName, Convert.ToString(expiryT), calcSessionTime(dt, logouttime), DateTime.Now);
               DeleteRow(some.companyName.Replace(" ", ""));
               trigger = true;

            }
            if (some.companyName == company && trigger == true)
            {
                DeleteRow(some.companyName.Replace(" ", ""));
            }
        }
    }

    static string calcSessionTime(DateTime startTime, string endTime)
    {
            DateTime startingTime = Convert.ToDateTime(startTime);
            DateTime endingtime = Convert.ToDateTime(endTime);
            TimeSpan duration = startingTime - endingtime;
            return Convert.ToString(duration.Duration());
            
        }

     static void DeleteRow(string companyname)
    {
        MainDataContext myDB = new MainDataContext();

        var sessions = from a in myDB.ActiveSessions
                      where a.companyName.Replace(" ", "") == companyname
                      select a;

        myDB.ActiveSessions.DeleteAllOnSubmit(sessions);
        myDB.SubmitChanges();
      

    }

     static void AddUserSessionEntry(string username, string computername, string companyname, string expirydate, string sessionduration, DateTime currentTime)
     {
         MainDataContext myDB = new MainDataContext();
         UserSession acs = new UserSession
         {
             userName = username.TrimEnd(),
             computerName = computername.TrimEnd(),
             companyName = companyname,
             expiryDate = DateTime.Parse(expirydate),
             sessionDuration = sessionduration,
             addedDate = currentTime
          };

         myDB.UserSessions.InsertOnSubmit(acs);

         if (acs != null)
         {
             //Console.WriteLine("new user session item inserted:  {0}, {1}, {2}, {3}, {4}, {5}, {6} ",
             //                   acs.Id, acs.userName, acs.computerName, acs.companyName, acs.expiryDate, acs.sessionDuration, acs.addedDate);
         }
         // Submit the change to the database. 
         try
         {
             myDB.SubmitChanges();
         }
         catch (Exception e)
         {
             Console.WriteLine(e);
         }
     }

    static void AddActiveSessionEntry(string logintime, string computer, string company, string expiry, string username)
    {
        
        
        MainDataContext myDB = new MainDataContext();
        ActiveSession acs = new ActiveSession
            {
               sessionStart = DateTime.Parse(logintime),
               computerName = computer.TrimEnd(),
               companyName = company,
               expiryDate = DateTime.Parse(expiry),
               userName = username.TrimEnd()
            };

        myDB.ActiveSessions.InsertOnSubmit(acs);

        if (acs != null)
        {
           // Console.WriteLine("new item inserted:  {0}, {1}, {2}, {3} ",
           //                 acs.sessionStart, acs.computerName, acs.companyName, acs.expiryDate);
        }
        // Submit the change to the database. 
        try
        {
            myDB.SubmitChanges();
        }
            catch (Exception e)
        {
            Console.WriteLine(e);
        }
}
    static void printTable()
    {
        
        MainDataContext myDB = new MainDataContext();

        var selectAll = from session in myDB.UserSessions
                        select session;
        
        foreach (var some in selectAll)
        {
            Console.WriteLine(" {0}, {1}, {2}, {3}, {4}, {5} ",
                                some.Id, some.userName, some.computerName, some.companyName, some.expiryDate, some.sessionDuration);
        }
    }

        // Rename old report file and move it to another folder
        static void archiveReports()
        {
            try
            {
                string fileUNQ = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString();
                string reportFileName = "Report_" + fileUNQ + ".csv";

                string oldfile = "\\\\DEMO\\Reports\\demo_usage_report1.csv";
                string newfile = "\\\\DEMO\\Reports\\Archive\\" + reportFileName ;
                System.IO.File.Move(oldfile, newfile);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message + " Move");
            }

        }

    }
}

     
 


           

