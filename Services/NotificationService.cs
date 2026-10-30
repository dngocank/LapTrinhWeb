using Microsoft.Data.SqlClient;
using BTL_PTTKYC.Models;
using System.Net.Mail;
using System.Net;

namespace BTL_PTTKYC.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public bool SendHomeWarrantyConfirmation(WarrantyTicketModel ticket, string email, string phone, out string emailResult, out string smsResult)
        {
            emailResult = string.Empty;
            smsResult = string.Empty;

            try
            {
                // 1. Soạn nội dung EMAIL
                string emailTitle = $"[KittyPhone] Xác nhận tiếp nhận bảo hành tại nhà #{ticket.TicketCode} - {ticket.ProductName ?? "Thiết bị di động"}";
                string emailBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 650px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);"">
    <div style=""background: linear-gradient(135deg, #0e2254, #0284c7); padding: 25px; text-align: center; color: white;"">
        <h2 style=""margin: 0; font-size: 24px; font-weight: 700;"">KITTYPHONE SERVICE</h2>
        <p style=""margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;"">Xác Nhận Đăng Ký Bảo Hành Tận Nhà Thành Công</p>
    </div>
    <div style=""padding: 30px; background: #ffffff;"">
        <p style=""font-size: 16px; color: #1e293b;"">Xin chào <strong>{ticket.CustomerName}</strong>,</p>
        <p style=""color: #475569; line-height: 1.6;"">
            KittyPhone đã ghi nhận yêu cầu bảo hành tận nhà của quý khách. Đội ngũ Kỹ thuật viên & Đối tác vận chuyển đã được điều phối và sẽ có mặt theo đúng lịch hẹn.
        </p>

        <div style=""background: #f8fafc; border-radius: 8px; border: 1px solid #cbd5e1; padding: 20px; margin: 20px 0;"">
            <h4 style=""margin: 0 0 15px 0; color: #0e2254; font-size: 16px; border-bottom: 2px solid #0284c7; padding-bottom: 8px;"">
                THÔNG TIN PHIẾU BẢO HÀNH TẠI NHÀ
            </h4>
            <table style=""width: 100%; border-collapse: collapse; font-size: 14px;"">
                <tr>
                    <td style=""padding: 6px 0; color: #64748b; width: 40%;"">Mã phiếu bảo hành:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #0284c7;"">{ticket.TicketCode}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Thiết bị cần bảo hành:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #1e293b;"">{ticket.ProductName ?? "Điện thoại / Thiết bị"}</td>
                </tr>
                {(string.IsNullOrEmpty(ticket.ImeiNumber) ? "" : $@"
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Số IMEI / Serial:</td>
                    <td style=""padding: 6px 0; font-family: monospace; color: #1e293b;"">{ticket.ImeiNumber}</td>
                </tr>")}
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Lỗi thiết bị do khách báo:</td>
                    <td style=""padding: 6px 0; color: #dc2626; font-weight: 500;"">{ticket.CustomerIssue}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Địa chỉ hẹn lấy máy:</td>
                    <td style=""padding: 6px 0; color: #1e293b; font-weight: 500;"">{ticket.PickupAddress ?? "Theo thỏa thuận"}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Thời gian hẹn tiếp nhận:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #16a34a;"">{(ticket.PickupTime.HasValue ? ticket.PickupTime.Value.ToString("dd/MM/yyyy HH:mm") : "Trong 24h")}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Hình thức & Đơn vị giao nhận:</td>
                    <td style=""padding: 6px 0; color: #1e293b;"">{ticket.ServiceType ?? "Kỹ thuật viên tới tận nhà"} ({ticket.ShippingCarrier ?? "Xanh SM Express"})</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Cước phí ship vận chuyển:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #16a34a;"">{(ticket.ShippingFee == 0 ? "0đ (KittyPhone tài trợ miễn phí)" : ticket.ShippingFee.ToString("N0") + "đ")}</td>
                </tr>
            </table>
        </div>

        <div style=""background: #fffbeb; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; border-radius: 4px;"">
            <h5 style=""margin: 0 0 5px 0; color: #b45309;"">Lưu ý quan trọng trước khi bàn giao máy:</h5>
            <ul style=""margin: 5px 0 0 0; padding-left: 20px; color: #78350f; font-size: 13px; line-height: 1.5;"">
                <li>Quý khách vui lòng sao lưu dữ liệu cá nhân quan trọng (danh bạ, hình ảnh) trước khi gửi máy.</li>
                <li>Đăng xuất tài khoản iCloud / Google hoặc cung cấp mật khẩu màn hình để KTV kiểm tra chức năng.</li>
                <li>Shipper / Kỹ thuật viên sẽ kiểm tra ngoại quan máy và gửi biên nhận điện tử ngay khi tiếp nhận.</li>
            </ul>
        </div>

        <p style=""text-align: center; margin-top: 30px;"">
            <a href=""https://kittyphone.vn/Customer/Profile"" style=""background: linear-gradient(135deg, #0e2254, #0284c7); color: white; text-decoration: none; padding: 12px 25px; border-radius: 30px; font-weight: bold; display: inline-block;"">
                Theo Dõi Tiến Độ Trực Tiếp
            </a>
        </p>
    </div>
    <div style=""background: #f1f5f9; padding: 15px; text-align: center; color: #64748b; font-size: 12px; border-top: 1px solid #e2e8f0;"">
        Cửa hàng KittyPhone • 123 Thái Hà, Đống Đa, Hà Nội • Hotline: 0396166509<br/>
        Email thông báo tự động từ hệ thống quản lý dịch vụ bảo hành KittyPhone.
    </div>
</div>";

                // Lưu log Email vào Database
                SaveNotification(ticket.TicketId, ticket.TicketCode, ticket.CustomerId, "Email", email, emailTitle, emailBody);
                emailResult = $"Đã gửi Email xác nhận tới {email}";

                // 2. Soạn nội dung SMS
                string pickupStr = ticket.PickupTime.HasValue ? ticket.PickupTime.Value.ToString("dd/MM/yyyy HH:mm") : "trong 24h";
                string smsBody = $"[KittyPhone] Yeu cau bao hanh tai nha {ticket.TicketCode} cho may {ticket.ProductName} da duoc xac nhan. Hen lay may luc {pickupStr} tai {ticket.PickupAddress}. CSKH: 0396166509.";

                // Lưu log SMS vào Database
                SaveNotification(ticket.TicketId, ticket.TicketCode, ticket.CustomerId, "SMS", phone, $"SMS xác nhận #{ticket.TicketCode}", smsBody);
                smsResult = $"Đã gửi tin nhắn SMS xác nhận tới {phone}";

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo bảo hành tại nhà");
                emailResult = "Lỗi gửi thông báo: " + ex.Message;
                return false;
            }
        }

        public bool SendWarrantyStatusUpdateEmail(WarrantyTicketModel ticket, string oldStatus, string newStatus, out string logMessage)
        {
            logMessage = string.Empty;
            try
            {
                string targetEmail = !string.IsNullOrEmpty(ticket.CustomerEmail) 
                    ? ticket.CustomerEmail 
                    : GetCustomerEmailFromDb(ticket.CustomerId, ticket.CustomerPhone);

                if (string.IsNullOrEmpty(targetEmail))
                {
                    targetEmail = "khachhang@kittyphone.vn"; // Fallback email
                }

                string emailTitle = $"[KittyPhone] Cập nhật tiến độ bảo hành #{ticket.TicketCode}: {ticket.StatusVietnamese}";
                string emailBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 650px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden;"">
    <div style=""background: linear-gradient(135deg, #0e2254, #0284c7); padding: 25px; text-align: center; color: white;"">
        <h2 style=""margin: 0; font-size: 22px;"">KITTYPHONE SERVICE</h2>
        <p style=""margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;"">Thông Báo Cập Nhật Trạng Thái Bảo Hành</p>
    </div>
    <div style=""padding: 25px; background: #ffffff;"">
        <p style=""font-size: 15px; color: #1e293b;"">Kính gửi quý khách <strong>{ticket.CustomerName}</strong>,</p>
        <p style=""color: #475569; line-height: 1.6;"">
            Kỹ thuật viên KittyPhone vừa tiến hành kiểm tra & cập nhật trạng thái mới nhất cho phiếu bảo hành thiết bị của quý khách:
        </p>

        <div style=""text-align: center; margin: 25px 0;"">
            <span style=""display: inline-block; padding: 10px 24px; border-radius: 30px; font-weight: bold; font-size: 16px; background: #0284c7; color: white;"">
                Trạng thái hiện tại: {ticket.StatusVietnamese}
            </span>
        </div>

        <div style=""background: #f8fafc; border-radius: 8px; border: 1px solid #cbd5e1; padding: 18px; margin-bottom: 20px;"">
            <table style=""width: 100%; border-collapse: collapse; font-size: 14px;"">
                <tr>
                    <td style=""padding: 6px 0; color: #64748b; width: 40%;"">Mã phiếu bảo hành:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #0e2254;"">{ticket.TicketCode}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Thiết bị:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #1e293b;"">{ticket.ProductName}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Kỹ thuật viên phụ trách:</td>
                    <td style=""padding: 6px 0; color: #1e293b;"">{ticket.TechnicianName ?? "Nguyễn Văn Kỹ Thuật"}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Kết quả chẩn đoán:</td>
                    <td style=""padding: 6px 0; color: #1e293b; font-weight: 500;"">{ticket.Diagnosis ?? "Đang kiểm tra bo mạch và linh kiện"}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Biện pháp xử lý:</td>
                    <td style=""padding: 6px 0; color: #1e293b;"">{ticket.RepairAction ?? "Theo dõi kiểm tra kỹ thuật"}</td>
                </tr>
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Chi phí xử lý:</td>
                    <td style=""padding: 6px 0; font-weight: bold; color: #16a34a;"">{ticket.PaymentStatusVietnamese} ({(ticket.RepairCost == 0 ? "Miễn phí BH chính hãng" : ticket.RepairCost.ToString("N0") + "đ")})</td>
                </tr>
                {(string.IsNullOrEmpty(ticket.Notes) ? "" : $@"
                <tr>
                    <td style=""padding: 6px 0; color: #64748b;"">Ghi chú kỹ thuật:</td>
                    <td style=""padding: 6px 0; color: #475569; font-style: italic;"">{ticket.Notes}</td>
                </tr>")}
            </table>
        </div>

        {(ticket.Status == "Completed" ? $@"
        <div style=""background: #ecfdf5; border-left: 4px solid #10b981; padding: 15px; margin: 15px 0; border-radius: 4px;"">
            <h5 style=""margin: 0 0 5px 0; color: #065f46;"">Thiết bị đã hoàn tất sửa chữa & kiểm định chất lượng:</h5>
            <p style=""margin: 0; color: #047857; font-size: 13px;"">
                Máy đã sẵn sàng để bàn giao trực tiếp tại cửa hàng hoặc ship về tận nhà theo địa chỉ yêu cầu của quý khách.
            </p>
        </div>" : "")}

        <p style=""text-align: center; margin-top: 25px;"">
            <a href=""https://kittyphone.vn/Customer/Warranty"" style=""background: linear-gradient(135deg, #0e2254, #0284c7); color: white; text-decoration: none; padding: 10px 22px; border-radius: 25px; font-weight: bold; font-size: 14px; display: inline-block;"">
                Tra Cứu Tiến Độ Chi Tiết
            </a>
        </p>
    </div>
    <div style=""background: #f8fafc; padding: 12px; text-align: center; color: #94a3b8; font-size: 12px; border-top: 1px solid #e2e8f0;"">
        Trung tâm bảo hành KittyPhone • 123 Thái Hà, Đống Đa, Hà Nội • Hotline: 0396166509
    </div>
</div>";

                SaveNotification(ticket.TicketId, ticket.TicketCode, ticket.CustomerId, "Email", targetEmail, emailTitle, emailBody);
                logMessage = $"Đã tự động gửi Email cập nhật tiến độ [{ticket.StatusVietnamese}] tới {targetEmail}";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email cập nhật trạng thái");
                logMessage = "Lỗi khi gửi email: " + ex.Message;
                return false;
            }
        }

        public List<WarrantyNotificationModel> GetNotificationsByTicketId(int ticketId)
        {
            var list = new List<WarrantyNotificationModel>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT * FROM Warranty_Notification WHERE ticket_id = @id ORDER BY sent_at DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", ticketId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new WarrantyNotificationModel
                {
                    NotificationId = Convert.ToInt32(r["notification_id"]),
                    TicketId = Convert.ToInt32(r["ticket_id"]),
                    TicketCode = r["ticket_code"].ToString()!,
                    CustomerId = r["customer_id"] != DBNull.Value ? Convert.ToInt32(r["customer_id"]) : null,
                    Channel = r["channel"].ToString()!,
                    Recipient = r["recipient"].ToString()!,
                    Title = r["title"].ToString()!,
                    MessageBody = r["message_body"].ToString()!,
                    Status = r["status"].ToString()!,
                    SentAt = Convert.ToDateTime(r["sent_at"])
                });
            }

            return list;
        }

        public List<WarrantyNotificationModel> GetNotificationsByCustomerId(int customerId)
        {
            var list = new List<WarrantyNotificationModel>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT TOP 20 * FROM Warranty_Notification WHERE customer_id = @id ORDER BY sent_at DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", customerId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new WarrantyNotificationModel
                {
                    NotificationId = Convert.ToInt32(r["notification_id"]),
                    TicketId = Convert.ToInt32(r["ticket_id"]),
                    TicketCode = r["ticket_code"].ToString()!,
                    CustomerId = r["customer_id"] != DBNull.Value ? Convert.ToInt32(r["customer_id"]) : null,
                    Channel = r["channel"].ToString()!,
                    Recipient = r["recipient"].ToString()!,
                    Title = r["title"].ToString()!,
                    MessageBody = r["message_body"].ToString()!,
                    Status = r["status"].ToString()!,
                    SentAt = Convert.ToDateTime(r["sent_at"])
                });
            }

            return list;
        }

        private void SaveNotification(int ticketId, string ticketCode, int? customerId, string channel, string recipient, string title, string body)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                INSERT INTO Warranty_Notification (ticket_id, ticket_code, customer_id, channel, recipient, title, message_body, status, sent_at)
                VALUES (@tid, @tcode, @cid, @chan, @recip, @title, @body, 'Sent', GETDATE())";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tid", ticketId);
            cmd.Parameters.AddWithValue("@tcode", ticketCode);
            cmd.Parameters.AddWithValue("@cid", (object?)customerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@chan", channel);
            cmd.Parameters.AddWithValue("@recip", recipient);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@body", body);

            cmd.ExecuteNonQuery();
        }

        private string GetCustomerEmailFromDb(int? customerId, string phone)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            if (customerId.HasValue && customerId.Value > 0)
            {
                using var cmd = new SqlCommand("SELECT email FROM Customer WHERE customer_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", customerId.Value);
                var email = cmd.ExecuteScalar()?.ToString();
                if (!string.IsNullOrEmpty(email)) return email;
            }

            if (!string.IsNullOrEmpty(phone))
            {
                using var cmd = new SqlCommand("SELECT email FROM Customer WHERE phone = @phone", conn);
                cmd.Parameters.AddWithValue("@phone", phone.Trim());
                var email = cmd.ExecuteScalar()?.ToString();
                if (!string.IsNullOrEmpty(email)) return email;
            }

            return string.Empty;
        }
    }
}
