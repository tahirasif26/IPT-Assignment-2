using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace k181169_Q1//1132
{
    public partial class Service1 : ServiceBase
    {
        Timer time;

        public Service1()
        {
            InitializeComponent();
            time = new Timer();
            time.Interval = 15*60000;
            time.Elapsed += new ElapsedEventHandler(ReadJSON);
        }

        public class EmailReader
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string MessageBody { get; set; }

        }

        public void OnDebug()
        {
            OnStart(null);
        }

        public void ReadJSON(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string[] dirs = Directory.GetFiles(@"D:\Assignment2\k181169_Q1\Files", "*.json", SearchOption.AllDirectories); ;
                foreach (string dir in dirs)
                {
                    using (StreamReader file = File.OpenText(dir))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        EmailReader email = (EmailReader)serializer.Deserialize(file, typeof(EmailReader));
                        SMTPSend(email);
                    }
                    File.Delete(dir);
                }
            }
            catch (Exception ex)
            {
                string errorfile = ConfigurationManager.AppSettings["errorpath"];
                TextWriter text = new StreamWriter(errorfile);
                text.WriteLine("Something wrong in reading files." + ex);
                text.Close();
            }
            
          
        }

        public void SMTPSend(EmailReader email)
        {
            SmtpClient client = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential()
                {
                    UserName = "ta0327364@gmail.com",
                    Password = "pass123//"
                }
            };

            MailAddress From = new MailAddress("ta0327364@gmail.com", "Tahir");
            MailAddress ToEmail = new MailAddress(email.To,"");
            MailMessage Mail = new MailMessage()
            {
                From = From,
                Subject = email.Subject,
                Body = email.MessageBody,
            };
            Mail.To.Add(ToEmail);
            client.Send(Mail);

        }
        
        protected override void OnStart(string[] args)
        {
            time.Enabled = true;
            //ReadJSON(null, null);
        }

        protected override void OnStop()
        {
            time.Enabled = false;
        }

    }
}
