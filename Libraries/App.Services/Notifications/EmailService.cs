using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace App.Services.Notifications
{
	public class EmailService : IEmailService
	{
		public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Suptech Co", "info@suptech.online"));
			message.To.Add(MailboxAddress.Parse(to));
			message.Subject = subject;

			var builder = new BodyBuilder();
			if (isHtml)
				builder.HtmlBody = body;
			else
				builder.TextBody = body;

			message.Body = builder.ToMessageBody();

			using (var smtp = new SmtpClient())
			{
				smtp.Timeout = 30000;
				smtp.LocalDomain = "suptech.online";

				try
				{
					// ✅ استخدم SSL مباشر مع 465
					await smtp.ConnectAsync("smtp.hostinger.com", 465, SecureSocketOptions.SslOnConnect);

					await smtp.AuthenticateAsync("info@suptech.online", "$Amman@123456");

					await smtp.SendAsync(message);
				}
				catch (Exception ex)
				{
					throw new Exception($"Email sending failed: {ex.Message}", ex);
				}
				finally
				{
					if (smtp.IsConnected)
						await smtp.DisconnectAsync(true);
				}
			}
		}
	}
}
