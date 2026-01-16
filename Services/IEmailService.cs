using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendOrderConfirmationAsync(string toEmail, string orderCode, string customerName);
        Task SendOrderStatusChangeEmailAsync(Order order, OrderStatus oldStatus, OrderStatus newStatus); // ✅ FIX
        Task SendOrderCancellationEmailAsync(Order order, string reason); // ✅ FIX
    }
}