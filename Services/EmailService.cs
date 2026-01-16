using FoodOrderingWeb.Models.Entities;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace FoodOrderingWeb.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["EmailSettings:FromName"] ?? "Food Ordering System";

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email configuration not set. Skipping email send.");
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? "noreply@foodorder.com", fromName ?? "Food Ordering"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                // Không throw exception để không ảnh hưởng flow chính
            }
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string orderCode, string customerName)
        {
            var subject = $"Xác nhận đơn hàng {orderCode}";
            var htmlMessage = $@"
                <h2>Cảm ơn bạn đã đặt hàng!</h2>
                <p>Xin chào <strong>{customerName}</strong>,</p>
                <p>Chúng tôi đã nhận được đơn hàng của bạn với mã: <strong>{orderCode}</strong></p>
                <p>Đơn hàng của bạn đang được xử lý. Chúng tôi sẽ liên hệ với bạn sớm nhất.</p>
                <p>Cảm ơn bạn đã tin tưởng!</p>
                <hr>
                <p><small>Email này được gửi tự động, vui lòng không reply.</small></p>
            ";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendOrderStatusChangeEmailAsync(Order order, OrderStatus oldStatus, OrderStatus newStatus)
        {
            if (string.IsNullOrEmpty(order.Email))
                return;

            var oldStatusText = GetStatusText(oldStatus);
            var newStatusText = GetStatusText(newStatus);

            var subject = $"Cập nhật đơn hàng {order.OrderCode}";
            var htmlMessage = $@"
                <h2>Cập nhật trạng thái đơn hàng</h2>
                <p>Xin chào <strong>{order.CustomerName}</strong>,</p>
                <p>Đơn hàng <strong>{order.OrderCode}</strong> của bạn đã được cập nhật.</p>
                <p>Trạng thái: <strong>{oldStatusText}</strong> → <strong>{newStatusText}</strong></p>
                <p>Cảm ơn bạn!</p>
                <hr>
                <p><small>Email này được gửi tự động, vui lòng không reply.</small></p>
            ";

            await SendEmailAsync(order.Email, subject, htmlMessage);
        }

        public async Task SendOrderCancellationEmailAsync(Order order, string reason)
        {
            if (string.IsNullOrEmpty(order.Email))
                return;

            var subject = $"Đơn hàng {order.OrderCode} đã bị hủy";
            var htmlMessage = $@"
                <h2>Thông báo hủy đơn hàng</h2>
                <p>Xin chào <strong>{order.CustomerName}</strong>,</p>
                <p>Đơn hàng <strong>{order.OrderCode}</strong> của bạn đã bị hủy.</p>
                <p>Lý do: <em>{reason}</em></p>
                <p>Nếu có thắc mắc, vui lòng liên hệ với chúng tôi.</p>
                <p>Xin lỗi vì sự bất tiện này!</p>
                <hr>
                <p><small>Email này được gửi tự động, vui lòng không reply.</small></p>
            ";

            await SendEmailAsync(order.Email, subject, htmlMessage);
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xác nhận",
                OrderStatus.Processing => "Đang chuẩn bị",
                OrderStatus.Shipping => "Đang giao",
                OrderStatus.Completed => "Hoàn thành",
                OrderStatus.Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }
    }
}