using ContactPro.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Security.Cryptography.X509Certificates;

namespace ContactPro.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");  //email address sending the sender.

            MimeMessage newEmail = new(); //handles email construction.

            newEmail.Sender = MailboxAddress.Parse(emailSender); //set sender as emailSender.

            foreach (var emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            } //add the option of multiple people be sent an email. Semicolon is typical in emails.

            newEmail.Subject = subject;
            BodyBuilder emailBody = new();

            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            //Login Point for the GMail Account.
            using SmtpClient smtpClient = new();

            try
            {
                var host = _mailSettings.MailHost ?? Environment.GetEnvironmentVariable("MailHost");
                var port = _mailSettings.MailPort != 0 ? _mailSettings.MailPort : int.Parse(Environment.GetEnvironmentVariable("MailPort")!); //error showed originally because port is an int by nature. Has to be converted.
                var password = _mailSettings.MailPassword ?? Environment.GetEnvironmentVariable("MailPassword");

                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);
                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
            }
            catch(Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

    }
}
