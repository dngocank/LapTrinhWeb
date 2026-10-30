using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Services
{
    public interface INotificationService
    {
        bool SendHomeWarrantyConfirmation(WarrantyTicketModel ticket, string email, string phone, out string emailResult, out string smsResult);
        bool SendWarrantyStatusUpdateEmail(WarrantyTicketModel ticket, string oldStatus, string newStatus, out string logMessage);
        List<WarrantyNotificationModel> GetNotificationsByTicketId(int ticketId);
        List<WarrantyNotificationModel> GetNotificationsByCustomerId(int customerId);
    }
}
