namespace CompetitiveGamingApp.Email;

using MailKit;

using System;

using MailKit.Net.Smtp;
using MimeKit;


public class Email {
    public static void SendEmail(string senderEmail, string recipientEmail, string subject, string body) {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress("Sender Name", senderEmail));
        email.To.Add(new MailboxAddress("Receiver Name", recipientEmail));

        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { 
            Text = body
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);

            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}