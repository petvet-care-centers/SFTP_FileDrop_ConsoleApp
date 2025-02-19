using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using SFTP_FileDrop2.Classes;


namespace SFTP_FileDrop_WorkerSerivce.Classes
{
    class Email
    {
        private readonly IConfiguration _configuration;

        public Email(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string processName, string subject, string message)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            string? smtpTo = configuration["EmailSettings:Dev_Email"];
            string? smtpFrom = configuration["EmailSettings:smtpFrom"];
            string? smtpHost = configuration["EmailSettings:smtpHost"];
            string? smtpPort = configuration["EmailSettings:smtpPort"];
            string? smtpUserName = configuration["EmailSettings:smtpUser"];
            string? smtpPassword = configuration["EmailSettings:smtpPwd"];
            string? testEmailTo = configuration["EmailSettings:Test_Email"];
            string? smtpIsSSL = configuration["EmailSettings:smtpIsSSL"];
            string? smtpReplyTo = configuration["EmailSettings:smtpReplyTo"];

            try
            {
                MailMessage mail = new MailMessage();
#if DEBUG
                mail.To.Add(testEmailTo);
#else
                mail.To.Add(smtpTo);
#endif
                mail.From = new MailAddress(smtpFrom);
                mail.IsBodyHtml = true;
                mail.ReplyToList.Add(smtpReplyTo);
                mail.Subject = subject;
                mail.Body = message;

                SmtpClient smtp = new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Host = smtpHost,
                    EnableSsl = Convert.ToBoolean(smtpIsSSL),
                    Port = Convert.ToInt32(smtpPort)

                };

                smtp.Host = smtpHost;
                smtp.Credentials = new System.Net.NetworkCredential(
                    smtpUserName,
                    smtpPassword
                    );

                smtp.Send(mail);

            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }
        }

    }
}
