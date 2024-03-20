using System;
using System.Net.Mail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace FacebookPosting.SMTP
{
    public class FacebookSMTPClient
    {
        private readonly string _email;
        private readonly string _password;

        public FacebookSMTPClient(string smtpEmail, string smtpPassword)
        {
            _email = smtpEmail;
            _password = smtpPassword;
            // Configure SmtpClient settings, e.g., credentials, host, port, etc.
            // _smtpClient.Credentials = new NetworkCredential("your_email@example.com", smtpPassword);
            // _smtpClient.Host = "smtp.yourmailprovider.com";
            // _smtpClient.Port = 587;
            // _smtpClient.EnableSsl = true;
        }

        public void DisplayLastMessagesReceivedIn7Days()
        {
            using var client = new ImapClient();
            client.Connect("imap.gmail.com", 993, true);

            client.Authenticate(_email, _password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var today = DateTime.Now.Date;
            var uids = inbox.Search(SearchQuery.SentSince(today.AddHours(-1)));

            foreach (var uid in uids)
            {
                var message = inbox.GetMessage(uid);
                Console.WriteLine($"Subject: {message.Subject}, From: {message.From}, Date: {message.Date}");
            }

            client.Disconnect(true);
        }

        public string GetFacebookCodeGuarantee()
        {
            using var client = new ImapClient();
            
            ConnectToImapServer(client);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            while (true)
            {
                var today = DateTime.Now.Date;
                var uids = inbox.Search(SearchQuery.SentSince(today.AddMinutes(-20)));

                foreach (var uid in uids)
                {
                    var message = inbox.GetMessage(uid);

                    if (IsFacebookSecurityCodeEmail(message))
                    {
                        Console.WriteLine($"Subject: {message.Subject}, From: {message.From}, Date: {message.Date}");
                        client.Disconnect(true);
                        return GetSubjectCode(message);
                    }

                    Console.WriteLine($"Subject: {message.Subject}, From: {message.From}, Date: {message.Date}");
                }

                Thread.Sleep(5000);
            }
        }

        private void ConnectToImapServer(ImapClient client)
        {
            client.Connect("imap.gmail.com", 993, true);
            client.Authenticate(_email, _password);
        }

        private static bool IsFacebookSecurityCodeEmail(MimeMessage message)
        {
            return message.From.ToString().Contains("security@facebookmail.com") &&
                message.Subject.Take(6).All(char.IsDigit);
        }

        private static string GetSubjectCode(MimeMessage message)
        {
            return new string(message.Subject.Take(6).ToArray());
        }

    }
}
