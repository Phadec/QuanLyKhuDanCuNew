using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Helpers
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendLoginNotificationAsync(string email, string userName, DateTime loginTime, string ipAddress);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["Sender"], emailSettings["SenderName"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                using (var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"])))
                {
                    client.EnableSsl = bool.Parse(emailSettings["EnableSsl"]);
                    client.Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]);
                    
                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw it to prevent login failures
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        public async Task SendLoginNotificationAsync(string email, string userName, DateTime loginTime, string ipAddress)
        {
            string subject = "Thông báo đăng nhập thành công";
            string message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 12px; border-radius: 0 0 5px 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Thông báo đăng nhập thành công</h2>
                        </div>
                        <div class='content'>
                            <p>Xin chào <strong>{userName}</strong>,</p>
                            <p>Tài khoản của bạn vừa được đăng nhập thành công vào hệ thống Quản lý Khu Dân Cư.</p>
                            <p><strong>Thời gian:</strong> {loginTime.ToString("dd/MM/yyyy HH:mm:ss")}</p>
                            <p><strong>Địa chỉ IP:</strong> {ipAddress}</p>
                            <p>Nếu đây không phải là bạn, vui lòng thay đổi mật khẩu ngay lập tức và liên hệ với quản trị viên.</p>
                            <p>Trân trọng,<br>Ban quản lý Khu Dân Cư</p>
                        </div>
                        <div class='footer'>
                            <p>Đây là email tự động, vui lòng không trả lời email này.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, message);
        }
    }
}