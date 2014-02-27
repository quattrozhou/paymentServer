using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace PaymentServer
{
    class paymentServer_email
    {
        public static void emailSignupHandler(UserProfile newProfile, string type)
        {
            if (type.Equals("AccountCreated"))
            {
                string head = "Shop. Save. Pay. With your phone. Your sign up has been confirmed.";
                string body = "Hello "+newProfile.username+",\n    Congraduatulations! You have been sign up a new account with us.\n"+
                    "Now you are realy to send out your first payment.\n"+
                    "Please let us know if you got any problem."+
                    "Sincerely,\n\nNFC POS Development Team\n\n"+
                    "Please do not reply to this email. This mailbox is not monitored and you will not receive a response.\n"+
                    "This Email has been sent from the payment server in this project.";
                sendEmail(newProfile.email, head, body);
            }
        }

        public static void sendEmail(string destination, string emailSubject, string emailBody)
        {
            string emailUsername = "quattrozhou";
            string emailPassword = "woaihuangqiaoye";
            string emailServer = "smtp.gmail.com";
            string emailAddress = emailUsername + "@gmail.com";

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(emailServer);

                mail.From = new MailAddress(emailAddress);
                mail.To.Add(destination);
                mail.Subject = emailSubject;
                mail.Body = emailBody;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(emailUsername, emailPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
